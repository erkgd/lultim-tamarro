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

    [Header("Atac")]
    [SerializeField] private int dany = 2;
    [SerializeField] private float tempsAtac = 0.3f;
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

    void Awake()
    {
        // Inicialització dels components
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        boxColliderAtac = GetComponent<BoxCollider>();
        
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;
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
        // Obtenció dels inputs
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // Direcció del moviment (implementació original)
        Vector3 direccio = new Vector3(h, 0, v);
        float magnitud = Mathf.Clamp01(direccio.magnitude);
        
        // Aplicació de la velocitat segons si es corre o no
        float velocitatActual = Input.GetKey(KeyCode.LeftShift) ? velocitatCorrer : velocitat;
        animator.SetBool("EstaCorrent", Input.GetKey(KeyCode.LeftShift) && magnitud > 0.1f);
        
        // Normalització i càlcul final
        direccioMoviment = direccio.normalized;
        
        // Rotació del personatge cap a la direcció si s'està movent
        if (magnitud > 0.1f)
        {
            animator.SetBool("EstaMoviment", true);
            float targetAngle = Mathf.Atan2(direccioMoviment.x, direccioMoviment.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, velocitatRotacio * Time.deltaTime);
        }
        else
        {
            animator.SetBool("EstaMoviment", false);
        }
        
        // Aplicació de la gravetat
        if (characterController.isGrounded)
        {
            ySpeed = -0.5f; // Petit valor per mantenir-lo enganxat al terra
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
        
        // Càlcul final de la velocitat
        Vector3 velocity = direccioMoviment * magnitud * velocitatActual;
        velocity.y = ySpeed;
        
        // Moviment del personatge
        characterController.Move(velocity * Time.deltaTime);
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
        
        // Activem l'animació d'invencibilitat
        if (animator != null)
            animator.SetTrigger("Invencibilitat");
        
        yield return new WaitForSeconds(tempsInvencibilitat);
        
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
            }
        }
    }
}