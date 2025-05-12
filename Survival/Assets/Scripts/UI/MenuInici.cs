using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario si cambias el icono de música
using System.Collections; // Para Coroutines

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración Escenas")]
    [SerializeField] private string firstLevelSceneName = "Intro";

    [Header("Paneles UI")]
    [Tooltip("El panel principal del menú con Play, Salir, Música")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Música")]
    [Tooltip("El AudioSource que contiene la música de fondo del menú")]
    [SerializeField] private AudioSource backgroundMusicSource;

    [Header("Iconos Música (Opcional)")]
    [Tooltip("El componente Image del botón para alternar la música")]
    [SerializeField] private Image musicButtonImage;
    [Tooltip("El sprite que se muestra cuando la música está activada")]
    [SerializeField] private Sprite musicOnSprite;
    [Tooltip("El sprite que se muestra cuando la música está desactivada")]
    [SerializeField] private Sprite musicOffSprite;
      void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        UpdateMusicButtonIcon();
        Time.timeScale = 1.0f;
    }
      void Update()
    {
        // Este método está vacío, pero se mantiene por si se necesita agregar funcionalidad en el futuro
    }
    public void PlayGame()
    {
        // Detener la música antes de cambiar de escena
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }

        // Cargar directamente la escena del juego
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            Debug.Log($"Cargando escena: {firstLevelSceneName}");
            SceneManager.LoadScene(firstLevelSceneName);
        }
        else
        {
            Debug.LogError("MainMenuController: El nombre de la escena del primer nivel no está configurado!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("SALIENDO DEL JUEGO...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ToggleMusic()
    {
        if (backgroundMusicSource == null) return;
        if (backgroundMusicSource.isPlaying) backgroundMusicSource.Pause();
        else backgroundMusicSource.Play();
        UpdateMusicButtonIcon();
    }

    private void UpdateMusicButtonIcon()
    {
        if (musicButtonImage != null && musicOnSprite != null && musicOffSprite != null && backgroundMusicSource != null)
        {
            musicButtonImage.sprite = backgroundMusicSource.isPlaying ? musicOnSprite : musicOffSprite;
        }
    }
}