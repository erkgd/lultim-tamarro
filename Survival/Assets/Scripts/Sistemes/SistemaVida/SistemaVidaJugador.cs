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
        // Configurar animación y estado
        if (animator != null)
        {
            animator.SetBool("senseVida", true);
        }
        
        // Notificar muerte para desactivar controles
        InvocarMuerte();
        
        // Siempre usamos la cortinilla gestionada desde el SistemaVidaJugador
        bool usarEfectoCortinilla = true;
        
        // Buscamos la cortinilla si no la tenemos ya asignada
        if (cortinilla == null)
        {
            cortinilla = FindObjectOfType<Cortinilla>();
            if (cortinilla == null)
            {
                Debug.LogError("No se encontró la cortinilla en la escena. Asegúrate de que existe en UI/ImageCortinilla");
            }
        }
        
        // Mostrar cortinilla para cerrar la escena actual
        if (cortinilla != null)
        {
            // Aseguramos que la cortinilla está lista para usarse
            cortinilla.ResetearCortinilla();
            // Activamos la cortinilla (cierre)
            cortinilla.MostrarCortinilla();
            Debug.Log("SistemaVidaJugador: Cortinilla activada al morir el jugador");
            // Esperamos un momento para que se vea la animación de la cortinilla
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogWarning("SistemaVidaJugador: No se pudo mostrar la cortinilla. Verificar que existe en la escena.");
            yield return new WaitForSeconds(0.2f);
        }
        
        // Esperar un momento antes de teleportar
        yield return new WaitForSeconds(tempsReviure);
        
        // IMPORTANTE: teleportamos pero NO deshacemos la cortinilla aquí
        // ya que estaríamos intentando usar la cortinilla de la escena anterior
        // La cortinilla se abrirá en la nueva escena mediante el PosicionadorJugador
        TeleportarAlHub(usarEfectoCortinilla);
        
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
    
    public void TeleportarAlHub(bool usarCortinilla = true)
    {
        PosicionadorJugador posicionador = GetComponent<PosicionadorJugador>();

        if (posicionador == null)
        {
            // Si no existe el componente, lo añadimos
            posicionador = gameObject.AddComponent<PosicionadorJugador>();
            Debug.Log("Se ha añadido automáticamente el componente PosicionadorJugador al jugador");
        }
        if (posicionador != null)
        {
            PlayerPrefs.Save();
            // Iniciamos el teleport
            posicionador.IniciarTeleport(TPConstants.HUB_SPAWN_POINT, TPConstants.HUB_SCENE);
            Debug.Log($"Teleportando jugador al Hub... (Cortinilla: {(usarCortinilla ? "Activada" : "Desactivada")})");
        }
        else
        {
            Debug.LogError("No se pudo crear el componente PosicionadorJugador en el jugador");
        }
    }
}