using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario si cambias el icono de música
using UnityEngine.Video; // Para VideoPlayer
using System.Collections; // Para Coroutines

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración Escenas")]
    [SerializeField] private string firstLevelSceneName = "David";

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

    // --- CAMPOS PARA EL VIDEO ---
    [Header("Video Intro")]
    [Tooltip("Arrastra aquí el componente VideoPlayer que reproducirá el video.")]
    [SerializeField] private VideoPlayer introVideoPlayer;
    [Tooltip("Arrastra aquí el GameObject (Panel) que contiene el VideoPlayer y su RawImage.")]
    [SerializeField] private GameObject videoPanel;
    // --- FIN CAMPOS VIDEO ---

    void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        if (videoPanel != null)
            videoPanel.SetActive(false);

        UpdateMusicButtonIcon();
        Time.timeScale = 1.0f;
    }

    public void PlayGame()
    {
        if (introVideoPlayer != null && videoPanel != null)
        {
            StartCoroutine(PlayIntroAndLoadScene());
        }
        else
        {
            Debug.LogError("MainMenuController: VideoPlayer o VideoPanel no están asignados en el Inspector!");
            // Fallback: Cargar la escena directamente si faltan componentes
            if (!string.IsNullOrEmpty(firstLevelSceneName))
            {
                SceneManager.LoadScene(firstLevelSceneName);
            }
            else
            {
                Debug.LogError("MainMenuController: El nombre de la escena del primer nivel no está configurado!");
            }
        }
    }

    private IEnumerator PlayIntroAndLoadScene()
    {
        Debug.Log("PlayIntroAndLoadScene coroutine iniciada.");

        // 1. Ocultar el Menú Principal
        if (mainMenuPanel != null)
        {
            Debug.Log("Ocultando MainMenuPanel.");
            mainMenuPanel.SetActive(false);
        }

        // --- AÑADIR ESTA SECCIÓN PARA PARAR LA MÚSICA ---
        // 2. Parar la música del menú
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            Debug.Log("Deteniendo la música del menú.");
            backgroundMusicSource.Stop(); // O .Pause() si prefieres poder reanudarla después tal cual
        }
        // --- FIN DE LA SECCIÓN PARA PARAR LA MÚSICA ---

        // 3. Preparar y reproducir el vídeo (era el paso 2, ahora es el 3)
        Debug.Log("Configurando video...");
        videoPanel.SetActive(true);
        Debug.Log("VideoPanel activado.");

        introVideoPlayer.Prepare(); 
        Debug.Log("Llamada a introVideoPlayer.Prepare()");

        while (!introVideoPlayer.isPrepared)
        {
            Debug.Log("Esperando preparación del vídeo...");
            yield return null;
        }
        Debug.Log("Vídeo preparado.");

        introVideoPlayer.Play();
        Debug.Log("Llamada a introVideoPlayer.Play()");

        // 4. Esperar a que termine el vídeo (era el paso 3, ahora es el 4)
        while (introVideoPlayer.isPlaying)
        {
            yield return null;
        }
        Debug.Log("Video de introducción finalizado (isPlaying es false).");
        
        // 5. Cargar la siguiente escena (era el paso 5, ahora es el 5)
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