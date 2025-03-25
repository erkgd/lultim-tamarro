// Implementació:
// 1. Selecciona el GameObject "Character" que representa el tamarro a l'escena.
//2. Al Inspector, fes clic a "Add Component" i busca "PosicionadorJugador".
//3. Selecciona el script per afegir-lo al jugador.
 
using UnityEngine;

public class PosicionadorJugador : MonoBehaviour
{
    // Aquest mètode s'executa quan s'inicia l'objecte a l'escena
    public void Start()
    {
        // Comprova si hi ha una petició de teleport pendent
        if (PlayerPrefs.GetInt("NecessitaTeleport", 0) == 1)
        {
            // Obté la posició guardada als PlayerPrefs
            float x = PlayerPrefs.GetFloat("DestíX", 0f);
            float y = PlayerPrefs.GetFloat("DestíY", 0f);
            float z = PlayerPrefs.GetFloat("DestíZ", 0f);
            
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                // Posiciona el jugador a les coordenades guardades
                jugador.transform.position = new Vector3(x, y, z);

                // Reinicia el flag de teleport per evitar que es torni a teleportar
                PlayerPrefs.SetInt("NecessitaTeleport", 0);
                PlayerPrefs.Save();
                Debug.Log($"Jugador teleportat a la posició: {x}, {y}, {z}");
            }
            else
            {
                Debug.LogError("No s'ha trobat cap objecte amb l'etiqueta 'Player'");
            }            
        }
    }
}