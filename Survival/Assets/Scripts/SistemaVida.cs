using System;
using System.Collections;
using UnityEngine;
public class SistemaVida : MonoBehaviour
{
    private Animator animator; // Referència a l'animador del personatge
    private PlayerMovement playerInput; // Referència al script de moviment del personatge
    private VidaUI vidaUI; // Referència a la UI de vida

    [Header("Vida")]
    public int vidaActual; // Vida actual del personatge
    public int vidaMaxima; // Vida màxima del personatge

    public event Action QuanCanviVida; // Event que es crida quan la vida canvia

    // Per fer una simulació d'aquest sistema, fem que el personatge perdi vida cada 2 segons
    void Start()
    {
        playerInput = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        vidaUI = FindObjectOfType<VidaUI>();

        // Si el personatge conté el tag "Player", li assignem 6 de vida màxima, sinó 3
        if (gameObject.CompareTag("Player"))
            SetVidaMaxima(99);
        else
            SetVidaMaxima(3);

        // Si el personatge està viu, cridem al mètode AutoDecrementarVida cada 2 segons
        //if (EsViu())
        //    InvokeRepeating("AutoDecrementarVida", 2f, 2f);
    }

    // Aquest mètode estableix la vida màxima del personatge
    public void SetVidaMaxima(int novaVidaMaxima)
    {
        vidaMaxima = novaVidaMaxima;
        vidaActual = novaVidaMaxima;
        Debug.Log($"Current life of {gameObject.name} is {vidaActual}");
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // Aquest métode verifica si el personatge està viu
    public bool EsViu()
    {
        return vidaActual > 0;
    }

    // Aquest mètode augmenta la vida del personatge en la quantitat indicada
    public void IncrementarVida(int quantitat, string font)
    {
        // Si la quantitat és negativa, no fem res
        if (quantitat <= 0)
            return;

        vidaActual += quantitat;

        Debug.Log($"Healing {quantitat} applied from {font}"); // Registra el missatge de curació
        Debug.Log($"Current life is {vidaActual}"); // Registra la vida actual

        // Si la vida actual supera la vida màxima, la iguala a la màxima
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Actualitza la UI
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // Aquest mètode disminueix la vida del personatge en la quantitat indicada
    public void DecrementarVida(int quantitat, string font)
    {
        // Si la quantitat és negativa, no fem res
        if (quantitat <= 0)
            return;

        vidaActual -= quantitat;
        animator.SetTrigger("TrRepMal");
        Debug.Log($"{quantitat} damage applied from {font}");
        Debug.Log($"Current life is {vidaActual}");

        // Si la vida actual és inferior a zero, la iguala a zero
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Actualitza la UI
        QuanCanviVida?.Invoke();
        vidaUI.UpdateHealth(vidaActual);
    }

    // Mètode auxiliar per disminuir la vida automàticament
    private void AutoDecrementarVida()
    {
        DecrementarVida(1, "Auto");
    }

    // De moment fem una simulació on fem l'animació de mort, esperem 5 segons i revivim al personatge
    private IEnumerator Morir()
    {
        if (!EsViu())
        {
            animator.SetBool("senseVida", true); // Activem l'animació per indicar que el personatge ha mort  
            Debug.Log($"{gameObject.name} DIET"); // Registra el missatge de mort
        }
        return null;

        // Parem els controls al jugador perquè no es mogui mentres estigui mort  
        //playerInput.enabled = false;
        // Pausa la ejecució durant 5 segons  
        //yield return new WaitForSeconds(5f);

        //// Desactiva la animació  
        //animator.SetBool("senseVida", false);
        //// Tornem a habilitar els controls un cop està viu  
        //playerInput.enabled = true;
        //// Restaurem la vida i actualitzem l'UI  
        //SetVidaMaxima(6);
        //vidaUI.UpdateHealth(vidaActual);
    }
}