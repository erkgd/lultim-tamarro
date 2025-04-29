using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Necesario para Image

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

    // --- MODIFICADO: Referencia a la imagen AZUL de relleno ---
    [Header("Referencias UI Barra")]
    [Tooltip("Arrastra aquí la Imagen UI AZUL que se llenará encima (ColdBarImage_Relleno).")]
    [SerializeField] private Image coldBarImageRelleno; // Cambiado el nombre y el propósito
    // --- FIN MODIFICADO ---

    // --- ELIMINADO: Referencias antiguas (RectTransforms, gradient, etc.) ---
    // [SerializeField] private RectTransform coldBarRect;
    // [SerializeField] private RectTransform hotBarRect;
    // private float anchoTotalBarra;
    // [SerializeField] private bool usarGradientColor = true;
    // [SerializeField] private Gradient gradientColorBarra;
    // --- FIN ELIMINADO ---

    private Coroutine reduccioCoroutine;

    private void Start()
    {
        jugador = FindObjectOfType<Jugador>();
        if (jugador == null)
            Debug.LogError($"{this.GetType().Name}: No se encontró Jugador.");

        // --- MODIFICADO: Comprobar la nueva referencia ---
        if (coldBarImageRelleno == null)
        {
            Debug.LogError($"{this.GetType().Name}: ¡Asigna la Imagen UI para la barra AZUL de relleno en el Inspector!");
            enabled = false;
            return;
        }
        // Asegurarse que es de tipo Filled (mejor hacerlo en editor, pero por si acaso)
        if (coldBarImageRelleno.type != Image.Type.Filled) {
             Debug.LogWarning($"{this.GetType().Name}: La imagen de relleno azul no es 'Filled'. Cambiando...");
             coldBarImageRelleno.type = Image.Type.Filled;
             coldBarImageRelleno.fillMethod = Image.FillMethod.Horizontal;
             coldBarImageRelleno.fillOrigin = (int)Image.OriginHorizontal.Left; // Creciendo desde la izquierda
        }
        // --- FIN MODIFICADO ---


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

    // --- MODIFICADO: Actualiza solo el fillAmount de la barra AZUL ---
    private void ActualitzarBarra()
    {
        if (coldBarImageRelleno == null) return;

        // Valor normalizado (0 = min temp, 1 = max temp)
        float valorNormalitzat = Mathf.InverseLerp(temperaturaMinima, temperaturaMaxima, temperaturaActual);

        // Queremos que la barra azul esté LLENA (fill=1) cuando la temp es MÍNIMA (norm=0)
        // y VACÍA (fill=0) cuando la temp es MÁXIMA (norm=1).
        // Por tanto, invertimos el valor normalizado:
        float fillAmountAzul = 1f - valorNormalitzat;

        // Aplicar el fillAmount a la imagen azul
        coldBarImageRelleno.fillAmount = fillAmountAzul;

        // Debug.Log($"Temp: {temperaturaActual}, Norm: {valorNormalitzat}, FillAzul: {fillAmountAzul}");
    }
}