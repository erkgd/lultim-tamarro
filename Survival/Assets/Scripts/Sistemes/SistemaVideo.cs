using UnityEngine;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Sistema simple para reproducir videos en Unity.
/// </summary>
public class SistemaVideo : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Reproductor de video de Unity")]
    [SerializeField] private VideoPlayer videoPlayer;
    [Tooltip("Objeto que contiene el renderer donde se mostrará el video (opcional)")]
    [SerializeField] private GameObject pantallaVideo;

    [Header("Configuración")]
    [Tooltip("Si es verdadero, el video comenzará automáticamente al iniciar")]
    [SerializeField] private bool reproducirAutomaticamente = false;
    [Tooltip("Si es verdadero, el video se repetirá en bucle")]
    [SerializeField] private bool repetirVideo = false;
    [Tooltip("Ruta del video (puede ser un URL o una ruta local)")]
    [SerializeField] private string rutaVideo = "";

    private void Awake()
    {
        // Inicializar el VideoPlayer si no está asignado
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        // Configuraciones básicas
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = repetirVideo;
        
        // Si tenemos una pantalla asignada, mostrar el video en ella
        if (pantallaVideo != null)
        {
            Renderer renderer = pantallaVideo.GetComponent<Renderer>();
            if (renderer != null)
            {
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
                videoPlayer.targetMaterialRenderer = renderer;
                videoPlayer.targetMaterialProperty = "_MainTex";
            }
        }
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(rutaVideo))
        {
            CargarVideo(rutaVideo);
            
            if (reproducirAutomaticamente)
            {º
                videoPlayer.Play();
            }
        }
    }

    /// <summary>
    /// Carga un video desde la ruta especificada
    /// </summary>
    /// <param name="ruta">Ruta del archivo o URL del video</param>
    public void CargarVideo(string ruta)
    {
        if (string.IsNullOrEmpty(ruta))
            return;

        // Detener cualquier reproducción actual
        videoPlayer.Stop();

        // Configurar la fuente del video
        if (ruta.StartsWith("http"))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = ruta;
        }
        else
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, ruta);
        }

        rutaVideo = ruta;
        videoPlayer.Prepare();
    }

    /// <summary>
    /// Reproduce el video
    /// </summary>
    public void ReproducirVideo()
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
        else
        {
            StartCoroutine(ReproducirCuandoEstePreparado());
        }
    }

    /// <summary>
    /// Espera hasta que el video esté preparado y luego lo reproduce
    /// </summary>
    private IEnumerator ReproducirCuandoEstePreparado()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        videoPlayer.Play();
    }

    /// <summary>
    /// Detiene el video
    /// </summary>
    public void DetenerVideo()
    {
        videoPlayer.Stop();
    }
}