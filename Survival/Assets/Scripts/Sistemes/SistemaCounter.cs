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

    [Header("Contador")]
    [SerializeField] private int enemigosDerrotados = 0;    // Eventos
    public event Action<int> OnEnemigoDerrotado;
    
    string route="api/counterenemy";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener entre escenas
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // Informar del inicio del conteo
        Debug.Log("SistemaCounter: Iniciando conteo de enemigos en este nivel");
    }    /// <summary>
    /// Registra un enemigo como eliminado
    /// </summary>
    public void RegistrarEnemigoEliminado()
    {
        // Incrementar contador
        enemigosDerrotados++;
        
        Debug.Log($"Enemigo eliminado - Total: {enemigosDerrotados}");
        
        // Notificar
        OnEnemigoDerrotado?.Invoke(enemigosDerrotados);
    }
      /// <summary>
    /// Registra un enemigo como eliminado (versión con parámetro, mantenida para compatibilidad)
    /// </summary>
    public void RegistrarEnemigoEliminado(int tipoEnemigo)
    {
        // Simplemente llama a la versión sin parámetros
        RegistrarEnemigoEliminado();
    }

    public int ObtenerTotalEnemigos()
    {
        return enemigosDerrotados;
    }      
    
    /// <summary>
    /// Devuelve un array con el contador (mantenido para compatibilidad)
    /// </summary>
    public int[] ObtenerTodosLosContadores()
    {
        return new int[] { enemigosDerrotados };
    }
    
    /// <summary>
    /// Configura el texto UI para mostrar el contador
    /// </summary>
    
      /// <summary>
    /// Reinicia el contador a cero
    /// </summary>
    public void ResetearContador()
    {
        enemigosDerrotados = 0;
    }
    
    void sendDataToServer()
    {
        string jsonData = JsonUtility.ToJson(new { count = enemigosDerrotados });
        HttpSystem.Instance.SendRequest(route, jsonData, "POST", (respuesta) => {
            if (respuesta != null) {
                Debug.Log("Respuesta del servidor: " + respuesta);
            }
        });
    }
}
