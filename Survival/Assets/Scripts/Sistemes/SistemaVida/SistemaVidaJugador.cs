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
    [SerializeField] private int vidaMaxima = 48;
    [SerializeField] private int vidaActual = 24;
    
    // Eventos para comunicación (eliminados los duplicados con la clase base)
    public event Action OnVidaCanviada;
    
    // Referencias a componentes
    private Animator animator;
    
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    
    public override void Awake()
    {
        base.Awake();
        
        // Obtener referencias
        animator = GetComponent<Animator>();
        
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
        // Comprobación detallada con logs
        if (quantitat <= 0) {
            Debug.Log("No se aplicó daño porque la cantidad es 0 o negativa");
            return;
        }
        
        // Comprobar invencibilidad con el Singleton
        if (InvencibilitatJugador.Instance != null && InvencibilitatJugador.Instance.EsInvencible) {
            Debug.Log("No se aplicó daño porque el jugador está invencible");
            return;
        }
        
        if (!EsViu()) {
            Debug.Log("No se aplicó daño porque el jugador no está vivo");
            return;
        }
        
        // Log para depuración
        Debug.Log($"Vida antes del daño: {vidaActual}");
        
        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        
        // Log para depuración
        Debug.Log($"Vida después del daño: {vidaActual}, Cantidad de daño: {quantitat}");
        
        // Notificar cambios
        NotificarCanviVida();
        
        // Activar invencibilidad usando el Singleton
        if (InvencibilitatJugador.Instance != null)
        {
            Debug.Log("Activando invencibilidad después del daño");
            InvencibilitatJugador.Instance.ActivarInvencibilitat();
        }
        else
        {
            Debug.LogWarning("No se puede activar la invencibilidad porque no existe una instancia del Singleton");
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