using UnityEngine;

public class RecojerPinya : MonoBehaviour
{
    // Cantidad de vida que se incrementará al recoger la pinya.
    public int incremento = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Se obtiene el componente Jugador
            Jugador jugador = other.GetComponent<Jugador>();
            if (jugador != null)
            {
                // Se llama a la función para incrementar la vida.
                jugador.IncrementarVida(incremento);
            }
            else
            {
                Debug.LogWarning("El objeto 'Player' no tiene el componente Jugador.");
            }

            // Se destruye el objeto pinya.
            Destroy(gameObject);
        }
    }
}
