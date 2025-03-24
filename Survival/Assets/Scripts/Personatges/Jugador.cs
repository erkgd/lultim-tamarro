using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Jugador : MonoBehaviour, IVida, IAtacant, IMovible
{
    [Header("Referències")]
    private Animator animator;
    private CharacterController characterController;
    private VidaUI vidaUI;
    private Cortinilla cortinilla;
    private BoxCollider boxColliderAtac;
    [SerializeField] private ParticleSystem efecteInvencibilitat;

    [Header("Vida")]
    [SerializeField] private int vidaActual;
    [SerializeField] private int vidaMaxima = 12;

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
    [SerializeField] private int dany = 1;
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;
    [SerializeField] private float tempsAtac = 0.3f;
    [SerializeField] private float forcaKnockback = 10f; // Força de l'empenta 
    [SerializeField] private float duracioKnockback = 0.25f; // Duració de l'efecte
    private float comptadorAtacs = 0f;
    private bool atacant = false;

    // Event vida
    public event Action QuanCanviVida;

    // Propietats interfície IVida
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;

    // Propietats interfície IAtacant
    public int Dany => dany;
    public bool EstaAtacant() => atacant;

    // Propietats interfície IMovible
    public float Velocitat => velocitat;

    private AtacAEnemics atacAEnemics; // Referencia al componente AtacAEnemics


    void Awake()
    {
        // Inicialització dels components
        animator = GetComponent<Animator>();
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

    void Start()
    {
        // Cerca de components globals
        vidaUI = FindObjectOfType<VidaUI>();
        cortinilla = FindObjectOfType<Cortinilla>();
        
        // Inicialització de l'estat
        vidaActual = vidaMaxima;
        
        // Actualització de l'UI
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);

        atacAEnemics = GetComponent<AtacAEnemics>();
        if (atacAEnemics == null)
        {
            Debug.LogWarning("No se encontró AtacAEnemics en el GameObject");
        }
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

    #region IVida
    public bool EsViu()
    {
        return vidaActual > 0;
    }

    public void IncrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual += quantitat;
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Actualitzem UI
        QuanCanviVida?.Invoke();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    public void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0 || esInvencible) return;

        vidaActual -= quantitat;
        
        // Activem l'animació de rebre mal
        if (animator != null)
            animator.SetTrigger("TrRepMal");
        
        // Activem el període d'invencibilitat
        StartCoroutine(PeriodeInvencibilitat());

        // Si la vida arriba a 0 o menys, iniciem el procés de mort
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Actualitzem UI
        QuanCanviVida?.Invoke();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
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

    private IEnumerator Morir()
    {
        if (animator != null)
            animator.SetBool("senseVida", true);
        
        // Activem la cortinilla
        if (cortinilla != null)
            cortinilla.MostrarCortinilla();

        // Desactivem controls
        enabled = false;
        
        // Esperem a que l'animació acabi
        yield return new WaitForSeconds(5f);

        // Revivim el jugador
        vidaActual = vidaMaxima;
        animator.SetBool("senseVida", false);
        enabled = true;
        
        // Actualitzem UI
        QuanCanviVida?.Invoke();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }
    #endregion

    #region IAtacant
    public void Atacar()
    {
        StartCoroutine(ExecutarAtac());

        // Detecció per si el personatje ataca a enemic
        if (atacAEnemics != null)
            atacAEnemics.DetectarCop();
    }

    private IEnumerator ExecutarAtac()
    {
        atacant = true;
        
        // Activem el collider d'atac
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = true;
            
        // Activem l'animació d'atac
        animator.SetTrigger("TrAtac");

        // Esperem el temps d'atac
        yield return new WaitForSeconds(tempsAtac);

        // Desactivem el collider d'atac
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;
            
        atacant = false;

        
    }
    #endregion

    #region IMovible
    public void Moure(Vector3 direccio)
    {
        direccioMoviment = direccio;
    }

    public void AturarMoviment()
    {
        direccioMoviment = Vector3.zero;
    }
    #endregion
    
    // Mètode per rebre knockback des d'enemics
    public void RecibirKnockback(Vector3 direccion, float fuerza)
    {
        impulsoExterno = direccion.normalized * fuerza;
        StartCoroutine(DisminuirImpulso());
    }
    
    // Mètode per disminuir gradualment l'impuls de knockback
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

    // Gestió de les col·lisions per a l'atac
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
}