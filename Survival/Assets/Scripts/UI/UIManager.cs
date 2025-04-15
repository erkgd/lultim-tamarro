using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Singleton per a accés global
    public static UIManager Instance { get; private set; }
    
    [Header("Referencias UI")]
    [SerializeField] private Cortinilla cortinilla;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private bool iniciarSinCortinilla = true;
    
    [Header("Configuración de Cortinilla")]
    [SerializeField] private bool usarPreferenciasJugador = true;
    private const string PREF_USAR_CORTINILLA = "UsarEfectoCortinilla";
    
    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Inicializar PlayerPrefs para la cortinilla si no existe
        if (usarPreferenciasJugador && !PlayerPrefs.HasKey(PREF_USAR_CORTINILLA))
        {
            PlayerPrefs.SetInt(PREF_USAR_CORTINILLA, 1); // Por defecto activado (1 = true)
            PlayerPrefs.Save();
        }
        
        // Buscar la cortinilla si no está asignada
        if (cortinilla == null)
        {
            cortinilla = FindObjectOfType<Cortinilla>();
            
            // Si aún no se encuentra, intentar buscar en hijos
            if (cortinilla == null)
            {
                cortinilla = GetComponentInChildren<Cortinilla>(true);
            }
            
            if (cortinilla == null)
            {
                Debug.LogWarning("No se ha encontrado una Cortinilla. Asegúrate de tener una en la jerarquía.");
            }
        }
        
        if (mainCanvas == null)
        {
            mainCanvas = GetComponent<Canvas>();
            if (mainCanvas == null)
            {
                mainCanvas = GetComponentInChildren<Canvas>();
            }
        }
    }
    
    private void Start()
    {
        // Al iniciar, deshacer la cortinilla (desvanecerla) si está configurado así
        if (iniciarSinCortinilla && cortinilla != null)
        {
            StartCoroutine(DesvanecerCortinillaInicio());
        }
    }
    
    private IEnumerator DesvanecerCortinillaInicio()
    {
        // Esperar un frame para asegurar que todo está inicializado
        yield return null;
        
        cortinilla.ResetearCortinilla();
        cortinilla.DesferCortinilla();
    }
      // Método para realizar una transición completa entre escenas
    public void TransicionarAEscena(string nombreEscena, Vector3 posicionDestino = default)
    {
        // Verificar si debemos usar la cortinilla según las preferencias
        if (usarPreferenciasJugador && PlayerPrefs.GetInt(PREF_USAR_CORTINILLA, 1) == 0)
        {
            // Si el efecto está desactivado por preferencias, cargar escena directamente
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
            return;
        }
        
        if (cortinilla != null)
        {
            cortinilla.HacerTransicionCompleta(nombreEscena, posicionDestino);
        }
        else
        {
            Debug.LogError("No hay una cortinilla asignada en el UIManager. No se puede realizar la transición.");
            // Fallback: cargar la escena directamente sin transición
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
        }
    }
    
    // Método para mostrar/ocultar elementos UI
    public void MostrarElementoUI(string nombreElemento, bool mostrar)
    {
        GameObject elemento = GameObject.Find(nombreElemento);
        if (elemento != null)
        {
            elemento.SetActive(mostrar);
        }
        else
        {
            Debug.LogWarning($"No se encontró el elemento UI: {nombreElemento}");
        }
    }
    
    // Método para actualizar la jerarquía del canvas si es necesario
    public void ActualizarOrdenCanvas(Canvas canvasParaActualizar, int nuevoOrden)
    {
        if (canvasParaActualizar != null)
        {
            canvasParaActualizar.sortingOrder = nuevoOrden;
        }
    }
      // Para integrar el sistema de cortinillas con el UIManager
    public void MostrarCortinilla()
    {
        if (cortinilla != null)
        {
            cortinilla.ResetearCortinilla();
        }
        else
        {
            Debug.LogError("No hay una cortinilla asignada en el UIManager.");
        }
    }
    
    public void OcultarCortinilla()
    {
        if (cortinilla != null)
        {
            cortinilla.DesferCortinilla();
        }
        else
        {
            Debug.LogError("No hay una cortinilla asignada en el UIManager.");
        }
    }

    // Método para verificar si ya tiene una cortinilla asignada
    public bool TieneCortinilla()
    {
        return cortinilla != null;
    }

    // Método para asignar una cortinilla existente
    public void AsignarCortinilla(Cortinilla nuevaCortinilla)
    {
        if (cortinilla == null && nuevaCortinilla != null)
        {
            cortinilla = nuevaCortinilla;
            Debug.Log("UIManager: Se ha asignado una cortinilla persistente");
            
            // Si está configurado para iniciar sin cortinilla, la desvanecemos inmediatamente
            if (iniciarSinCortinilla)
            {
                StartCoroutine(DesvanecerCortinillaInicio());
            }
        }
    }
    
    // Métodos para gestionar la preferencia de cortinilla
    public bool EstaCortinillaActivada()
    {
        if (!usarPreferenciasJugador) return true;
        return PlayerPrefs.GetInt(PREF_USAR_CORTINILLA, 1) == 1;
    }
    
    public void AlternarCortinilla()
    {
        if (usarPreferenciasJugador)
        {
            bool estadoActual = PlayerPrefs.GetInt(PREF_USAR_CORTINILLA, 1) == 1;
            PlayerPrefs.SetInt(PREF_USAR_CORTINILLA, estadoActual ? 0 : 1);
            PlayerPrefs.Save();
            
            Debug.Log($"Efecto de cortinilla: {(estadoActual ? "Desactivado" : "Activado")}");
        }
    }
    
    public void ActivarCortinilla(bool activar)
    {
        if (usarPreferenciasJugador)
        {
            PlayerPrefs.SetInt(PREF_USAR_CORTINILLA, activar ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"Efecto de cortinilla: {(activar ? "Activado" : "Desactivado")}");
        }
    }
}
