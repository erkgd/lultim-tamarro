using UnityEngine;

/// <summary>
/// Extensiones para el GameObject del jugador que garantizan
/// los componentes necesarios para el correcto funcionamiento del Input System.
/// </summary>
public static class JugadorExtensions
{
    /// <summary>
    /// Asegura que el GameObject del jugador tenga todos los componentes
    /// necesarios para el correcto funcionamiento del Input System.
    /// </summary>
    /// <param name="jugador">El GameObject del jugador</param>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void AsegurarComponentesInput()
    {
        // Este método se ejecuta automáticamente antes de que se cargue cualquier escena
        Debug.Log("Configurando Input System para el juego");
        
        // Nos suscribimos al evento de carga de escena para añadir los componentes necesarios
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            
            if (jugador != null)
            {
                // Añadir el InputSystemManager si no existe
                if (jugador.GetComponent<InputSystemManager>() == null)
                {
                    jugador.AddComponent<InputSystemManager>();
                    Debug.Log("InputSystemManager añadido al jugador");
                }
            }
            else
            {
                // Si no encontramos al jugador, creamos un objeto de gestión independiente
                GameObject gestionInput = new GameObject("Input System Manager");
                gestionInput.AddComponent<InputSystemManager>();
                Debug.Log("Se ha creado un GameObject separado para InputSystemManager porque no se encontró un jugador");
            }
        };
    }
}