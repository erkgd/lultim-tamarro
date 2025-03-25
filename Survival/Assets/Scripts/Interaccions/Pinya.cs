using UnityEngine;

public class RecojerPinya : MonoBehaviour
{
    // Cantidad de vida que se incrementar� al recoger la pinya.
    public int incremento = 4;
    // Fuente que se mostrar� en el log del SistemaVida.
    private string fuente = "Pinya";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Se obtiene el componente SistemaVida del jugador.
            Jugador jugador = other.GetComponent<Jugador>();
            if (jugador != null)
            {
                // Se llama a la funci�n para incrementar la vida.
                jugador.IncrementarVida(incremento, fuente);
            }
            else
            {
                Debug.LogWarning("El objeto 'Player' no tiene el componente SistemaVida.");
            }

            // Se destruye el objeto pinya.
            Destroy(gameObject);
        }
    }
}
