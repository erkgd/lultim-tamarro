using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaJugador : SistemaVida
{
    // Componentes UI y efectos
    [SerializeField] private VidaUI vidaUI;
    [SerializeField] private Cortinilla cortinilla;
    [SerializeField] private float tempsReviure = 5f;
    
    // Propiedades internas para gestionar la vida
    [SerializeField] private int vidaMaxima = 10;
    [SerializeField] private int vidaActual = 6;
    
    // Eventos para comunicación (eliminados los duplicados con la clase base)
    public event Action OnVidaCanviada;
    
    // Referencias a componentes
    private Animator animator;
    private InvencibilitatJugador invencibilitat;
    
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    
    public override void Awake()
    {
        base.Awake();
        
        // Obtener referencias
        animator = GetComponent<Animator>();
        invencibilitat = GetComponent<InvencibilitatJugador>();
        
        // Buscar dependencias
        if (vidaUI == null) vidaUI = FindObjectOfType<VidaUI>();
        if (cortinilla == null) cortinilla = FindObjectOfType<Cortinilla>();
    }
    
    private void Start()
    {
        ActualitzarUI();
    }
    
    public bool EsViu()
    {
        return vidaActual > 0;
    }
    
    public void IncrementarVida(int quantitat)
    {
        if (quantitat <= 0) return;
        
        vidaActual = Mathf.Min(vidaActual + quantitat, vidaMaxima);
        
        // Notificar cambios
        NotificarCanviVida();
    }
    
    public void DecrementarVida(int quantitat)
    {
        // Add null check for invencibilitat
        bool isInvencible = invencibilitat != null && invencibilitat.EsInvencible;
        
        if (quantitat <= 0 || isInvencible || !EsViu()) return;
        
        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        
        // Notificar cambios
        NotificarCanviVida();
        
        // Activar invencibilidad si está disponible
        if (invencibilitat != null)
        {
            invencibilitat.ActivarInvencibilitat();
        }
        
        // Si la vida llega a 0, iniciar secuencia de muerte
        if (vidaActual <= 0)
        {
            StartCoroutine(Morir());
        }
    }
    
    public override IEnumerator Morir()
    {
        // Mostrar cortinilla
        if (cortinilla != null)
        {
            cortinilla.MostrarCortinilla();
        }
        
        // Configurar animación y estado
        if (animator != null)
        {
            animator.SetBool("senseVida", true);
        }
        
        // Notificar muerte para desactivar controles
        InvocarMuerte();
        
        // Esperar tiempo de reanimación
        yield return new WaitForSeconds(tempsReviure);
        
        // Revivir
        ReviureJugador();
    }
    
    public void ReviureJugador()
    {
        vidaActual = vidaMaxima;
        
        // Actualizar animación y estado
        if (animator != null)
        {
            animator.SetBool("senseVida", false);
        }
        
        // Notificar reanimación para activar controles
        InvocarRevivir();
        
        // Mostrar cortinilla de nuevo (efecto visual)
        if (cortinilla != null)
        {
            cortinilla.MostrarCortinilla();
        }
        
        // Notificar cambios
        NotificarCanviVida();
    }
    
    protected override void NotificarCanviVida()
    {
        base.NotificarCanviVida();
        ActualitzarUI();
        OnVidaCanviada?.Invoke();
    }
    
    private void ActualitzarUI()
    {
        if (vidaUI != null)
        {
            vidaUI.UpdateHealth(vidaActual);
        }
    }
    
    public override void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }
}