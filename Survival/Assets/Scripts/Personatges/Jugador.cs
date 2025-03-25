using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Jugador : Personatge, IMovible, IAtacant
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
        
        // Configurar el efecto de invencibilidad para el componente de invencibilidad
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

    public override void DecrementarVida(int quantitat, string font)
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
    }

    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }

    protected override IEnumerator ExecutarAtac()
    {
        // Delegamos la lógica de ataque al componente especializado
        yield return StartCoroutine(atacJugador.ExecutarAtac());
    }
    public override void IncrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual += quantitat;
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Actualitzem UI
        NotificarCanviVida(); // Use the protected method instead of direct event invocation
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }
}