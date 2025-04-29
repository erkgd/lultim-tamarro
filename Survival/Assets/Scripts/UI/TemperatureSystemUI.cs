using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TemperatureSystemUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform canvasRect;    // Asignado en tiempo de ejecución
    [SerializeField] private RectTransform uiContainer;   // El RectTransform raíz del prefab
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private TextMeshProUGUI tempText;

    [Header("Temperature Settings")]
    [SerializeField] private float minTemperature         = 0f;
    [SerializeField] private float maxTemperature         = 200f;
    [SerializeField] private float reductionAmount        = 1f;
    [SerializeField] private float reductionInterval      = 2f;
    [SerializeField] private float bonfireIncreaseAmount  = 2f;

    [Header("Player & Positioning")]
    [SerializeField] private bool followPlayer            = true;
    [SerializeField] private float verticalOffset         = 2f;

    private float    currentTemperature;
    private bool     nearBonfire = false;
    private Camera   mainCam;
    private Transform player;
    private Jugador  jugador;

    // Singleton guard
    private static bool instanceExists = false;

    // Constants
    private const string HUD_CANVAS_TAG     = "HUDCanvas";
    private const string RUNTIME_CANVAS_NAME = "HUDCanvas_Runtime";

    private void Awake()
    {
        // 1) Singleton + DontDestroyOnLoad
        if (instanceExists)
        {
            Destroy(gameObject);
            return;
        }
        instanceExists = true;
        DontDestroyOnLoad(gameObject);

        // 2) Inicializar referencias
        mainCam  = Camera.main;
        player   = GameObject.FindGameObjectWithTag("Player")?.transform;
        jugador  = player?.GetComponent<Jugador>();

        // 3) Validar campos enlazados en prefab
        if (uiContainer == null)        Debug.LogError("UI Container no asignado");
        if (temperatureSlider == null)  Debug.LogError("Temperature Slider no asignado");
        if (tempText == null)           Debug.LogError("Temp Text no asignado");

        // 4) Inicializar temperatura
        currentTemperature          = maxTemperature;
        temperatureSlider.minValue = minTemperature;
        temperatureSlider.maxValue = maxTemperature;

    }

    private void Start()
    {
        StartCoroutine(ReduceTemperatureOverTime());
        UpdateUI();
    }

    private void Update()
    {
        // Si la UI sigue al jugador, recalcular posición
        if (followPlayer && player != null)
        {
            Vector3 worldPos  = player.position + Vector3.up * verticalOffset;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0f)
            {
                uiContainer.gameObject.SetActive(false);
            }
            else
            {
                uiContainer.gameObject.SetActive(true);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                   canvasRect,
                   new Vector2(screenPos.x, screenPos.y),
                   null,
                   out Vector2 localPos
                );
                uiContainer.anchoredPosition = localPos;
            }
        }
    }

    private IEnumerator ReduceTemperatureOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(reductionInterval);

            currentTemperature += nearBonfire
                ? bonfireIncreaseAmount
                : -reductionAmount;

            // CLAMP siempre entre min y max
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);

            UpdateUI();

            if (currentTemperature <= 0f)
            {
                if (jugador != null)
                    jugador.DecrementarVida(999, "Temperatura");
                uiContainer.gameObject.SetActive(false);
                yield break;
            }
        }
    }

    private void UpdateUI()
    {
        temperatureSlider.value = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
        tempText.text = $"{currentTemperature:0}°C";
    }


    public void SetNearBonfire(bool state)
    {
        nearBonfire = state;
    }

    private void OnEnable()  => SceneManager.sceneLoaded += HandleSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReparentAfterLoad());
    }

    private IEnumerator ReparentAfterLoad()
    {
        yield return null; // esperamos un frame

        mainCam = Camera.main;
        player  = GameObject.FindGameObjectWithTag("Player")?.transform;
        jugador = player?.GetComponent<Jugador>();

        Canvas hudCanvas = FindOrCreateHUDCanvas();
        canvasRect = hudCanvas.GetComponent<RectTransform>();
        uiContainer.SetParent(canvasRect, false);
        uiContainer.localScale = Vector3.one;
        Debug.Log($"[TempUI] TemperatureContainer parented to: {hudCanvas.name}");

    }

    // AÑADE este método dentro de tu clase TemperatureSystemUI
    private Canvas FindOrCreateHUDCanvas()
    {
        // 1) busca un Canvas Overlay con CanvasScaler
        foreach (var c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay 
                && c.GetComponent<CanvasScaler>() != null)
            {
                Debug.Log($"[TempUI] Usando Canvas existente: {c.name}");
                return c;
            }
        }
        // 2) si no hay, crea uno en runtime
        Debug.Log("[TempUI] No encontré Canvas válido, creando uno runtime...");
        GameObject go = new GameObject("HUDCanvas_Runtime");
        var canvas = go.AddComponent<Canvas>();
        var scaler = go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 100;

        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        DontDestroyOnLoad(go);
        return canvas;
    }

}
