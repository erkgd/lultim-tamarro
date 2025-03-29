using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SistemaVidaJugador))]
public class Jugador : Personatge
{
    [Header("Referències")]
    [SerializeField] private ParticleSystem efecteInvencibilitat;
    private CharacterController characterController;
    private Animator animator;
    private BoxCollider boxColliderAtac;
    private SistemaVidaJugador sistemaVida;
    
    // Variables para implementar las propiedades abstractas
    [SerializeField] private int danyAtac = 1;
    [SerializeField] private float forcaKnockback = 5f;
    private bool atacant = false;

    // Componentes modularizados
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
    public override int VidaActual => sistemaVida.VidaActual;
    public override int VidaMaxima => sistemaVida.VidaMaxima;
    public override int Dany => danyAtac;
    public override float ForcaKnockback => forcaKnockback;

    public CharacterController CharacterController => characterController;
    public Animator AnimatorJugador => animator;
    public BoxCollider BoxColliderAtac => boxColliderAtac;

    // Propiedad para acceder al estado de ataque
    public bool Atacant { get => atacant; set => atacant = value; }
    
    protected override void Awake()
    {
        // Inicializar componentes
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        boxColliderAtac = GetComponent<BoxCollider>();
        sistemaVida = GetComponent<SistemaVidaJugador>();
        
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;

        // Inicializar componentes modulares
        movimentJugador = gameObject.AddComponent<MovimentJugador>();
        atacJugador = gameObject.AddComponent<AtacJugador>();
        invencibilitatJugador = gameObject.AddComponent<InvencibilitatJugador>();
        
        // Configurar components
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
        // Suscribirse a eventos del sistema de vida
        sistemaVida.OnVidaCanviada += OnVidaCanviada;
        sistemaVida.OnMuerte += DesactivarControl;
        sistemaVida.OnRevivir += ActivarControl;
    }
    
    private void OnVidaCanviada()
    {
        // Cambiado para no invocar QuanCanviVida directamente
        // QuanCanviVida?.Invoke(); <- Error, los eventos solo pueden aparecer al lado izquierdo de += o -=
        // En su lugar, notificar a los suscriptores usando un método propio
        NotificarCambiVida();
    }
    
    // Método adicional para notificar a los suscriptores
    protected void NotificarCambiVida()
    {
        // Usar el método protegido de la clase base en lugar de reflexión
        InvocarQuanCanviVida();
    }
    
    private void DesactivarControl()
    {
        enabled = false;
    }
    
    private void ActivarControl()
    {
        enabled = true;
    }

    void Update()
    {
        if (!sistemaVida.EsViu()) return;

        movimentJugador.ActualitzarMoviment();
        atacJugador.ActualitzarAtac();
    }

    // Métodos que delegan al sistema de vida
    public override void DecrementarVida(int quantitat, string font = "")
    {
        sistemaVida.DecrementarVida(quantitat);
    }
    
    public void IncrementarVida(int quantitat)
    {
        sistemaVida.IncrementarVida(quantitat);
    }

    public override IEnumerator Morir()
    {
        // Delegar al sistema de vida
        return sistemaVida.Morir();
    }

    public override bool EstaAtacant()
    {
        return atacant;
    }

    public override void Atacar()
    {
        atacJugador.IniciarAtac();
    }

    public override IEnumerator ExecutarAtac()
    {
        return atacJugador.ExecutarAtac();
    }
    
    // Método para llamar al método protegido desde otros componentes
    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }
    
    public void RebreKnockback(Vector3 direccio, float forca)
    {
        movimentJugador.AplicarKnockback(direccio, forca);
    }
    
    // Asegurarnos de desuscribirse de los eventos al destruir el objeto
    public void OnDestroy()
    {
        if (sistemaVida != null)
        {
            sistemaVida.OnVidaCanviada -= OnVidaCanviada;
            sistemaVida.OnMuerte -= DesactivarControl;
            sistemaVida.OnRevivir -= ActivarControl;
        }
    }
}