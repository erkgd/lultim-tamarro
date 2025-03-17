using System;
using System.Collections;
using UnityEngine;
public class SistemaVida : MonoBehaviour
{
    private Animator animator; // Refer�ncia a l'animador del personatge
    private PlayerMovement playerInput; // Refer�ncia al script de moviment del personatge
    private VidaUI vidaUI; // Refer�ncia a la UI de vida

    [Header("Vida")]
    public int vidaActual; // Vida actual del personatge
    public int vidaMaxima; // Vida m�xima del personatge

    public event Action QuanCanviVida; // Event que es crida quan la vida canvia

    // Per fer una simulaci� d'aquest sistema, fem que el personatge perdi vida cada 2 segons
    void Start()
    {
        playerInput = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        vidaUI = FindObjectOfType<VidaUI>();

        // Si el personatge cont� el tag "Player", li assignem 6 de vida m�xima, sin� 3
        if (gameObject.CompareTag("Player"))
            SetVidaMaxima(6);
        else
            SetVidaMaxima(3);

        // Si el personatge est� viu, cridem al m�tode AutoDecrementarVida cada 2 segons
        //if (EsViu())
        //    InvokeRepeating("AutoDecrementarVida", 2f, 2f);
    }

    // Aquest m�tode estableix la vida m�xima del personatge
    public void SetVidaMaxima(int novaVidaMaxima)
    {
        vidaMaxima = novaVidaMaxima;
        vidaActual = novaVidaMaxima;
        Debug.Log($"Current life of {gameObject.name} is {vidaActual}");
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // Aquest m�tode verifica si el personatge est� viu
    public bool EsViu()
    {
        return vidaActual > 0;
    }

    // Aquest m�tode augmenta la vida del personatge en la quantitat indicada
    public void IncrementarVida(int quantitat, string font)
    {
        // Si la quantitat �s negativa, no fem res
        if (quantitat <= 0)
            return;

        vidaActual += quantitat;

        Debug.Log($"Healing {quantitat} applied from {font}"); // Registra el missatge de curaci�
        Debug.Log($"Current life is {vidaActual}"); // Registra la vida actual

        // Si la vida actual supera la vida m�xima, la iguala a la m�xima
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Actualitza la UI
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // Aquest m�tode disminueix la vida del personatge en la quantitat indicada
    public void DecrementarVida(int quantitat, string font)
    {
        // Si la quantitat �s negativa, no fem res
        if (quantitat <= 0)
            return;

        vidaActual -= quantitat;
        animator.SetTrigger("TrRepMal");
        Debug.Log($"{quantitat} damage applied from {font}");
        Debug.Log($"Current life is {vidaActual}");

        // Si la vida actual �s inferior a zero, la iguala a zero
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Actualitza la UI
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // M�tode auxiliar per disminuir la vida autom�ticament
    private void AutoDecrementarVida()
    {
        DecrementarVida(1, "Auto");
    }

    // De moment fem una simulaci� on fem l'animaci� de mort, esperem 5 segons i revivim al personatge
    private IEnumerator Morir()
    {
        if (!EsViu())
        {
            animator.SetBool("senseVida", true); // Activem l'animaci� per indicar que el personatge ha mort  
            Debug.Log($"{gameObject.name} DIET"); // Registra el missatge de mort

            // Obtenim la duraci� de l'animaci� de mort
            float duracioAnimacio = animator.GetCurrentAnimatorStateInfo(0).length;

            // Esperem a que l'animaci� acabi
            yield return new WaitForSeconds(duracioAnimacio);

            // Desactivem el GameObject
            gameObject.SetActive(false);
        }

        // Parem els controls al jugador perqu� no es mogui mentres estigui mort  
        //playerInput.enabled = false;
        // Pausa la ejecuci� durant 5 segons  
        //yield return new WaitForSeconds(5f);

        //// Desactiva la animaci�  
        //animator.SetBool("senseVida", false);
        //// Tornem a habilitar els controls un cop est� viu  
        //playerInput.enabled = true;
        //// Restaurem la vida i actualitzem l'UI  
        //SetVidaMaxima(6);
        //vidaUI.UpdateHealth(vidaActual);
    }
}