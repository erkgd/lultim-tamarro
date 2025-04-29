using System.Collections;
using UnityEngine;
using TMPro;

public class TemperaturaUI : MonoBehaviour
{
    private Jugador jugador;

    [SerializeField] private float temperaturaActual;
    [SerializeField] private float quantitatReduccio;
    [SerializeField] private float tempsReduccio;
    [SerializeField] private TextMeshProUGUI textTemperatura;

    private void Start()
    {
        jugador = FindObjectOfType<Jugador>();

        temperaturaActual = 200f;
        quantitatReduccio = 1f;
        tempsReduccio    = 2f;

        StartCoroutine(ReduirTemperatura());
        ActualitzarText();
    }

    private IEnumerator ReduirTemperatura()
    {
        while (temperaturaActual > 0)
        {
            yield return new WaitForSeconds(tempsReduccio);
            temperaturaActual -= quantitatReduccio;
            ActualitzarText();

            if (temperaturaActual <= 0)
            {
                if (jugador != null)
                    jugador.DecrementarVida(999, "Temperatura");
                else
                    Debug.LogError("No s'ha trobat Jugador a l'escena.");
            }
        }
    }

    // Aumenta la temperatura actual, mètode públic
    public void AugmentarTemperatura(float quantitat)
    {
        temperaturaActual += quantitat;
        ActualitzarText();
    }

    private void ActualitzarText()
    {
        if (textTemperatura != null)
            textTemperatura.text = $"Temperatura: {temperaturaActual:0}°";
    }

    // ← Aquí va la propiedad
    public float TemperaturaActual => temperaturaActual;
}
