/* 
Configuració a Unity:
1. Afegeix aquest script com a component d'un objecte 3D com ara un cub i l'adaptem al rang del tp, posteriorment li treiem el mesh render per tal que no sigui visible.
2. A la zona de Box Collider seleccionar "IsTrigger".
3. Configura les propietats:
   - Nom Escena Destí: el nom de la nova escena on es vol teletransportar el jugador.
   - Posició Destí: la posició on apareixerà el jugador a la nova escena.
   - Etiqueta Jugador: "Player".
*/
using UnityEngine;
using UnityEngine.SceneManagement;

// Aquesta classe teletransporta el jugador quan el collider trigger detecta una col·lisió amb l'objecte amb l'etiqueta definida.
public class TeleportJugador3D : MonoBehaviour
{
    [Header("Destí")]
    public string nomEscenaDestí="Escena Principal"; // Nom de l'escena on es teletransportarà el jugador
    public Vector3 posicioDestí;   // Posició dins la nova escena on apareixerà el jugador

    [Header("Configuració")]
    public string etiquetaJugador = "Player"; // Etiqueta per identificar l'objecte del jugador a l'escena

    // Aquest mètode s'executa quan algun collider entra en el trigger
    private void OnTriggerEnter(Collider algo)
    {
        if (algo.CompareTag(etiquetaJugador))
        {
            TeletransportarJugador();
        }
    }

    // Emmagatzema la posició de destí als PlayerPrefs que és una classe de Unity que permet guardar dades entre sessions de joc, en aquest cas, s'utilitza per passar la posició de destí a la nova escena.
    private void TeletransportarJugador()
    {
        PlayerPrefs.SetFloat("DestíX", posicioDestí.x);
        PlayerPrefs.SetFloat("DestíY", posicioDestí.y);
        PlayerPrefs.SetFloat("DestíZ", posicioDestí.z);
        PlayerPrefs.SetInt("NecessitaTeleport", 1);
        //Aqui cal afegir les dades que es vulguin compartir entre escenes vida, fed, habilitats actuals...
        PlayerPrefs.Save();

        SceneManager.LoadScene(nomEscenaDestí);
    }
}