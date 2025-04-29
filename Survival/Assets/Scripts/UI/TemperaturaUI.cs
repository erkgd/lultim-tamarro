using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class TemperaturaUI : MonoBehaviour
{
    private Jugador jugador;

    [Header("Configuració Temperatura")]
    [SerializeField] private float temperaturaMaxima = 200f;
    [SerializeField] private float temperaturaMinima = 0f;
    [SerializeField] private float temperaturaInicial = 200f;
    private float temperaturaActual;

    [Header("Reducció de Temperatura")]
    [SerializeField] private float quantitatReduccio = 1f;
    [SerializeField] private float tempsReduccio = 1f;

    [Header("Referències UI Barra")]
    [Tooltip("Arrossega aquí la Imatge UI BLAVA que s'omplirà a sobre (BarraFredaImatge_Emplenat).")]
    [SerializeField] private Image barraFredaImatgeEmplenament; // Canviat el nom i el propòsit

    private Coroutine reduccioCoroutine;

    private void Start()
    {
        jugador = FindObjectOfType<Jugador>();
        if (jugador == null)
            Debug.LogError($"{this.GetType().Name}: No s'ha trobat Jugador.");

        if (barraFredaImatgeEmplenament == null)
        {
            Debug.LogError($"{this.GetType().Name}: Assigna la Imatge UI per a la barra BLAVA d'emplenament a l'Inspector!");
            enabled = false;
            return;
        }
        // Assegurar-se que és de tipus Filled (millor fer-ho a l'editor, però per si de cas)
        if (barraFredaImatgeEmplenament.type != Image.Type.Filled) {
             Debug.LogWarning($"{this.GetType().Name}: La imatge d'emplenament blava no és 'Filled'. Canviant...");
             barraFredaImatgeEmplenament.type = Image.Type.Filled;
             barraFredaImatgeEmplenament.fillMethod = Image.FillMethod.Horizontal;
             barraFredaImatgeEmplenament.fillOrigin = (int)Image.OriginHorizontal.Left; // Creixent des de l'esquerra
        }


        temperaturaActual = temperaturaInicial;
        if (quantitatReduccio > 0 && tempsReduccio > 0) {
            reduccioCoroutine = StartCoroutine(ReduirTemperatura());
        }
        ActualitzarBarra(); // Actualització inicial
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
                    jugador.DecrementarVida(999, "Temperatura Baixa");
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

    private void ActualitzarBarra()
    {
        if (barraFredaImatgeEmplenament == null) return;

        // Valor normalitzat (0 = temp mínima, 1 = temp màxima)
        float valorNormalitzat = Mathf.InverseLerp(temperaturaMinima, temperaturaMaxima, temperaturaActual);

        // Volem que la barra blava estigui PLENA (fill=1) quan la temp és MÍNIMA (norm=0)
        // i BUIDA (fill=0) quan la temp és MÀXIMA (norm=1).
        // Per tant, invertim el valor normalitzat:
        float emplenamentBlau = 1f - valorNormalitzat;

        // Aplicar el fillAmount a la imatge blava
        barraFredaImatgeEmplenament.fillAmount = emplenamentBlau;

        // Debug.Log($"Temp: {temperaturaActual}, Norm: {valorNormalitzat}, EmplenamentBlau: {emplenamentBlau}");
    }
}