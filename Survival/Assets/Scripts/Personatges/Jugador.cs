using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Jugador : Personatge
{
    // Referencias compartidas - se mantienen en la clase principal
    [Header("Referències")]
    [SerializeField] private ParticleSystem efecteInvencibilitat;
    private CharacterController characterController;
    private VidaUI vidaUI;
    private Cortinilla cortinilla;
    private BoxCollider boxColliderAtac;

    // Componentes de las clases modularizadas
    private MovimentJugador movimentJugador;
    private AtacJugador atacJugador;
    private InvencibilitatJugador invencibilitatJugador;

    [Header("Configuració Moviment")]
    [SerializeField] private float velocitat = 5f;
    [SerializeField] private float velocitatRotacio = 120f;
    [SerializeField] private float velocitatCorrer = 10f;
    [SerializeField] private float forcaGravetat = 1f;
    [SerializeField] private float duracioKnockback = 0.25f;

    [Header("Configuració Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 0.6f;
    [SerializeField] private float tempsAtac = 0.05f;
    [SerializeField] private float angleVisioAtac = 60f;
    [SerializeField] private int danyAtac = 1;

    [Header("Configuració Invencibilitat")]
    [SerializeField] private float tempsInvencibilitat = 1.7f;
    [SerializeField] private Color colorEfecteInvencibilitat = Color.yellow;
    [SerializeField] private float midaParticules = 0.2f;
    [SerializeField] private float velocitatParticules = 0.5f;
    [SerializeField] private float taxaEmissioParticules = 40f;
    [SerializeField] private float radiEfecte = 1.0f;

    public CharacterController CharacterController => characterController;
    public Animator AnimatorJugador => animator;
    public BoxCollider BoxColliderAtac => boxColliderAtac;
    public float Velocitat { get; set; }

    // Propiedades públicas para acceder a miembros protegidos
    public bool Atacant { get => atacant; set => atacant = value; }
    
    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        boxColliderAtac = GetComponent<BoxCollider>();
        
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;

        // Inicializar los componentes modulares
        movimentJugador = gameObject.AddComponent<MovimentJugador>();
        atacJugador = gameObject.AddComponent<AtacJugador>();
        invencibilitatJugador = gameObject.AddComponent<InvencibilitatJugador>();
        
        // Configurar los componentes con los valores serializados
        ConfigurarComponents();
    }

    private void ConfigurarComponents()
    {
        // Configurar MovimentJugador
        movimentJugador.ConfigurarMoviment(velocitat, velocitatRotacio, velocitatCorrer, forcaGravetat, duracioKnockback);

        // Configurar AtacJugador
        atacJugador.ConfigurarAtac(rangAtacar, tempsEntreAtacs, tempsAtac, angleVisioAtac, danyAtac);

        // Configurar InvencibilitatJugador
        invencibilitatJugador.ConfigurarInvencibilitat(
            tempsInvencibilitat,
            colorEfecteInvencibilitat,
            midaParticules,
            velocitatParticules,
            taxaEmissioParticules,
            radiEfecte
        );
        invencibilitatJugador.ConfigurarEfecteInvencibilitat(efecteInvencibilitat);
    }

    protected override void Start()
    {
        base.Start();
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);

        cortinilla = FindObjectOfType<Cortinilla>();

        // Subscribirse a eventos
        SubscribeToQuanCanviVida(OnCanviVidaHandler);

        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    // Manejador de eventos para cambios de vida
    private void OnCanviVidaHandler()
    {
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    void Update()
    {
        if (!EsViu()) return;

        // Delegamos el movimiento al componente especializado
        movimentJugador.ActualitzarMoviment();

        // Control de ataque
        atacJugador.ActualitzarAtac();
    }

    public override void Atacar()
    {
        atacJugador.IniciarAtac();
    }

    // Corregir la firma del método Morir para que devuelva IEnumerator
    protected override IEnumerator Morir()
    {
        // Implementación directa como corrutina en lugar de llamar a otro método
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
        InvokeQuanCanviVida(); 
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    public void RebreKnockback(Vector3 direccio, float forca)
    {
        movimentJugador.AplicarKnockback(direccio, forca);
    }

    public void Moure()
    {
        movimentJugador.Moure();
    }

    public void AturarMoviment()
    {
        movimentJugador.AturarMoviment();
    }

    // Això cap a SistemaVidaJugador.cs
    /* public override void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0 || invencibilitatJugador.EsInvencible) return;

        base.DecrementarVida(quantitat, font);

        // Activamos invencibilidad
        invencibilitatJugador.ActivarInvencibilitat();

        if (vidaActual <= 0 && cortinilla != null)
        {
            cortinilla.MostrarCortinilla();
            Morir(); // Llamamos al método protected
        }
    } */

    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }

    protected override IEnumerator ExecutarAtac()
    {
        // Delegamos la lógica de ataque al componente especializado
        yield return StartCoroutine(atacJugador.ExecutarAtac());
    }
}