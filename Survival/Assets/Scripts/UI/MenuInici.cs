using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario si cambias el icono de música

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración Escenas")]
    // IMPORTANTE: Cambia "GameLevel" por el nombre EXACTO de tu primera escena jugable
    // Viendo tu MenuPausa.cs, podría ser "Escena Principal"
    [SerializeField] private string firstLevelSceneName = "David";

    // Ya NO necesitamos referencia al panel de confirmación
    // [SerializeField] private GameObject confirmQuitPanel;
    [Header("Paneles UI")]
    [Tooltip("El panel principal del menú con Play, Salir, Música")]
    [SerializeField] private GameObject mainMenuPanel; // Aún necesitamos este

    [Header("Música")]
    [Tooltip("El AudioSource que contiene la música de fondo del menú")]
    [SerializeField] private AudioSource backgroundMusicSource;

    // --- Opcional: Para cambiar el icono del botón de música ---
    [Header("Iconos Música (Opcional)")]
    [Tooltip("El componente Image del botón para alternar la música")]
    [SerializeField] private Image musicButtonImage;
    [Tooltip("El sprite que se muestra cuando la música está activada")]
    [SerializeField] private Sprite musicOnSprite;
    [Tooltip("El sprite que se muestra cuando la música está desactivada")]
    [SerializeField] private Sprite musicOffSprite;
    // --- Fin Opcional ---

    void Start()
    {
        // Asegurarse de que el panel principal esté visible
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        // Ya no hay panel de confirmación que ocultar

        // Actualizar el icono de música si se usa esa funcionalidad
        UpdateMusicButtonIcon();

        // Asegurarse de que el tiempo fluya normalmente
        Time.timeScale = 1.0f;
    }

    // --- Funciones para Botones ---

    /// <summary>
    /// Carga la primera escena del juego.
    /// Conectar al botón "Play" / "Jugar".
    /// </summary>
    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuController: El nombre de la escena del primer nivel no está configurado!");
        }
    }

    /// <summary>
    /// Cierra la aplicación directamente.
    /// Conectar al botón "Salir" del menú principal.
    /// </summary>
    public void QuitGame() // Renombramos y simplificamos
    {
        Debug.Log("SALIENDO DEL JUEGO...");
        Application.Quit();

        // Código para detener el modo Play en el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Ya NO necesitamos RequestQuit ni CancelQuit

    /// <summary>
    /// Activa o desactiva la música de fondo.
    /// Conectar al botón de Música (el icono de altavoz).
    /// </summary>
    public void ToggleMusic()
    {
        if (backgroundMusicSource == null)
        {
            Debug.LogWarning("MainMenuController: No se ha asignado un AudioSource para la música.");
            return;
        }

        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause(); // O .Stop() si prefieres que reinicie
        }
        else
        {
            backgroundMusicSource.Play();
        }

        // Actualizar el icono si está configurado
        UpdateMusicButtonIcon();
    }

    // --- Funciones Auxiliares ---

    /// <summary>
    /// Actualiza el icono del botón de música según si está sonando o no.
    /// </summary>
    private void UpdateMusicButtonIcon()
    {
        // Solo intentar actualizar si todos los componentes necesarios están asignados
        if (musicButtonImage != null && musicOnSprite != null && musicOffSprite != null && backgroundMusicSource != null)
        {
            if (backgroundMusicSource.isPlaying)
            {
                musicButtonImage.sprite = musicOnSprite;
            }
            else
            {
                musicButtonImage.sprite = musicOffSprite;
            }
        }
    }
}