using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem;
public class HealthManager : MonoBehaviour
{
    public VidaUI vidaUI;
    public int currentLife = 12;
    public int maxLife;

    public event Action OnHealthChanged;

    private Animator animator;

    private PlayerMovement playerInput;

    public void SetMaxLife(int newMaxLife)
    {
        maxLife = newMaxLife;
        currentLife = newMaxLife;
        OnHealthChanged?.Invoke();
        vidaUI.UpdateHealth(currentLife);

    }

    public void IncreaseLife()
    {
        currentLife++;
        if (currentLife > maxLife)
            currentLife = maxLife;
        OnHealthChanged?.Invoke();
        vidaUI.UpdateHealth(currentLife);

    }

    public void DecreaseLife()
    {
        currentLife--;

        if (currentLife < 0)
            currentLife = 0;

        OnHealthChanged?.Invoke();
        vidaUI.UpdateHealth(currentLife);

        // Si al baixar la vida esta a zero, fem animacio de mort
        if (currentLife == 0)
        {
            StartCoroutine(HandleZeroLife());
        }
    }

    // De moment fem una simulació on fem l'animació de mort, esperem 5 segons i revivim al personatge
    private IEnumerator HandleZeroLife()
    {
        // Activem l'animació per indicar que el personatge ha mort
        animator.SetBool("senseVida", true);
        // Parem els controls al jugador perquè no es mogui mentres estigui mort
        playerInput.enabled = false;
        // Pausa la ejecució durant 5 segons
        yield return new WaitForSeconds(5f);

        // Desactiva la animació
        animator.SetBool("senseVida", false);
        // Tornem a habilitar els controls un cop està viu
        playerInput.enabled = true;
        // Restaurem la vida i actualitzem l'UI
        currentLife = 3;
        vidaUI.UpdateHealth(currentLife);
    }
    
    // Per fer una simulació d'aquest sistema, fem que el personatge perdi vida cada 2 segons
    void Start()
    {
        playerInput = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        InvokeRepeating("DecreaseLife", 2.0f, 2.0f);
    }
}
