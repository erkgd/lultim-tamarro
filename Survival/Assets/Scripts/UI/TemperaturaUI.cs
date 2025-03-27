using System.Collections;
using System.Collections.Generic;
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
        // Inicialitzem la referència al jugador
        jugador = FindObjectOfType<Jugador>();

        // Inicialitzem els valors de la temperatura
        temperaturaActual = 200f;
        quantitatReduccio = 1f;
        tempsReduccio = 2f;

        // Iniciem la corrutina que redueix la temperatura
        StartCoroutine(ReduirTemperatura());

        // Actualitzem el text inicial
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
                {
                    jugador.DecrementarVida(999, "Temperatura"); // Un valor alt per assegurar la mort
                    Debug.Log("El jugador ha mort de fred");
                } else {
                    Debug.LogError("No s'ha trobat Jugador a l'escena.");
                }
            }
        }
    }

    private void ActualitzarText()
    {
        if (textTemperatura != null)
        {
            textTemperatura.text = $"Temperatura: {temperaturaActual}°";
        }
    }
}
