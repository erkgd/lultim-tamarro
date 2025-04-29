using System.Collections;
using UnityEngine;
// using TMPro; // No necesario
using UnityEngine.UI; // No necesario si solo usamos RectTransform, pero puede ser útil

public class TemperaturaUI : MonoBehaviour
{
    private Jugador jugador;

    [Header("Configuración Temperatura")]
    [SerializeField] private float temperaturaMaxima = 200f;
    [SerializeField] private float temperaturaMinima = 0f;
    [SerializeField] private float temperaturaInicial = 200f;
    private float temperaturaActual;

    [Header("Reducción de Temperatura")]
    [SerializeField] private float quantitatReduccio = 1f;
    [SerializeField] private float tempsReduccio = 1f;

    // --- NUEVO: Referencias a los RectTransforms de las barras ---
    [Header("Referencias UI Barra")]
    [Tooltip("Arrastra aquí el RectTransform de la imagen de la barra FRÍA (azul).")]
    [SerializeField] private RectTransform coldBarRect;
    [Tooltip("Arrastra aquí el RectTransform de la imagen de la barra CALIENTE (rojo).")]
    [SerializeField] private RectTransform hotBarRect;
    // --- FIN NUEVO ---

    // --- NUEVO: Variable para el ancho total ---
    private float anchoTotalBarra;
    // --- FIN NUEVO ---

    // --- ELIMINADO: Referencias antiguas (fillAmount, gradient) ---
    // [SerializeField] private Image imagenMascaraRelleno;
    // [SerializeField] private bool usarGradientColor = true;
    // [SerializeField] private Gradient gradientColorBarra;
    // --- FIN ELIMINADO ---

    private Coroutine reduccioCoroutine;

    private void Start()
    {
        jugador = FindObjectOfType<Jugador>();
        if (jugador == null)
            Debug.LogError($"{this.GetType().Name}: No se encontró Jugador.");

        // --- NUEVO: Comprobar referencias y obtener ancho ---
        if (coldBarRect == null || hotBarRect == null)
        {
            Debug.LogError($"{this.GetType().Name}: ¡Asigna los RectTransforms de ColdBar y HotBar en el Inspector!");
            enabled = false;
            return;
        }
        // Asumimos que el RectTransform del padre (contenedor) o una de las barras inicialmente tiene el ancho correcto
        // O podemos obtenerlo del RectTransform del propio UIManager si las barras están ancladas a él.
        // Calculamos el ancho basado en el contenedor padre de una de las barras (más robusto)
        if (hotBarRect.parent is RectTransform parentRect) {
             anchoTotalBarra = parentRect.rect.width;
        } else {
             // Fallback: usar el ancho inicial de la hot bar (menos ideal si no está bien configurada al inicio)
             anchoTotalBarra = hotBarRect.sizeDelta.x;
             Debug.LogWarning($"{this.GetType().Name}: No se pudo obtener ancho del padre, usando ancho inicial de HotBar. Asegúrate que es correcto.");
        }

        if (anchoTotalBarra <= 0) {
            Debug.LogError($"{this.GetType().Name}: El ancho total de la barra es 0 o negativo. Verifica la configuración del contenedor.");
            enabled = false;
            return;
        }
        // --- FIN NUEVO ---

        temperaturaActual = temperaturaInicial;
        if (quantitatReduccio > 0 && tempsReduccio > 0) {
            reduccioCoroutine = StartCoroutine(ReduirTemperatura());
        }
        ActualitzarBarra(); // Actualización inicial
    }

    private IEnumerator ReduirTemperatura()
    {
        while (temperaturaActual > temperaturaMinima)
        {
            yield return new WaitForSeconds(tempsReduccio);

            if (jugador != null && jugador.VidaActual > 0)
            {
                 temperaturaActual -= quantitatReduccio;
                 temperaturaActual = Mathf.Max(temperaturaActual, temperaturaMinima);
                 ActualitzarBarra();

                 if (temperaturaActual <= temperaturaMinima)
                 {
                    jugador.DecrementarVida(999, "Temperatura Baja");
                 }
            }
        }
        reduccioCoroutine = null;
    }

    public void AugmentarTemperatura(float quantitat)
    {
        if (quantitat <= 0 || temperaturaActual >= temperaturaMaxima || (jugador != null && jugador.VidaActual <= 0))
            return;

        temperaturaActual += quantitat;
        temperaturaActual = Mathf.Min(temperaturaActual, temperaturaMaxima);
        ActualitzarBarra();

        if (reduccioCoroutine == null && temperaturaActual > temperaturaMinima && quantitatReduccio > 0 && tempsReduccio > 0)
        {
            reduccioCoroutine = StartCoroutine(ReduirTemperatura());
        }
    }

    // --- MODIFICADO: Actualiza los anchos de las dos barras ---
    private void ActualitzarBarra()
    {
        if (coldBarRect == null || hotBarRect == null) return;

        float valorNormalitzat = Mathf.InverseLerp(temperaturaMinima, temperaturaMaxima, temperaturaActual);

        // Calcular anchos
        float anchoHot = anchoTotalBarra * valorNormalitzat;
        float anchoCold = anchoTotalBarra * (1f - valorNormalitzat);

        // Aplicar anchos a los RectTransforms usando sizeDelta
        // sizeDelta establece el tamaño relativo a las anclas. Como anclamos a un borde y estiramos verticalmente,
        // cambiar solo sizeDelta.x ajustará el ancho desde el punto de anclaje.
        hotBarRect.sizeDelta = new Vector2(anchoHot, hotBarRect.sizeDelta.y); // Mantenemos la altura original o la calculada por anclas
        coldBarRect.sizeDelta = new Vector2(anchoCold, coldBarRect.sizeDelta.y);

        // Debug.Log($"Temp: {temperaturaActual}, Norm: {valorNormalitzat}, AnchoHot: {anchoHot}, AnchoCold: {anchoCold}");
    }
}