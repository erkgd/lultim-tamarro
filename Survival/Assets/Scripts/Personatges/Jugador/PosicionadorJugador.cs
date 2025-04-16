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
    private const string DINAMIC_CAMERA_NAME = "Dinamic Camera";      void Start()
    {
        Debug.Log($"PosicionadorJugador inicialitzat en {gameObject.name}");
        
        // Al iniciar, comprobamos si hay una solicitud de teleport pendiente
        StartCoroutine(ComprovarTeleport());
        
        // Si hay un punto de aparición guardado, significa que venimos de otra escena
        string lastSpawnPoint = "";
        
        // Intentamos usar SistemaPerks primero y siempre
        if (SistemaPerks.Instance == null)
        {
            Debug.LogError("SistemaPerks no está disponible al iniciar PosicionadorJugador. Esto no debería ocurrir nunca. Comprueba que existe un objeto con SistemaPerks en la escena o que es cargado antes.");
            
            // Intento de recuperación - buscar SistemaPerks en la escena
            SistemaPerks[] sistemasEnEscena = FindObjectsOfType<SistemaPerks>(true);
            if (sistemasEnEscena.Length > 0)
            {
                Debug.Log("Se encontró un SistemaPerks en la escena, intentando usarlo");
                // No hacemos nada más, ya que el Awake del SistemaPerks debería configurar la instancia
            }
            else 
            {
                Debug.LogError("No se encontró ningún SistemaPerks en la escena. Se crearán problemas de persistencia de datos.");
            }
        }
        
        // Intentamos de nuevo después de la posible recuperación
        if (SistemaPerks.Instance != null)
        {
            lastSpawnPoint = SistemaPerks.Instance.ObtenirValorString("LastSpawnPoint", "");
            Debug.Log($"PosicionadorJugador: LastSpawnPoint desde SistemaPerks = '{lastSpawnPoint}'");
            
            if (!string.IsNullOrEmpty(lastSpawnPoint))
            {
                // Eliminar el valor para futuros usos
                SistemaPerks.Instance.GuardarValor("LastSpawnPoint", "");
                
                // Esperamos un momento para asegurarnos de que todo esté cargado
                StartCoroutine(BuscarYDesferCortinilla());
            }
        }
        else
        {
            // Solo como último recurso usamos PlayerPrefs directamente
            Debug.LogWarning("FALLBACK CRÍTICO - SistemaPerks sigue no disponible, usando PlayerPrefs directamente");
            lastSpawnPoint = PlayerPrefs.GetString("LastSpawnPoint", "");
            Debug.Log($"PosicionadorJugador: LastSpawnPoint desde PlayerPrefs = '{lastSpawnPoint}'");
            
            if (!string.IsNullOrEmpty(lastSpawnPoint))
            {
                PlayerPrefs.DeleteKey("LastSpawnPoint");
                PlayerPrefs.Save();
                
                // Esperamos un momento para asegurarnos de que todo esté cargado
                StartCoroutine(BuscarYDesferCortinilla());
            }
        }
    }
      private IEnumerator ComprovarTeleport()
    {
        // Esperamos un momento para que todo esté inicializado
        yield return new WaitForSeconds(0.2f);
        
        bool necessitaTeleport = false;
        Vector3 posicionFinal = Vector3.zero;
        
        // Intentamos usar SistemaPerks primero
        if (SistemaPerks.Instance != null)
        {
            necessitaTeleport = SistemaPerks.Instance.NecessitaTeleport();
            
            if (mostrarDebug) Debug.Log($"Comprovant teleport via SistemaPerks: NecessitaTeleport = {necessitaTeleport}");
            
            if (necessitaTeleport)
            {
                posicionFinal = SistemaPerks.Instance.ObtenirPosicioTeleport();
                if (mostrarDebug) Debug.Log($"Valors de teleport trobats via SistemaPerks: {posicionFinal}");
            }
        }
        else
        {
            Debug.LogWarning("SistemaPerks no está disponible - esto no debería ocurrir. Se intentará recuperar datos de PlayerPrefs como fallback");
            // Fallback a PlayerPrefs solo en caso de emergencia
            int necessitaTeleportInt = PlayerPrefs.GetInt("NecessitaTeleport", 0);
            necessitaTeleport = necessitaTeleportInt == 1;
            
            if (necessitaTeleport)
            {
                float x = PlayerPrefs.GetFloat("DestiX", 0f);
                float y = PlayerPrefs.GetFloat("DestiY", 0f);
                float z = PlayerPrefs.GetFloat("DestiZ", 0f);
                posicionFinal = new Vector3(x, y, z);
                if (mostrarDebug) Debug.Log($"FALLBACK - Valors de teleport trobats via PlayerPrefs: ({x}, {y}, {z})");
            }
        }
        
        // Si hay un teleport pendiente, posicionar al jugador
        if (necessitaTeleport)
        {
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
            
            // Limpiar el flag de teleport para evitar teleports adicionales
            if (SistemaPerks.Instance != null)
            {
                SistemaPerks.Instance.MarcarTeleportCompletat();
                if (mostrarDebug) Debug.Log("SistemaPerks: Teleport marcat com completat");
            }
            else
            {
                // Fallback a PlayerPrefs solo en caso de emergencia
                PlayerPrefs.SetInt("NecessitaTeleport", 0);
                PlayerPrefs.Save();
                Debug.LogWarning("FALLBACK - No se encontró SistemaPerks para marcar el teleport como completado. Usando PlayerPrefs directamente");
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
        
        // Guardar la posición utilizando SistemaPerks
        if (SistemaPerks.Instance != null)
        {
            SistemaPerks.Instance.GuardarPosicioTeleport(posicion);
            if (mostrarDebug) Debug.Log($"Teleport guardado en SistemaPerks: {posicion} en escena {escenaDestino}");
        }
        else
        {
            Debug.LogWarning("SistemaPerks no está disponible - esto no debería ocurrir. Usando PlayerPrefs como fallback");
            // Fallback a PlayerPrefs solo en caso de emergencia
            PlayerPrefs.SetFloat("DestiX", posicion.x);
            PlayerPrefs.SetFloat("DestiY", posicion.y);
            PlayerPrefs.SetFloat("DestiZ", posicion.z);
            PlayerPrefs.SetInt("NecessitaTeleport", 1);
            PlayerPrefs.Save();
            Debug.Log($"FALLBACK - Teleport guardado en PlayerPrefs: {posicion} en escena {escenaDestino}");
        }
        // Cargar la escena de destino
        SceneManager.LoadScene(escenaDestino);
    }
    
    

    #region Cortinilla
    
    private IEnumerator DesferCortinillaConRetraso(Cortinilla cortinilla)
    {
        // Pequeño retraso adicional para asegurar que la escena está completamente cargada
        yield return new WaitForSeconds(1.0f);
        
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
        yield return new WaitForSeconds(0.3f);
        
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