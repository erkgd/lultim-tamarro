using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Jugador : Personatge, IMovible
{
    [Header("Referències")]
    private CharacterController characterController;
    private VidaUI vidaUI;
    private Cortinilla cortinilla;
    private BoxCollider boxColliderAtac;
    [SerializeField] private ParticleSystem efecteInvencibilitat;

    [Header("Invencibilitat")]
    [SerializeField] private float tempsInvencibilitat = 1.5f;
    private bool esInvencible = false;

    [Header("Moviment")]
    [SerializeField] private float velocitat = 5f;
    [SerializeField] private float velocitatRotacio = 120f;
    [SerializeField] private float velocitatCorrer = 10f;
    private float ySpeed;
    private Vector3 direccioMoviment;
    private Vector3 impulsoExterno = Vector3.zero; // Vector per a knockback

    [Header("Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;
    [SerializeField] private float tempsAtac = 0.3f;
    [SerializeField] private float duracioKnockback = 0.25f;
    private float comptadorAtacs = 0f;

    private AtacAEnemics atacAEnemics; // Referencia al componente AtacAEnemics

    public float Velocitat { get; set; } // Implementing the missing interface member

    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        boxColliderAtac = GetComponent<BoxCollider>();
        
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;

        // Si no s'ha assignat el sistema de partícules, en creem un
        if (efecteInvencibilitat == null)
        {
            // Buscar si ja existeix
            efecteInvencibilitat = GetComponentInChildren<ParticleSystem>();
            
            // Si no existeix, crear-ne un
            if (efecteInvencibilitat == null)
            {
                GameObject efectoObj = new GameObject("EfecteInvencibilitat");
                efectoObj.transform.SetParent(transform);
                efectoObj.transform.localPosition = Vector3.up * 0.5f; // A mitja alçada del personatge
                
                efecteInvencibilitat = efectoObj.AddComponent<ParticleSystem>();
                
                // Sistema de partícules - COLOR GROC
                var main = efecteInvencibilitat.main;
                main.loop = true;
                main.startLifetime = 1.0f;
                main.startSpeed = 0.5f;
                main.startSize = 0.2f; //Tamany de les partícules
                main.startColor = Color.yellow; // Color groc
                
                // Emissor de partícules
                var emission = efecteInvencibilitat.emission;
                emission.rateOverTime = 40; // Més partícules
                
                // Forma (esfera al voltant del personatge)
                var shape = efecteInvencibilitat.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1.0f;
                shape.radiusThickness = 0.0f; // Emetre des de la superfície
                
                // Moviment de les partícules
                var velocity = efecteInvencibilitat.velocityOverLifetime;
                velocity.orbitalY = 1.0f;
                
                // Color groc i transparència
                var colorOverLifetime = efecteInvencibilitat.colorOverLifetime;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(Color.yellow, 0.0f), // Groc brillant
                        new GradientColorKey(new Color(1f, 0.7f, 0.0f), 1.0f) // Taronja daurat
                    },
                    new GradientAlphaKey[] { 
                        new GradientAlphaKey(0.9f, 0.0f), 
                        new GradientAlphaKey(0.0f, 1.0f) 
                    }
                );
                colorOverLifetime.color = gradient;
                
                // Renderer per assegurar el color correcte
                var renderer = efecteInvencibilitat.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.material.color = Color.yellow;
            }
            
            // Desactivem inicialment
            efecteInvencibilitat.Stop();
        }
    }

    protected override void Start()
    {
        base.Start();
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);

        cortinilla = FindObjectOfType<Cortinilla>();

        // Use a protected method to subscribe to the event
        SubscribeToQuanCanviVida(OnCanviVidaHandler);

        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);

        atacAEnemics = GetComponent<AtacAEnemics>();
        if (atacAEnemics == null)
            Debug.LogWarning("No se encontró AtacAEnemics en el GameObject");
    }

    // New event handler method
    private void OnCanviVidaHandler()
    {
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    void Update()
    {
        if (!EsViu()) return;

        // Control del moviment
        ProcessarEntrada();
        
        // Control de l'atac
        if (Input.GetMouseButtonDown(0) && !atacant)
        {
            Atacar();
        }
    }

    private void ProcessarEntrada()
    {
        // Obtenim els inputs d'Horizontal i Vertical
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Nova direcció de moviment que barreja ambdós eixos
        Vector3 movementDirection = new(h + v, 0, v - h);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Si Shift està premut, duplica la velocitat
        float currentSpeed = velocitat;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("EstaCorrent", true);
            currentSpeed = velocitatCorrer;
        }
        else
        {
            animator.SetBool("EstaCorrent", false);
        }

        // Assignar la direcció normalitzada
        movementDirection.Normalize();
        direccioMoviment = movementDirection;

        // Calcular velocitat final
        float finalSpeed = magnitude * currentSpeed;

        // Actualitzar velocitat vertical (gravetat)
        ySpeed += Physics.gravity.y * Time.deltaTime;

        // Crear vector de velocitat final
        Vector3 velocity = direccioMoviment * finalSpeed;
        velocity.y = ySpeed;
        
        // Afegir l'impuls extern si existeix (knockback)
        if (impulsoExterno.magnitude > 0.1f)
        {
            velocity += impulsoExterno;
        }

        // Moure el personatge
        characterController.Move(velocity * Time.deltaTime);

        // Rotació cap a la direcció del moviment
        if (direccioMoviment != Vector3.zero)
        {
            animator.SetBool("EstaMoviment", true);

            // Rotació instantània cap a la direcció del moviment
            Quaternion toRotation = Quaternion.LookRotation(direccioMoviment, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, velocitatRotacio * 2 * Time.deltaTime);
        }
        else
        {
            animator.SetBool("EstaMoviment", false);
        }
    }

    public override void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0 || esInvencible) return;

        base.DecrementarVida(quantitat, font);

        // Activamos invencibilidad
        StartCoroutine(PeriodeInvencibilitat());

        if (vidaActual <= 0 && cortinilla != null)
            cortinilla.MostrarCortinilla();
    }

    private IEnumerator PeriodeInvencibilitat()
    {
        esInvencible = true;
        
        // Assegurem que les partícules són grogues abans d'activar-les
        if (efecteInvencibilitat != null)
        {
            // Actualitzem el color per si s'ha canviat
            var main = efecteInvencibilitat.main;
            main.startColor = Color.yellow; // Color groc
            
            // Activem les partícules
            efecteInvencibilitat.Play();
        }
        
        // Activem l'animació d'invencibilitat
        if (animator != null)
            animator.SetTrigger("Invencibilitat");
        
        yield return new WaitForSeconds(tempsInvencibilitat);
        
        // Desactivem l'efecte de partícules
        if (efecteInvencibilitat != null)
            efecteInvencibilitat.Stop();
        
        esInvencible = false;
    }

    protected override IEnumerator Morir()
    {
        if (animator != null)
            animator.SetBool("senseVida", true);

        // Desactivamos controles
        enabled = false;

        // Esperamos a que la animación termine
        yield return new WaitForSeconds(5f);

        // Revivimos al jugador
        vidaActual = vidaMaxima;
        animator.SetBool("senseVida", false);
        enabled = true;

        // Actualizamos la UI
        InvokeQuanCanviVida(); // Use the protected method to invoke the event
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    protected override IEnumerator ExecutarAtac()
    {
        atacant = true;
        
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = true;
            
        animator.SetTrigger("TrAtac");

        yield return new WaitForSeconds(tempsAtac);

        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;
            
        atacant = false;
    }

    public void RecibirKnockback(Vector3 direccion, float fuerza)
    {
        impulsoExterno = direccion.normalized * fuerza;
        StartCoroutine(DisminuirImpulso());
    }

    private IEnumerator DisminuirImpulso()
    {
        float duracion = duracioKnockback;
        float tiempoInicial = Time.time;
        Vector3 impulsoInicial = impulsoExterno;
        
        while (Time.time - tiempoInicial < duracion)
        {
            float factor = 1 - ((Time.time - tiempoInicial) / duracion);
            impulsoExterno = impulsoInicial * factor;
            yield return null;
        }
        
        impulsoExterno = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (atacant && other.CompareTag("Enemy"))
        {
            // Apliquem dany a l'enemic
            IVida vidaEnemic = other.GetComponent<IVida>();
            if (vidaEnemic != null)
            {
                vidaEnemic.DecrementarVida(dany, gameObject.name);
                
                // Aplicar knockback a l'enemic (opcional)
                Vector3 direccioKnockback = (other.transform.position - transform.position).normalized;
                direccioKnockback.y = 0; // Mantenim el knockback horitzontal
                
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(direccioKnockback * forcaKnockback, ForceMode.Impulse);
                }
            }
        }
    }

    public void AturarMoviment()
    {
        direccioMoviment = Vector3.zero;
    }

    public void Moure(Vector3 direccio)
    {
        // Implement the movement logic here
        transform.position += direccio;
    }
}