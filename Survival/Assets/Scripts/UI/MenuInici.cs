using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario si cambias el icono de música
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
    [SerializeField] private Sprite musicOffSprite;    // --- CAMPOS PARA LA ANIMACIÓN DE IMAGEN ---
    [Header("Animación de Imagen")]
    [Tooltip("GameObject con la imagen que se moverá")]
    [SerializeField] private GameObject imageObject;
    [Tooltip("Velocidad de movimiento de la imagen")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("La anchura total de la imagen que se quiere mostrar")]
    [SerializeField] private float imageWidth = 1000f;
    [Tooltip("El ancho del área visible (viewport)")]
    [SerializeField] private float viewportWidth = 300f;
      private Vector2 startPosition;
    private float moveDistance;
    // --- FIN CAMPOS PARA ANIMACIÓN ---
    
    void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        UpdateMusicButtonIcon();
        Time.timeScale = 1.0f;          // Inicializar posiciones para la animación
        if (imageObject != null)
        {
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                startPosition = rectTransform.anchoredPosition;
                // Calcular la distancia total que debe desplazarse la imagen (anchura completa - anchura visible)
                moveDistance = imageWidth - viewportWidth;
                
                // Asegurar que la imagen comienza desde la posición donde se ve el inicio de la imagen
                rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y);
            }
            else
            {
                Debug.LogError("El GameObject no tiene un componente RectTransform");
            }
        }
    }    void Update()
    {
        // Animación de la imagen (desplazamiento horizontal)
        if (imageObject != null)
        {
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Calcular nueva posición x
                float newXPosition = rectTransform.anchoredPosition.x - moveSpeed * Time.deltaTime;
                
                // Si la imagen se ha movido lo suficiente para mostrar toda su longitud, reiniciar
                if (newXPosition <= -moveDistance)
                {
                    newXPosition = 0; // Volver al inicio
                }
                
                // Aplicar la nueva posición
                rectTransform.anchoredPosition = new Vector2(newXPosition, rectTransform.anchoredPosition.y);
            }
        }
    }public void PlayGame()
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