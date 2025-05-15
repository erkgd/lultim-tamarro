using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Sistema simplificado para contar enemigos derrotados
/// </summary>
public class SistemaCounter : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaCounter Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private bool guardarContadorEntreSesiones = true;
    [SerializeField] private TextMeshProUGUI contadorTextoUI;

    [Header("Tipos de Enemigos")]
    [SerializeField] private string[] tiposEnemigos = {
        "Blobo",     // el rojo ciclope 0
        "Amanita",   // seta 1
        "Greko",     // volador 2
        "Tatxo"      // pinchitos 3
    };

    [Header("Contadores")]
    [SerializeField] private int enemigosTotalesDerrotados = 0;
    [SerializeField] private int[] contadoresPorTipo;

    // Claves para PlayerPrefs
    private const string KEY_TOTAL_ENEMIGOS = "TotalEnemigos";
    private const string KEY_TIPO_ENEMIGO_BASE = "EnemigosTipo_";

    // Eventos
    public event Action<int> OnEnemigoDerrotado;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Inicializar arrays
            if (contadoresPorTipo == null || contadoresPorTipo.Length != tiposEnemigos.Length)
            {
                contadoresPorTipo = new int[tiposEnemigos.Length];
            }
            
            // Cargar datos guardados si corresponde
            if (guardarContadorEntreSesiones)
            {
                CargarContadores();
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Suscribirse a eventos de muerte de enemigos
        SuscribirseAEventosMuerte();
        
        // Actualizar UI
        ActualizarUI();
    }
    
    private void SuscribirseAEventosMuerte()
    {
        // Buscar todos los enemigos en la escena y suscribirse a sus eventos de muerte
        SistemaVidaEnemic[] enemigos = FindObjectsOfType<SistemaVidaEnemic>();
        foreach (SistemaVidaEnemic enemigo in enemigos)
        {
            // Nos suscribimos al evento de muerte
            enemigo.QuanMoriEnemic += () => RegistrarEnemigoEliminado(ObtenerTipoEnemigo(enemigo.gameObject));
        }
    }
    
    /// <summary>
    /// Registra un enemigo como eliminado
    /// </summary>
    /// <param name="tipoEnemigo">Tipo de enemigo (índice)</param>
    public void RegistrarEnemigoEliminado(int tipoEnemigo)
    {
        // Comprobar que el tipo es válido
        if (tipoEnemigo < 0 || tipoEnemigo >= contadoresPorTipo.Length)
        {
            tipoEnemigo = 0; // Tipo por defecto
        }
        
        // Incrementar contadores
        enemigosTotalesDerrotados++;
        contadoresPorTipo[tipoEnemigo]++;
        
        Debug.Log($"Enemigo eliminado - Tipo: {tiposEnemigos[tipoEnemigo]}, " +
                 $"Total: {enemigosTotalesDerrotados}");
        
        // Guardar datos
        if (guardarContadorEntreSesiones)
        {
            GuardarContadores();
        }
        
        // Notificar
        OnEnemigoDerrotado?.Invoke(enemigosTotalesDerrotados);
        
        // Actualizar UI
        ActualizarUI();
    }
    
    /// <summary>
    /// Determina el tipo de enemigo basándose en su nombre
    /// </summary>
    private int ObtenerTipoEnemigo(GameObject enemigo)
    {
        if (enemigo == null) return 0;
        
        string nombreEnemigo = enemigo.name.ToLower();
        
        for (int i = 0; i < tiposEnemigos.Length; i++)
        {
            if (nombreEnemigo.Contains(tiposEnemigos[i].ToLower()))
            {
                return i;
            }
        }
        
        return 0; // Tipo por defecto
    }
    
    /// <summary>
    /// Actualiza la UI con los contadores actuales
    /// </summary>
    private void ActualizarUI()
    {
        if (contadorTextoUI != null)
        {
            contadorTextoUI.text = $"Enemigos: {enemigosTotalesDerrotados}";
        }
    }
    
    /// <summary>
    /// Guarda todos los contadores en PlayerPrefs
    /// </summary>
    private void GuardarContadores()
    {
        PlayerPrefs.SetInt(KEY_TOTAL_ENEMIGOS, enemigosTotalesDerrotados);
        
        for (int i = 0; i < contadoresPorTipo.Length; i++)
        {
            PlayerPrefs.SetInt(KEY_TIPO_ENEMIGO_BASE + i, contadoresPorTipo[i]);
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Carga todos los contadores desde PlayerPrefs
    /// </summary>
    private void CargarContadores()
    {
        enemigosTotalesDerrotados = PlayerPrefs.GetInt(KEY_TOTAL_ENEMIGOS, 0);
        
        for (int i = 0; i < contadoresPorTipo.Length; i++)
        {
            contadoresPorTipo[i] = PlayerPrefs.GetInt(KEY_TIPO_ENEMIGO_BASE + i, 0);
        }
    }
    
    /// <summary>
    /// Obtiene el total de enemigos derrotados
    /// </summary>
    public int ObtenerTotalEnemigos()
    {
        return enemigosTotalesDerrotados;
    }
    
    /// <summary>
    /// Obtiene el número de enemigos de un tipo específico
    /// </summary>
    public int ObtenerEnemigosPorTipo(int tipoEnemigo)
    {
        if (tipoEnemigo >= 0 && tipoEnemigo < contadoresPorTipo.Length)
        {
            return contadoresPorTipo[tipoEnemigo];
        }
        return 0;
    }
    
    /// <summary>
    /// Reinicia todos los contadores
    /// </summary>
    public void ReiniciarContadores()
    {
        enemigosTotalesDerrotados = 0;
        
        for (int i = 0; i < contadoresPorTipo.Length; i++)
        {
            contadoresPorTipo[i] = 0;
        }
        
        if (guardarContadorEntreSesiones)
        {
            GuardarContadores();
        }
        
        ActualizarUI();
    }
}
