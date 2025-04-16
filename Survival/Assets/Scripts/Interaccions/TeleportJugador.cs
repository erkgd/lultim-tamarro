
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
    [SerializeField] private bool mostrarDebug = true;
    
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
        if (mostrarDebug) Debug.Log("TeleportJugador inicialitzat a " + destinacioSeleccionada);

        if (string.IsNullOrEmpty(nomEscenaDestí))
        {
            Debug.LogError("El nom de l'escena de destí no pot estar buit.");
        }
        
        // Verificar si las coordenadas del destino seleccionado son (0,0,0)
        if (posicioDestí == Vector3.zero && destinacioSeleccionada != TeleportDestination.Custom)
        {
            Debug.LogWarning($"La posición de destino para {destinacioSeleccionada} es (0,0,0). Verifica TPConstants.cs");
        }
    }
    
    private void OnTriggerEnter(Collider algo)
    {
        if (mostrarDebug) Debug.Log($"Colisión detectada con TeleportJugador por: {algo.name}");
        if (algo.CompareTag(etiquetaJugador))
        {
            if (algo.GetComponent<Jugador>() != null)
            {
                if (mostrarDebug) Debug.Log($"Jugador válido detectado: {algo.name}, iniciando teleporte a {nomEscenaDestí} en posición {posicioDestí}");
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
            // Verificar que la posición no sea (0,0,0) a menos que sea explícitamente esa
            if (posicioDestí == Vector3.zero && destinacioSeleccionada != TeleportDestination.Custom)
            {
                Debug.LogWarning($"¡Advertencia! Teleportando a posición (0,0,0) desde TeleportDestination.{destinacioSeleccionada}");
            }
            
            if (mostrarDebug) Debug.Log($"Teleportando jugador a: {posicioDestí} en escena: {nomEscenaDestí}");
              // Intentar usar el PosicionadorJugador si está disponible o añadirlo si no existe
            PosicionadorJugador posicionador = jugador.GetComponent<PosicionadorJugador>();
            if (posicionador == null)
            {
                if (mostrarDebug) Debug.Log($"No se encontró componente PosicionadorJugador, añadiéndolo automáticamente");
                posicionador = jugador.AddComponent<PosicionadorJugador>();
            }
            
            if (posicionador != null)
            {
                if (mostrarDebug) Debug.Log($"Usando PosicionadorJugador.IniciarTeleport");
                posicionador.IniciarTeleport(posicioDestí, nomEscenaDestí);
                return;
            }
              // Método de respaldo sin componente PosicionadorJugador (esto no debería ejecutarse nunca ahora)
            if (mostrarDebug) Debug.Log($"Error al crear/usar PosicionadorJugador, usando SistemaPerks directamente");
            
            // Guardar en SistemaPerks para que el jugador en la escena de destino lo use
            if (SistemaPerks.Instance != null)
            {
                SistemaPerks.Instance.GuardarPosicioTeleport(posicioDestí);
                if (mostrarDebug) Debug.Log($"SistemaPerks: Posición guardada: DestiX={posicioDestí.x}, DestiY={posicioDestí.y}, DestiZ={posicioDestí.z}, NecessitaTeleport=1");
            }
            else
            {
                Debug.LogWarning("SistemaPerks no está disponible, usando PlayerPrefs directamente como fallback");
                // Fallback a PlayerPrefs si SistemaPerks no está disponible
                PlayerPrefs.SetFloat("DestiX", posicioDestí.x);
                PlayerPrefs.SetFloat("DestiY", posicioDestí.y);
                PlayerPrefs.SetFloat("DestiZ", posicioDestí.z);
                PlayerPrefs.SetInt("NecessitaTeleport", 1);
                PlayerPrefs.Save();
                
                if (mostrarDebug) Debug.Log($"PlayerPrefs guardados como fallback: DestiX={posicioDestí.x}, DestiY={posicioDestí.y}, DestiZ={posicioDestí.z}, NecessitaTeleport=1");
            }

            // Cargar la nueva escena
            if (mostrarDebug) Debug.Log($"Cargando escena: {nomEscenaDestí}");
            SceneManager.LoadScene(nomEscenaDestí);
        }
        else
        {
            Debug.LogError("El objeto jugador es nulo. No se puede teletransportar.");
        }
    }
}

