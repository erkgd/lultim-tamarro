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
    private Animator animator;
    private VidaUI vidaUI;
    private Cortinilla cortinilla;
    private BoxCollider boxColliderAtac;
    
    // Variables para implementar las propiedades abstractas
    [SerializeField] protected int vidaMaxima = 10;
    [SerializeField] protected int vidaActual = 6;
    [SerializeField] protected int danyAtac = 1;
    [SerializeField] protected float forcaKnockback = 5f;
    protected bool atacant = false;

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

    [Header("Configuració Invencibilitat")]
    [SerializeField] private float tempsInvencibilitat = 1.7f;
    [SerializeField] private Color colorEfecteInvencibilitat = Color.yellow;
    [SerializeField] private float midaParticules = 0.2f;
    [SerializeField] private float velocitatParticules = 0.5f;
    [SerializeField] private float taxaEmissioParticules = 40f;
    [SerializeField] private float radiEfecte = 1.0f;

    // Implementación de propiedades abstractas
    public override int VidaActual => vidaActual;
    public override int VidaMaxima => vidaMaxima;
    public override int Dany => danyAtac;
    public override float ForcaKnockback => forcaKnockback;

    public CharacterController CharacterController => characterController;
    public Animator AnimatorJugador => animator;
    public BoxCollider BoxColliderAtac => boxColliderAtac;
    public float Velocitat { get; set; }

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
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
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);

        cortinilla = FindObjectOfType<Cortinilla>();

        // Subscribirse a eventos
        SubscribeToQuanCanviVida(OnCanviVidaHandler);
    }

    // Manejador de eventos para cambios de vida
    private void OnCanviVidaHandler()
    {
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    void Update()
    {
        if (!SistemaVidaJugador.EsViu()) return;

        movimentJugador.ActualitzarMoviment();

        // Control de ataque
        atacJugador.ActualitzarAtac();
    }

    public override bool EstaAtacant()
    {
        return atacant;
    }

    public override void Atacar()
    {
        atacJugador.IniciarAtac();
    }

    public void IncrementarVida(int quantitat)
    {
        sistemaVida.IncrementarVida(quantitat);
    }

    public override void DecrementarVida(int quantitat)
    {
        sistemaVida.DecrementarVida(quantitat);
    }

    protected override void NotificarCanviVida()
    {
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
        
        InvokeQuanCanviVida();
    }

    protected override void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }

    protected override void InvokeQuanCanviVida()
    {
        QuanCanviVida?.Invoke();
    }

    protected override IEnumerator Morir()
    {
        if (animator != null)
            animator.SetBool("senseVida", true);

        // Desactivamos controles
        enabled = false;

        // Esperamos a que la animación de la cortinilla termine (esto deberiamos definirlo en una constante).
        yield return new WaitForSeconds(5f);

        // Revivimos al jugador
        vidaActual = vidaMaxima;
        animator.SetBool("senseVida", false);
        enabled = true;

        // Actualizamos la UI
        NotificarCanviVida();
    }

    protected override IEnumerator ExecutarAtac()
    {
        // Delegamos la lógica de ataque al componente especializado
        yield return StartCoroutine(atacJugador.ExecutarAtac());
    }

    public void Moure()
    {
        movimentJugador.Moure();
    }

    public void AturarMoviment()
    {
        movimentJugador.AturarMoviment();
    }
}