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
        
        Cortinilla cortinilla = FindObjectOfType<Cortinilla>();
        if (cortinilla != null)
        {
            
            cortinilla.ResetearCortinilla();
            // Activamos la cortinilla (cierre)
            cortinilla.MostrarCortinillaInversa();
            
        }
        Debug.Log($"PosicionadorJugador inicialitzat en {gameObject.name}");
        
        // Al iniciar, comprobamos si hay una solicitud de teleport pendiente
        StartCoroutine(ComprovarTeleport());
        
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
            Debug.Log($"POSICIONADOR JUGADOR!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1111");
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
}