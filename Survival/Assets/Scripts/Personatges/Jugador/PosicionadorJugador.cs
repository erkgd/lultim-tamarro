// Implementació:
// 1. Selecciona el GameObject "Character" que representa el tamarro a l'escena.
//2. Al Inspector, fes clic a "Add Component" i busca "PosicionadorJugador".
//3. Selecciona el script per afegir-lo al jugador.
 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PosicionadorJugador : MonoBehaviour
{
    // Instancia singleton del TeleportManager
    public static PosicionadorJugador Instance;

    // Datos del teleport
    public Vector3 targetPosition;
    public bool needsTeleport;

    // Inicialización del singleton
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        
        Debug.Log("PosicionadorJugador inicialitzat.");
        Posicionar();
    }

    public void Posicionar(){
        if (PlayerPrefs.GetInt("NecessitaTeleport", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("DestiX", 0f);
            float y = PlayerPrefs.GetFloat("DestiY", 0f);
            float z = PlayerPrefs.GetFloat("DestiZ", 0f);

            transform.position = new Vector3(x, y, z);

            PlayerPrefs.SetInt("NecessitaTeleport", 0);
            PlayerPrefs.Save();

            Debug.Log($"Jugador teleportat a la posició: {x}, {y}, {z}");
        }
        else
        {
            Debug.Log("No hi ha cap petició de teleport pendent.");
        }
    }

    // Se llama cuando se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Esperamos un frame para asegurarnos de que todos los objetos estén inicializados
        StartCoroutine(PosicionarJugadorDespuesDeCargar());
    }

    private IEnumerator PosicionarJugadorDespuesDeCargar()
    {
        // Esperamos un frame para que todos los objetos estén completamente inicializados
        yield return (1f);
        
        // Verificamos si hay una solicitud de teleport pendiente
        if (needsTeleport || PlayerPrefs.GetInt("NecessitaTeleport", 0) == 1)
        {
    
            // Obtenemos la posición desde PlayerPrefs si está disponible
            Vector3 posicionFinal = needsTeleport ? targetPosition : new Vector3(
                PlayerPrefs.GetFloat("DestiX", 0f),
                PlayerPrefs.GetFloat("DestiY", 0f),
                PlayerPrefs.GetFloat("DestiZ", 0f)
            );
            
            // Buscamos el jugador
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
            {
                // Desactivamos temporalmente el CharacterController si existe
                CharacterController controller = jugadorObj.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }
                // Posicionamos al jugador
                jugadorObj.transform.position = posicionFinal;
                
                // Reactivamos el CharacterController
                if (controller != null)
                {
                    controller.enabled = true;
                }
                
                Debug.Log($"Jugador teleportado correctamente a la posición: {posicionFinal.x}, {posicionFinal.y}, {posicionFinal.z}");
                
                // Limpiamos las variables de teleport
                needsTeleport = false;
                PlayerPrefs.SetInt("NecessitaTeleport", 0);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("No se encontró el jugador después de cargar la escena");
            }
        }
    }

    // Método para solicitar un teleport
    public void RequestTeleport(Vector3 position, string sceneName)
    {
        Debug.Log($"Solicitud de teletransporte recibida. Posición: {position}, Escena: {sceneName}");
        targetPosition = position;
        needsTeleport = true;
        SceneManager.LoadScene(sceneName);
    }
}