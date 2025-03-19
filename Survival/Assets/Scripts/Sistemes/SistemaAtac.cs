using UnityEngine;

public class SistemaAtac : MonoBehaviour
{
    private Animator animator; // Referència a l'animador del personatge
    private GameObject player;
    private SistemaVida sistemaVidaJugador;

    private void Start()
    {
        animator = GetComponent<Animator>();
        // Obtenim el jugador i el sistema de vida del jugador
        player = GameObject.FindGameObjectWithTag("Player");
        //movimentEnemics = GetComponent<MovimentEnemics>();
        if (player != null)
            sistemaVidaJugador = player.GetComponent<SistemaVida>();
    }

    // Aquest mètode s'executarà quan el jugador sigui atacat
    public void AplicarDany(int quantitat, string nomEnemic)
    {
        animator.SetTrigger("TrAtac");
        // Si el sistema de vida del jugador existe y el jugador está vivo
        if (sistemaVidaJugador != null && sistemaVidaJugador.EsViu())
            sistemaVidaJugador.DecrementarVida(quantitat, nomEnemic);
    }
}
