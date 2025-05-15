using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Sistema simplificado para contar enemigos derrotados por nivel
/// </summary>
public class SistemaCounter : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaCounter Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private TextMeshProUGUI contadorTextoUI;

    [Header("Tipo de Enemigo")]
    [SerializeField] private TipoEnemigoEnum tipoEnemigoNivel;
    
    // Enumeración para el selector en el inspector
    public enum TipoEnemigoEnum
    {
        Blobo,      // el rojo ciclope 0
        Amanita,    // seta 1
        Greko,      // volador 2
        Tatxo       // pinchitos 3
    }

    [Header("Contador")]
    [SerializeField] private int enemigosDerrotados = 0;

    // Eventos
    public event Action<int> OnEnemigoDerrotado;
      private void Awake()
    {
        // Configuración del Singleton (sin DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }private void Start()
    {
        // Actualizar UI
        ActualizarUI();
        
        // Informar del tipo de enemigo seleccionado para este nivel
        Debug.Log($"SistemaCounter: Contando enemigos del tipo {tipoEnemigoNivel} en este nivel");
    }    /// <summary>
    /// Registra un enemigo como eliminado
    /// </summary>
    public void RegistrarEnemigoEliminado()
    {
        // Incrementar contador
        enemigosDerrotados++;
        
        Debug.Log($"Enemigo eliminado - Tipo: {tipoEnemigoNivel}, " +
                 $"Total: {enemigosDerrotados}");
        
        // Notificar
        OnEnemigoDerrotado?.Invoke(enemigosDerrotados);
        
        // Actualizar UI
        ActualizarUI();
    }
    
    /// <summary>
    /// Registra un enemigo como eliminado (obsoleto, pero mantenido para compatibilidad)
    /// </summary>
    public void RegistrarEnemigoEliminado(int tipoEnemigo)
    {
        // Simplemente llama a la versión sin parámetros
        RegistrarEnemigoEliminado();
    }
      /// <summary>
    /// Actualiza la UI con los contadores actuales
    /// </summary>
    private void ActualizarUI()
    {
        if (contadorTextoUI != null)
        {
            contadorTextoUI.text = $"Enemigos: {enemigosDerrotados}";
        }
    }
      // Las funciones de guardado y carga han sido eliminadas ya que el contador no es persistente
      /// <summary>
    /// Obtiene el total de enemigos derrotados
    /// </summary>
    public int ObtenerTotalEnemigos()
    {
        return enemigosDerrotados;
    }
    
    /// <summary>
    /// Obtiene el tipo de enemigo configurado para este nivel
    /// </summary>
    public TipoEnemigoEnum ObtenerTipoEnemigo()
    {
        return tipoEnemigoNivel;
    }
      /// <summary>
    /// Obtiene el nombre del tipo de enemigo de este nivel
    /// </summary>
    public string ObtenerNombreTipoEnemigo()
    {
        return tipoEnemigoNivel.ToString();
    }
    
    /// <summary>
    /// Obtiene el nombre del tipo de enemigo por índice (obsoleto, pero mantenido para compatibilidad)
    /// </summary>
    public string ObtenerNombreTipoEnemigo(int tipoEnemigo)
    {
        // Por compatibilidad, devolvemos el nombre del tipo de enemigo de este nivel
        // independientemente del índice recibido
        return tipoEnemigoNivel.ToString();
    }
    
    /// <summary>
    /// Devuelve un array con todos los contadores por tipo (obsoleto, pero mantenido para compatibilidad)
    /// </summary>
    public int[] ObtenerTodosLosContadores()
    {
        // Por compatibilidad, devolvemos un array con un solo elemento (el contador actual)
        return new int[] { enemigosDerrotados };
    }
    
    /// <summary>
    /// Configura el texto UI para mostrar el contador
    /// </summary>
    public void ConfigurarUITexto(TextMeshProUGUI texto)
    {
        contadorTextoUI = texto;
        ActualizarUI();
    }
      /// <summary>
    /// Reinicia el contador a cero
    /// </summary>
    public void ResetearContador()
    {
        enemigosDerrotados = 0;
        ActualizarUI();
    }
    
}
