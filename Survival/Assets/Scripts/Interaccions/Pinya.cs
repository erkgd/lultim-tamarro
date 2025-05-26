using UnityEngine;

public class RecollirPinya : MonoBehaviour
{
    public int increment = 4;
    public AudioClip sonidoRecogida;
    [Range(0f, 3f)]
    public float volumen = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Jugador jugador = other.GetComponent<Jugador>();
        if (jugador != null)
            jugador.IncrementarVida(increment);
        else
            Debug.LogWarning("El objeto 'Player' no tiene el componente Jugador.");

        // 1) Creamos un GameObject temporal
        GameObject tempGO = new GameObject("AudioTemp");
        tempGO.transform.position = transform.position;

        // 2) Le añadimos AudioSource
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = sonidoRecogida;
        aSource.volume = volumen;           // puede ser hasta, p.ej., 3
        aSource.spatialBlend = 0f;          // 0 = 2D (sin roll-off)
        aSource.Play();

        // 3) Destruir el AudioSource cuando termine
        Destroy(tempGO, sonidoRecogida.length);

        // 4) Destruir la piña inmediatamente
        Destroy(gameObject);
    }
}
