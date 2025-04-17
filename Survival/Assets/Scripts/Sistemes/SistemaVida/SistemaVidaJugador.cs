using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaJugador : SistemaVida
{
    // Componentes UI y efectos
    [SerializeField] private VidaUI vidaUI;
    [SerializeField] private Cortinilla cortinilla;
    [SerializeField] private float tempsReviure = 1f;

    // Propiedades internas para gestionar la vida
    [SerializeField] private int vidaMaxima = 24;
    [SerializeField] private int vidaActual = 12;
    
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
        
        // Activar invencibilidad usando el Singleton i si el perk de resistència està desbloquejat   
        if (InvencibilitatJugador.Instance != null && SistemaPerks.Instance.EstaDesbloquejada(1))
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
        // Configurar animación y estado
        if (animator != null)
        {
            animator.SetBool("senseVida", true);
        }
        
        // Notificar muerte para desactivar controles
        InvocarMuerte();
        // Mostrar cortinilla
        if (cortinilla != null)
        {
            cortinilla.MostrarCortinilla();
            // Esperamos un momento para que se vea la animación
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogWarning("No se encontró la referencia a la cortinilla");
            yield return null;
        }
        // Esperar un momento antes de teleportar
        yield return new WaitForSeconds(tempsReviure);
        
        // IMPORTANTE: teleportamos pero NO deshacemos la cortinilla aquí
        // ya que estaríamos intentando usar la cortinilla de la escena anterior
        TeleportarAlHub();
        
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
    
    public void TeleportarAlHub()
    {
        PosicionadorJugador posicionador = GetComponent<PosicionadorJugador>();

        if (posicionador == null)
        {
            // Si no existe el componente, lo añadimos
            posicionador = gameObject.AddComponent<PosicionadorJugador>();
            Debug.Log("Se ha añadido automáticamente el componente PosicionadorJugador al jugador");
        }
          if (posicionador != null)
        {            // Guardar información del punto de spawn usando SistemaPerks
            if (SistemaPerks.Instance != null)
            {
                // Guardamos un tag identificativo del punto de spawn - usamos "Hub" como tag
                SistemaPerks.Instance.GuardarValor("LastSpawnPoint", "Hub");
                Debug.Log("Se guardó el punto de spawn a través de SistemaPerks");
                
                // También guardamos la posición del punto de spawn del Hub
                SistemaPerks.Instance.GuardarPosicioTeleport(TPConstants.HUB_SPAWN_POINT);
            }
            else
            {
                Debug.LogWarning("No se encontró SistemaPerks, usando PlayerPrefs directamente como fallback");
                PlayerPrefs.SetString("LastSpawnPoint", "Hub");
                PlayerPrefs.Save();
            }
            
            posicionador.IniciarTeleport("Hub", TPConstants.HUB_SCENE);
            Debug.Log("Teleportando jugador al Hub...");
        }
        else
        {
            Debug.LogError("No se pudo crear el componente PosicionadorJugador en el jugador");
        }
    }
}