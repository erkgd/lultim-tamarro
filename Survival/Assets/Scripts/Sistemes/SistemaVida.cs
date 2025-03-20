using System;
using System.Collections;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    [Header("Referencias")]
    private Animator animator;
    private PlayerMovement playerInput;
    private VidaUI vidaUI;
    private Cortinilla Cortinilla;

    [Header("Vida")]
    public int vidaActual;
    public int vidaMaxima;

    [Header("Invencibilitat")]
    [SerializeField] private float tempsInvencibilitat = 1.5f;
    private bool esInvencible = false;
    [SerializeField] private bool mostrarDebug = true;

    // Event que es dispara quan canvia la vida
    public event Action QuanCanviVida;

    void Start()
    {
        // Inicialitzem components
        playerInput = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        vidaUI = FindObjectOfType<VidaUI>();
        Cortinilla = FindObjectOfType<Cortinilla>();

        // Establim la vida inicial segons si és jugador o no
        if (gameObject.CompareTag("Player"))
            SetVidaMaxima(12);
        else
            SetVidaMaxima(3);
    }

    // Estableix la vida màxima i actual
    public void SetVidaMaxima(int novaVidaMaxima)
    {
        vidaMaxima = novaVidaMaxima;
        vidaActual = novaVidaMaxima;
        
        if (mostrarDebug) 
            Debug.Log($"Current life of {gameObject.name} is {vidaActual}");
        
        // Notifiquem el canvi de vida
        QuanCanviVida?.Invoke();
        if (vidaUI != null) 
            vidaUI.UpdateHealth(vidaActual);
    }

    // Comprova si el personatge està viu
    public bool EsViu()
    {
        return vidaActual > 0;
    }

    // Incrementa la vida en la quantitat indicada
    public void IncrementarVida(int quantitat, string font)
    {
        // Validacions inicials
        if (quantitat <= 0)
            return;

        vidaActual += quantitat;

        if (mostrarDebug)
        {
            Debug.Log($"Healing {quantitat} applied from {font}");
            Debug.Log($"Current life is {vidaActual}");
        }

        // Limitem la vida màxima
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Actualitzem UI
        QuanCanviVida?.Invoke();
        if (vidaUI != null) 
            vidaUI.UpdateHealth(vidaActual);
    }

    // Decrementa la vida en la quantitat indicada
    public void DecrementarVida(int quantitat, string font)
    {
        // No apliquem dany si és invencible o la quantitat és negativa
        if (quantitat <= 0 || esInvencible)
            return;

        vidaActual -= quantitat;
        
        // Activem l'animació de rebre mal
        if (animator != null)
            animator.SetTrigger("TrRepMal");
        
        if (mostrarDebug)
        {
            Debug.Log($"{quantitat} damage applied from {font}");
            Debug.Log($"Current life is {vidaActual}");
        }

        // Activem el període d'invencibilitat
        StartCoroutine(PeriodeInvencibilitat());

        // Si la vida arriba a 0 o menys, iniciem el procés de mort
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Actualitzem UI
        QuanCanviVida?.Invoke();
        if (vidaUI != null) 
            vidaUI.UpdateHealth(vidaActual);
    }

    // Corrutina per gestionar el període d'invencibilitat
    private IEnumerator PeriodeInvencibilitat()
    {
        esInvencible = true;
        
        // Activem l'animació d'invencibilitat
        if (animator != null)
        {
            animator.SetTrigger("Invencibilitat");
        }
        
        if (mostrarDebug) 
            Debug.Log($"{gameObject.name} és invencible durant {tempsInvencibilitat} segons");
        
        // Esperem el temps d'invencibilitat
        yield return new WaitForSeconds(tempsInvencibilitat);
        
        // Desactivem la invencibilitat
        esInvencible = false;
        
        if (mostrarDebug) 
            Debug.Log($"{gameObject.name} ja no és invencible");
    }

    // Corrutina per gestionar la mort del personatge
    private IEnumerator Morir()
    {
        if (!EsViu())
        {
            if (animator != null)
                animator.SetBool("senseVida", true);
                
            Debug.Log($"{gameObject.name} DIED");
            
            // Activamos la cortinilla de muerte si existe
            if (Cortinilla != null)
            {
                Cortinilla.MostrarCortinilla();
            }

            // Obtenim la duració de l'animació de mort
            float duracioAnimacio = 2.0f;
            if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Mort"))
            {
                duracioAnimacio = animator.GetCurrentAnimatorStateInfo(0).length;
            }

            // Esperem a que l'animació acabi
            yield return new WaitForSeconds(duracioAnimacio);

            // Desactivem el GameObject si no és el jugador
            if (!gameObject.CompareTag("Player"))
            {
                gameObject.SetActive(false);
                yield break; // Sortim si no és el jugador
            }
        }

        // Només per al jugador: gestionem la seva mort amb revival
        if (gameObject.CompareTag("Player"))
        {
            // Parem els controls al jugador perquè no es mogui mentres estigui mort  
            if (playerInput != null)
                playerInput.enabled = false;
                
            // Pausa la execució durant 5 segons  
            yield return new WaitForSeconds(5f);

            // Desactiva la animació de mort
            if (animator != null)
                animator.SetBool("senseVida", false);
                
            // Tornem a habilitar els controls un cop està viu  
            if (playerInput != null)
                playerInput.enabled = true;
                
            // Restaurem la vida i actualitzem l'UI  
            SetVidaMaxima(6);
            if (vidaUI != null)
                vidaUI.UpdateHealth(vidaActual);
        }
    }
}