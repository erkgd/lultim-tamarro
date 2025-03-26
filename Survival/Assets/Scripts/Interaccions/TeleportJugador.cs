using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportJugador : MonoBehaviour
{
    [Header("Destí")]
    public string nomEscenaDestí = "Escena Principal";
    public Vector3 posicioDestí;

    [Header("Configuració")]
    public string etiquetaJugador = "Player";

    
    void Start ()
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