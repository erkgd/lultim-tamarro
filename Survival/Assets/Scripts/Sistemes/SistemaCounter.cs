using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sistema para contar y registrar enemigos derrotados
public class SistemaCounter : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaCounter Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private bool guardarContadorEntreSesiones = true;
    [SerializeField] private TextMeshProUGUI contadorTextoUI; // Opcional: Referencia al elemento de UI para mostrar el contador

    [Header("Tipos de Enemigos")]
    [SerializeField] private string[] tiposEnemigos = {
        "Blobo",     // el rojo ciclope 0
        "Amanita",     // seta 1
        "Greko",  // volador 2
        "Tatxo"   // pinchitos 3

    };

    [Header("Contadores")]
    [SerializeField] private int enemigosTotalesDerrotados = 0;
    [SerializeField] private int[] contadoresPorTipo;

    // Claves para PlayerPrefs
    private const string KEY_TOTAL_ENEMIGOS = "TotalEnemigos";
    private const string KEY_TIPO_ENEMIGO_BASE = "EnemigosTipo_";

    // Eventos
    public event Action<int> OnEnemigoDerrotado; // Se dispara cuando se derrota cualquier enemigo (total)
    public event Action<int, int> OnEnemigoTipoDerrotado; // (tipoEnemigo, cantidad)    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener entre escenas
            
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
        
        // Actualizar UI si existe
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
        
        Debug.Log($"SistemaCounter: Suscrito a {enemigos.Length} enemigos en la escena");
    }
    
    // Este método debe llamarse cuando se cargue una nueva escena
    public void ActualizarSuscripcionesEnemigos()
    {
        SuscribirseAEventosMuerte();
    }
    
    #region Gestión de Enemigos
    
    /// <summary>
    /// Registra un enemigo como eliminado
    /// </summary>
    /// <param name="tipoEnemigo">Tipo de enemigo (índice)</param>
    public void RegistrarEnemigoEliminado(int tipoEnemigo)
    {
        // Comprobar que el tipo es válido
        if (tipoEnemigo < 0 || tipoEnemigo >= contadoresPorTipo.Length)
        {
            Debug.LogWarning($"SistemaCounter: Tipo de enemigo inválido: {tipoEnemigo}");
            tipoEnemigo = 0; // Tipo por defecto
        }
        
        // Incrementar contadores
        enemigosTotalesDerrotados++;
        contadoresPorTipo[tipoEnemigo]++;
        
        Debug.Log($"SistemaCounter: Enemigo eliminado - Tipo: {tiposEnemigos[tipoEnemigo]}, " +
                  $"Total de este tipo: {contadoresPorTipo[tipoEnemigo]}, Total global: {enemigosTotalesDerrotados}");
        
        // Guardar datos si corresponde
        if (guardarContadorEntreSesiones)
        {
            GuardarContadores();
        }
        
        // Disparar eventos
        OnEnemigoDerrotado?.Invoke(enemigosTotalesDerrotados);
        OnEnemigoTipoDerrotado?.Invoke(tipoEnemigo, contadoresPorTipo[tipoEnemigo]);
        
        // Actualizar UI
        ActualizarUI();
    }    /// <summary>
    /// Determina el tipo de enemigo basándose en el nombre del GameObject
    /// </summary>
    private int ObtenerTipoEnemigo(GameObject enemigo)
    {
        string nombreEnemigo = enemigo.name.ToLower();
        
        // Intentar determinar el tipo basado en el nombre
        for (int i = 0; i < tiposEnemigos.Length; i++)
        {
            if (nombreEnemigo.Contains(tiposEnemigos[i].ToLower()))
            {
                return i;
            }
        }
        
        // Comprobar tags específicos
        if (enemigo.CompareTag("Boss") || nombreEnemigo.Contains("boss") || nombreEnemigo.Contains("jefe"))
        {
            return 3; // Jefe
        }
        
        // Devolver el tipo genérico si no se puede determinar
        return 0; // Tipo por defecto: "Normal"
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
        // Guardar total
        PlayerPrefs.SetInt(KEY_TOTAL_ENEMIGOS, enemigosTotalesDerrotados);
        
        // Guardar contadores por tipo
        for (int i = 0; i < contadoresPorTipo.Length; i++)
        {
            PlayerPrefs.SetInt(KEY_TIPO_ENEMIGO_BASE + i, contadoresPorTipo[i]);
        }
        
        PlayerPrefs.Save();
        Debug.Log("SistemaCounter: Contadores guardados en PlayerPrefs");
    }
    
    /// <summary>
    /// Carga todos los contadores desde PlayerPrefs
    /// </summary>
    private void CargarContadores()
    {
        // Cargar total
        enemigosTotalesDerrotados = PlayerPrefs.GetInt(KEY_TOTAL_ENEMIGOS, 0);
        
        // Cargar contadores por tipo
        for (int i = 0; i < contadoresPorTipo.Length; i++)
        {
            contadoresPorTipo[i] = PlayerPrefs.GetInt(KEY_TIPO_ENEMIGO_BASE + i, 0);
        }
        
        Debug.Log("SistemaCounter: Contadores cargados desde PlayerPrefs");
    }
    
    #endregion
    #region API Pública
    
    /// <summary>
    /// Obtiene el total de enemigos derrotados
    /// </summary>
    public int ObtenerTotalEnemigos()
    {
        return enemigosTotalesDerrotados;
    }
    
    /// <summary>
    /// Obtiene el número de enemigos derrotados de un tipo específico
    /// </summary>
    public int ObtenerEnemigosPorTipo(int tipoEnemigo)
    {
        if (tipoEnemigo >= 0 && tipoEnemigo < contadoresPorTipo.Length)
        {
            return contadoresPorTipo[tipoEnemigo];
        }
        return 0;
    }    /// <summary>
    /// Obtiene el nombre del tipo de enemigo
    /// </summary>
    public string ObtenerNombreTipoEnemigo(int tipoEnemigo)
    {
        if (tipoEnemigo >= 0 && tipoEnemigo < tiposEnemigos.Length)
        {
            return tiposEnemigos[tipoEnemigo];
        }
        return "Desconocido";
    }
    
    /// <summary>
    /// Obtiene un array con los contadores de todos los tipos de enemigos
    /// </summary>
    public int[] ObtenerTodosLosContadores()
    {
        return contadoresPorTipo.Clone() as int[];
    }
    
    /// <summary>
    /// Reinicia todos los contadores a cero
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
        
        // Actualizar UI
        ActualizarUI();
        
        Debug.Log("SistemaCounter: Todos los contadores han sido reiniciados");
    }
    
    /// <summary>
    /// Configura el texto de la UI para mostrar el contador
    /// </summary>
    public void ConfigurarUITexto(TextMeshProUGUI textoUI)
    {
        contadorTextoUI = textoUI;
        ActualizarUI();
    }
    
    /// <summary>
    /// Método para registrar manualmente un enemigo derrotado (útil para sistemas externos)
    /// </summary>
    public void RegistrarEnemigoEliminadoPorNombre(string nombreTipoEnemigo)
    {
        // Buscar el índice correspondiente al nombre
        int tipoIndex = -1;
        for (int i = 0; i < tiposEnemigos.Length; i++)
        {
            if (tiposEnemigos[i].Equals(nombreTipoEnemigo, StringComparison.OrdinalIgnoreCase))
            {
                tipoIndex = i;
                break;
            }
        }
        
        if (tipoIndex >= 0)
        {
            RegistrarEnemigoEliminado(tipoIndex);
        }
        else
        {
            Debug.LogWarning($"SistemaCounter: Tipo de enemigo no encontrado: {nombreTipoEnemigo}");
            // Registrar como tipo genérico
            RegistrarEnemigoEliminado(0);
        }
    }
    
    #endregion
}

