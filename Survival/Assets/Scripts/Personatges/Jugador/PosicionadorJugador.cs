// Implementació:
// 1. Añadir este script al jugador en CADA escena
// 2. No uses DontDestroyOnLoad - cada escena debe tener su propio jugador
 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PosicionadorJugador : MonoBehaviour
{
    // Configuración
    [SerializeField] private bool mostrarDebug = true;
    
    // Nombres de los objetos de cámara que debemos buscar
    private const string DINAMIC_CAMERA_NAME = "Dinamic Camera";      
    
    void Start()
    {
        Debug.Log($"PosicionadorJugador inicialitzat en {gameObject.name}");
        
        // Al iniciar, comprobamos si hay una solicitud de teleport pendiente
        StartCoroutine(ComprovarTeleport());
        
        // Iniciamos la transición de cortinilla
        StartCoroutine(BuscarYDesferCortinilla());
    
        // Verificamos si hay un punto de aparición guardado
        string lastSpawnPoint = PlayerPrefs.GetString("LastSpawnPoint", "");
        if (mostrarDebug) Debug.Log($"PosicionadorJugador: LastSpawnPoint = '{lastSpawnPoint}'");
        
        if (!string.IsNullOrEmpty(lastSpawnPoint))
        {
            PlayerPrefs.DeleteKey("LastSpawnPoint");
            PlayerPrefs.Save();
        }
    }
    
    private IEnumerator ComprovarTeleport()
    {
        // Esperamos un momento para que todo esté inicializado
        yield return new WaitForSeconds(0.2f);
        
        int necessitaTeleport = PlayerPrefs.GetInt("NecessitaTeleport", 0);
        
        if (mostrarDebug) Debug.Log($"Comprovant teleport: NecessitaTeleport = {necessitaTeleport}");
        
        // Si hay un teleport pendiente, posicionar al jugador
        if (necessitaTeleport == 1)
        {
            // Obtener las coordenadas guardadas
            float x = PlayerPrefs.GetFloat("DestiX", 0f);
            float y = PlayerPrefs.GetFloat("DestiY", 0f);
            float z = PlayerPrefs.GetFloat("DestiZ", 0f);
            Vector3 posicionFinal = new Vector3(x, y, z);
            
            if (mostrarDebug) Debug.Log($"Valors de teleport trobats: ({x}, {y}, {z})");
            
            // Desactivar el CharacterController temporalmente para evitar conflictos
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Posicionar el jugador
            transform.position = posicionFinal;
            
            // Reactivar el CharacterController
            if (controller != null)
            {
                controller.enabled = true;
            }
            
            // Asegurarse de que la cámara sigue al jugador
            AssignarCamera();
            
            // Limpiar los PlayerPrefs para evitar teleports adicionales
            PlayerPrefs.SetInt("NecessitaTeleport", 0);
            PlayerPrefs.Save();
            
            if (mostrarDebug) Debug.Log($"Jugador teleportat a la posició: {posicionFinal}");
        }
        else
        {
            if (mostrarDebug) Debug.Log("No hi ha cap petició de teleport pendent.");
            
            // Incluso si no hay teleport, nos aseguramos de que la cámara sigue al jugador
            AssignarCamera();
        }
    }
    
    private void AssignarCamera()
    {
        GameObject camara = GameObject.Find(DINAMIC_CAMERA_NAME);
        if (camara != null)
        {
            // Intentar obtener componente Cinemachine
            var virtualCamera = camara.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                virtualCamera.Follow = transform;
                if (mostrarDebug) Debug.Log($"Cámara virtual asignada para seguir a {gameObject.name}");
            }
            else
            {
                if (mostrarDebug) Debug.LogWarning("No se encontró el componente CinemachineVirtualCamera");
            }
        }
        else
        {
            if (mostrarDebug) Debug.LogWarning($"No se encontró la cámara: {DINAMIC_CAMERA_NAME}");
        }
    }

    public void IniciarTeleport(Vector3 posicion, string escenaDestino)
    {
        if (mostrarDebug) Debug.Log($"Iniciando teleport a {posicion} en escena {escenaDestino}");
        
        // Guardar la posición en PlayerPrefs
        PlayerPrefs.SetFloat("DestiX", posicion.x);
        PlayerPrefs.SetFloat("DestiY", posicion.y);
        PlayerPrefs.SetFloat("DestiZ", posicion.z);
        PlayerPrefs.SetInt("NecessitaTeleport", 1);
        PlayerPrefs.Save();
        
        // Cargar la escena de destino
        SceneManager.LoadScene(escenaDestino);
    }
    
    

    #region Cortinilla
    
    private IEnumerator DesferCortinillaConRetraso(Cortinilla cortinilla)
    {
        
        Debug.Log("DesferCortinillaConRetraso: Iniciando apertura de cortinilla");
        
        // Verificar que la cortinilla aún existe
        if (cortinilla == null)
        {
            Debug.LogError("DesferCortinillaConRetraso: La referencia a la cortinilla es nula, intentando encontrarla de nuevo");
            
            // Intento de recuperación - buscar la cortinilla de nuevo
            if (Cortinilla.Instance != null) 
            {
                cortinilla = Cortinilla.Instance;
            }
            else 
            {
                Cortinilla[] cortinillasEnEscena = FindObjectsOfType<Cortinilla>(true);
                if (cortinillasEnEscena.Length > 0)
                {
                    cortinilla = cortinillasEnEscena[0];
                    Debug.Log("DesferCortinillaConRetraso: Se encontró una cortinilla alternativa");
                }
                else
                {
                    Debug.LogError("DesferCortinillaConRetraso: No se pudo encontrar ninguna cortinilla");
                    yield break;
                }
            }
        }
        
        // Forzamos a usar la cortinilla en este caso para asegurar la transición correcta
        bool usarCortinilla = true;
        
        // Solo para debug, comprobamos la preferencia guardada
        if (SistemaPerks.Instance != null)
        {
            string usarCortinillaStr = SistemaPerks.Instance.ObtenirValorString("UsarCortinilla", "1");
            Debug.Log($"Preferencia de cortinilla desde SistemaPerks: {(usarCortinillaStr == "1" ? "Activada" : "Desactivada")}");
        }
        
        // Comprobar si el gameObject de la cortinilla existe
        if (cortinilla.gameObject != null)
        {
            Debug.Log($"Estado de la cortinilla antes de usarla: Activa={cortinilla.gameObject.activeInHierarchy}");
            
            try 
            {
                // Activamos la cortinilla siempre para asegurar que podemos interactuar con ella
                cortinilla.gameObject.SetActive(true);
                
                // Asegurarnos de que la cortinilla puede mostrarse de nuevo
                cortinilla.ResetearCortinilla();
                
                // Deshacer el efecto de la cortinilla (abrir)
                cortinilla.DesferCortinilla();
                
                Debug.Log("DesferCortinillaConRetraso: Cortinilla abierta correctamente");
            }
            catch (System.Exception ex) 
            {
                Debug.LogError($"Error al intentar abrir la cortinilla: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("DesferCortinillaConRetraso: El GameObject de la cortinilla es nulo");
        }
    }
      // Corrutina para buscar la cortinilla y deshacer su efecto
    private IEnumerator BuscarYDesferCortinilla()
    {
        // Esperamos para asegurarnos que toda la escena esté cargada
        yield return new WaitForSeconds(3.5f);
        
        Debug.Log("PosicionadorJugador: Buscando cortinilla para deshacer efecto...");
        
        // Primero intentamos usar la instancia singleton si existe
        if (Cortinilla.Instance != null)
        {
            Debug.Log("PosicionadorJugador: Usando instancia singleton de Cortinilla");
            StartCoroutine(DesferCortinillaConRetraso(Cortinilla.Instance));
            yield break;
        }
        
        // Buscar en la jerarquía UI/ImageCortinilla
        GameObject uiObject = GameObject.Find("UI");
        if (uiObject != null)
        {
            Transform imageCortinillaTransform = uiObject.transform.Find("ImageCortinilla");
            
            if (imageCortinillaTransform != null)
            {
                Cortinilla cortinilla = imageCortinillaTransform.GetComponent<Cortinilla>();
                if (cortinilla != null)
                {
                    Debug.Log("PosicionadorJugador: Cortinilla encontrada en UI/ImageCortinilla");
                    StartCoroutine(DesferCortinillaConRetraso(cortinilla));
                    yield break;
                }
            }
        }
        
        // Si no lo encontramos en la ruta específica, buscamos en toda la escena
        Cortinilla[] cortinillas = FindObjectsOfType<Cortinilla>(true); // incluye objetos inactivos
        if (cortinillas.Length > 0)
        {
            Debug.Log($"PosicionadorJugador: Encontradas {cortinillas.Length} cortinillas en la escena");
            StartCoroutine(DesferCortinillaConRetraso(cortinillas[0]));
        }
        else
        {
            Debug.LogError("PosicionadorJugador: No se encontró ninguna cortinilla en la escena. ¡IMPORTANTE! Asegúrate de que existe un GameObject llamado 'ImageCortinilla' con el componente Cortinilla.cs en la jerarquía UI");
        }
    }
    #endregion
    
}