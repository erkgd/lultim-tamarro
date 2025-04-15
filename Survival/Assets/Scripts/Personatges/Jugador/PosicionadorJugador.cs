// Implementació:
// 1. Añadir este script al jugador en CADA escena
// 2. No uses DontDestroyOnLoad - cada escena debe tener su propio jugador
 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PosicionadorJugador : MonoBehaviour
{
    // Configuración
    [SerializeField] private bool mostrarDebug = false;
    
    // Nombres de los objetos de cámara que debemos buscar
    private const string DINAMIC_CAMERA_NAME = "Dinamic Camera";    
    
    void Start()
    {
        if (mostrarDebug) Debug.Log($"PosicionadorJugador inicialitzat en {gameObject.name}");
        
        // Al iniciar, comprobamos si hay una solicitud de teleport pendiente
        StartCoroutine(ComprovarTeleport());
        
        // Si hay un punto de aparición guardado, significa que venimos de otra escena
        string lastSpawnPoint = "";
        
        if (SistemaPerks.Instance != null)
        {
            lastSpawnPoint = SistemaPerks.Instance.ObtenirValorString("LastSpawnPoint", "");
            if (!string.IsNullOrEmpty(lastSpawnPoint))
            {
                // Eliminar el valor para futuros usos
                SistemaPerks.Instance.GuardarValor("LastSpawnPoint", "");
                
                // Buscar una cortinilla en la escena actual y deshacer el efecto
                Cortinilla cortinilla = FindObjectOfType<Cortinilla>();
                if (cortinilla != null)
                {
                    StartCoroutine(DesferCortinillaConRetraso(cortinilla));
                    if (mostrarDebug) Debug.Log("Detectada transición entre escenas, deshaciendo cortinilla");
                }
                else
                {
                    if (mostrarDebug) Debug.LogWarning("No se encontró cortinilla en la escena actual tras teleporte");
                }
            }
        }
        else
        {
            // Fallback si SistemaPerks no está disponible
            lastSpawnPoint = PlayerPrefs.GetString("LastSpawnPoint", "");
            if (!string.IsNullOrEmpty(lastSpawnPoint))
            {
                PlayerPrefs.DeleteKey("LastSpawnPoint");
                PlayerPrefs.Save();
                
                // Buscar una cortinilla en la escena actual y deshacer el efecto
                Cortinilla cortinilla = FindObjectOfType<Cortinilla>();
                if (cortinilla != null)
                {
                    StartCoroutine(DesferCortinillaConRetraso(cortinilla));
                    if (mostrarDebug) Debug.Log("Detectada transición entre escenas, deshaciendo cortinilla (fallback)");
                }
            }
        }
    }    
    private IEnumerator ComprovarTeleport()
    {
        // Esperamos un momento para que todo esté inicializado
        yield return new WaitForSeconds(0.2f);
        
        bool necessitaTeleport = false;
        
        // Usar SistemaPerks si está disponible
        if (SistemaPerks.Instance != null)
        {
            necessitaTeleport = SistemaPerks.Instance.NecessitaTeleport();
        }
        else
        {
            // Fallback a PlayerPrefs si SistemaPerks no está disponible
            necessitaTeleport = PlayerPrefs.GetInt("NecessitaTeleport", 0) == 1;
        }
        
        if (mostrarDebug) Debug.Log($"Comprovant teleport: NecessitaTeleport = {necessitaTeleport}");
          // Si hay un teleport pendiente, posicionar al jugador
        if (necessitaTeleport)
        {
            Vector3 posicionFinal;
            string spawnPointTag = "";
            
            // Usar SistemaPerks si está disponible
            if (SistemaPerks.Instance != null)
            {
                spawnPointTag = SistemaPerks.Instance.ObtenirValorString("SpawnPointTag", "");
            }
            else
            {
                // Fallback si SistemaPerks no está disponible
                spawnPointTag = PlayerPrefs.GetString("SpawnPointTag", "");
            }
            
            if (!string.IsNullOrEmpty(spawnPointTag))
            {
                // Buscar el punto de spawn por tag
                GameObject spawnPoint = GameObject.FindWithTag(spawnPointTag);
                
                if (spawnPoint != null)
                {
                    posicionFinal = spawnPoint.transform.position;
                    
                    if (mostrarDebug) Debug.Log($"Punto de spawn encontrado por tag: {spawnPointTag} en posición {posicionFinal}");
                }
                else
                {
                    if (mostrarDebug) Debug.LogWarning($"No se encontró un punto de spawn con tag: {spawnPointTag}");
                    
                    // Usar coordenadas específicas como respaldo
                    if (SistemaPerks.Instance != null)
                    {
                        // Obtener posición usando SistemaPerks
                        posicionFinal = SistemaPerks.Instance.ObtenirPosicioTeleport();
                    }
                    else
                    {
                        // Fallback a PlayerPrefs
                        float x = PlayerPrefs.GetFloat("DestiX", 0f);
                        float y = PlayerPrefs.GetFloat("DestiY", 0f);
                        float z = PlayerPrefs.GetFloat("DestiZ", 0f);
                        posicionFinal = new Vector3(x, y, z);
                    }
                    
                    if (mostrarDebug) Debug.Log($"Usando coordenadas específicas como respaldo: {posicionFinal}");
                }
            }
            else
            {
                // Obtener las coordenadas guardadas directamente
                if (SistemaPerks.Instance != null)
                {
                    // Obtener posición usando SistemaPerks
                    posicionFinal = SistemaPerks.Instance.ObtenirPosicioTeleport();
                }
                else
                {
                    // Fallback a PlayerPrefs
                    float x = PlayerPrefs.GetFloat("DestiX", 0f);
                    float y = PlayerPrefs.GetFloat("DestiY", 0f);
                    float z = PlayerPrefs.GetFloat("DestiZ", 0f);
                    posicionFinal = new Vector3(x, y, z);
                }
                
                if (mostrarDebug) Debug.Log($"Valors de teleport trobats: {posicionFinal}");
            }
            
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
              // Marcar el teleport como completado
            if (SistemaPerks.Instance != null)
            {
                // Usar SistemaPerks para limpiar la información de teleport
                SistemaPerks.Instance.MarcarTeleportCompletat();
                SistemaPerks.Instance.GuardarValor("SpawnPointTag", "");
            }
            else
            {
                // Fallback a PlayerPrefs
                PlayerPrefs.SetInt("NecessitaTeleport", 0);
                PlayerPrefs.DeleteKey("SpawnPointTag");
                PlayerPrefs.Save();
            }
            
            if (mostrarDebug) Debug.Log($"Jugador teleportat a la posició: {posicionFinal}");
        }
        else
        {
            if (mostrarDebug) Debug.Log("No hi ha cap petició de teleport pendent.");
            
            // Incluso si no hay teleport, nos aseguramos de que la cámara sigue al jugador
            AssignarCamera();
        }
    }
    
    // Método para asignar la cámara al jugador
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
    
    // Método para iniciar un teleport desde TeleportJugador
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
    
    // Sobrecarga para iniciar teleport usando tags de spawn points
    public void IniciarTeleport(string spawnPointTag, string escenaDestino)
    {
        if (mostrarDebug) Debug.Log($"Iniciando teleport al punto de spawn '{spawnPointTag}' en escena {escenaDestino}");
        
        // Guardar el tag del punto de spawn para buscarlo en la escena de destino
        PlayerPrefs.SetString("SpawnPointTag", spawnPointTag);
        PlayerPrefs.SetInt("NecessitaTeleport", 1);
        PlayerPrefs.Save();
        
        // Cargar la escena de destino
        SceneManager.LoadScene(escenaDestino);
    }
    
    // Método para deshacer la cortinilla con un pequeño retraso como el mio :)
    private IEnumerator DesferCortinillaConRetraso(Cortinilla cortinilla)
    {
        // Pequeño retraso para asegurar que la escena está completamente cargada
        yield return new WaitForSeconds(0.2f);
        
        // Asegurarnos de que la cortinilla puede mostrarse de nuevo (por si acaso)
        cortinilla.ResetearCortinilla();
        
        // Deshacer el efecto de la cortinilla
        cortinilla.DesferCortinilla();
        
        if (mostrarDebug) Debug.Log("Efecto de cortinilla deshecho después de la transición entre escenas");
    }
}