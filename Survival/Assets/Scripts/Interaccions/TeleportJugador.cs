using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportJugador : MonoBehaviour
{
    // Enum for predefined teleport destinations
    public enum TeleportDestination
    {
        Custom,
        Pedrosa,
        Hub,
        Tutorial,
        Bosc,
        Engo,
        Ramio
    }
    
    [Header("Destí")]
    [SerializeField] private TeleportDestination destinacioSeleccionada = TeleportDestination.Custom;
    [SerializeField] private string nomEscenaDestí = "Escena Principal";
    [SerializeField] private Vector3 posicioDestí;
    
    [Header("Configuració")]
    [SerializeField] private string etiquetaJugador = "Player";
    
    private void OnValidate()
    {
        // Update destination values when selection changes in inspector
        switch (destinacioSeleccionada)
        {
            case TeleportDestination.Pedrosa:
                nomEscenaDestí = TPConstants.PEDROSA_SCENE;
                posicioDestí = TPConstants.PEDROSA_SPAWN_POINT;
                break;
            
            case TeleportDestination.Hub:
                nomEscenaDestí = TPConstants.HUB_SCENE;
                posicioDestí = TPConstants.HUB_SPAWN_POINT;
                break;
                
            case TeleportDestination.Tutorial:
                nomEscenaDestí = TPConstants.TUTORIAL_SCENE;
                posicioDestí = TPConstants.TUTORIAL_SPAWN_POINT;
                break;
                
            case TeleportDestination.Bosc:
                nomEscenaDestí = TPConstants.BOSC_SCENE;
                posicioDestí = TPConstants.BOSC_SPAWN_POINT;
                break;
                
            case TeleportDestination.Engo:
                nomEscenaDestí = TPConstants.ENGO_SCENE;
                posicioDestí = TPConstants.ENGO_SPAWN_POINT;
                break;
                
            case TeleportDestination.Ramio:
                nomEscenaDestí = TPConstants.RAMIO_SCENE;
                posicioDestí = TPConstants.RAMIO_SPAWN_POINT;
                break;
                
            case TeleportDestination.Custom:
                // Do nothing, keep custom values
                break;
        }
    }
    
    void Start()
    {
        Debug.Log("TeleportJugador inicialitzat.");

        if (string.IsNullOrEmpty(nomEscenaDestí))
        {
            Debug.LogError("El nom de l'escena de destí no pot estar buit.");
        }
    }
    
    private void OnTriggerEnter(Collider algo)
    {
        Debug.Log($"Colisión detectada con TeleportJugador por: {algo.name}");
        if (algo.CompareTag(etiquetaJugador))
        {
            if (algo.GetComponent<Jugador>() != null)
            {
                Debug.Log($"Colisión detectada con TeleportJugador por: {algo.name}");
                TeletransportarJugador(algo.gameObject);
            }
            else
            {
                Debug.Log("El objeto colisionado tiene la etiqueta de jugador pero no es un jugador válido.");
            }
        }
    }

    private void TeletransportarJugador(GameObject jugador)
    {
        if (jugador != null)
        {
            PlayerPrefs.SetFloat("DestiX", posicioDestí.x);
            PlayerPrefs.SetFloat("DestiY", posicioDestí.y);
            PlayerPrefs.SetFloat("DestiZ", posicioDestí.z);
            PlayerPrefs.SetInt("NecessitaTeleport", 1);
            PlayerPrefs.Save();
            Debug.Log($"Pilladas las referencias de {posicioDestí} en la escena {nomEscenaDestí}");

            PosicionadorJugador posicionador = jugador.GetComponent<PosicionadorJugador>();
            if (posicionador != null)
            {
                posicionador.targetPosition = posicioDestí;
                posicionador.needsTeleport = true;
            }

            SceneManager.LoadScene(nomEscenaDestí);
        }
        else
        {
            Debug.LogError("El objeto jugador es nulo. No se puede teletransportar.");
        }
    }
}