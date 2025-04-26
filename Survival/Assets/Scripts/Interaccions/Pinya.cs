using UnityEngine;

public class RecollirPinya : MonoBehaviour
{
    // quantitat de vida que incrementa el jugador al recollir la pinya
    public int increment = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // s'obte el component Jugador de l'objecte amb el tag "Player"
            Jugador jugador = other.GetComponent<Jugador>();
            if (jugador != null)
            {
                // Truquem a la funcio de incrementar vida
                jugador.IncrementarVida(increment);
            }
            else
            {
                Debug.LogWarning("El objeto 'Player' no tiene el componente Jugador.");
            }

            // Destruiim l'objecte de la pinya
            Destroy(gameObject);
        }
    }
}
