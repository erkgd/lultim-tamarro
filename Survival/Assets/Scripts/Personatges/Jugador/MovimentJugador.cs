using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Añadido para InputAction

[RequireComponent(typeof(Jugador))]
public class MovimentJugador : MonoBehaviour
{
    private Jugador jugador;
    private CharacterController characterController;
    private Animator animator;
    
    private float velocitat;
    private float velocitatRotacio;
    private float velocitatCorrer;
    private float forcaGravetat;
    
    // Variables para knockback
    private Vector3 impulsExtern = Vector3.zero;
    private float duracioKnockback;
    
    private float ySpeed;
    private Vector3 direccioMoviment;
    private InputAction sprintAction;
    
    private void Awake()
    {
        jugador = GetComponent<Jugador>();
        characterController = jugador.CharacterController;
        animator = jugador.AnimatorJugador;
    }

    private void Start()
    {
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

    public void ConfigurarMoviment(float velocitat, float velocitatRotacio, float velocitatCorrer, float forcaGravetat, float duracioKnockback)
    {
        this.velocitat = velocitat;
        this.velocitatRotacio = velocitatRotacio;
        this.velocitatCorrer = velocitatCorrer;
        this.forcaGravetat = forcaGravetat;
        this.duracioKnockback = duracioKnockback;
    }
    
    public void ActualitzarMoviment()
    {
        Moure();
    }
    
    public void Moure()
    {
        // Obtenim els inputs d'Horizontal i Vertical
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // Comprovar si l'acció de córrer està activa
        bool estaCorrent = sprintAction != null && sprintAction.IsPressed();

        // Nova direcció de moviment que barreja ambdós eixos
        Vector3 movementDirection = new(h + v, 0, v - h);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Si Shift està premut, duplica la velocitat
        float currentSpeed = velocitat;
        if (estaCorrent)
        {
         
            currentSpeed = velocitatCorrer;
            
            animator.SetBool("EstaCorrent", true);
        }
        else
        {
            animator.SetBool("EstaCorrent", false);
        }

        // Assignar la direcció normalitzada
        movementDirection.Normalize();
        direccioMoviment = movementDirection;

        // Calcular velocitat final
        float finalSpeed = magnitude * currentSpeed;

        // Actualitzar velocitat vertical (gravetat)
        ySpeed += Physics.gravity.y * forcaGravetat * Time.deltaTime;

        // Crear vector de velocitat final
        Vector3 velocity = direccioMoviment * finalSpeed;
        velocity.y = ySpeed;
        
        // Afegir l'impuls extern si existeix (knockback)
        if (impulsExtern.magnitude > 0.1f)
        {
            velocity += impulsExtern;
        }

        // Moure el personatge
        characterController.Move(velocity * Time.deltaTime);

        // Rotació cap a la direcció del moviment
        if (direccioMoviment != Vector3.zero)
        {
            animator.SetBool("EstaMoviment", true);

            // Rotació instantània cap a la direcció del moviment
            Quaternion toRotation = Quaternion.LookRotation(direccioMoviment, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, velocitatRotacio * 2 * Time.deltaTime);
        }
        else
        {
            AturarMoviment();
        }
    }
    
    public void AturarMoviment()
    {
        animator.SetBool("EstaMoviment", false);
    }
    
    // Método para aplicar knockback al jugador
    public void AplicarKnockback(Vector3 direccio, float forca)
    {
        impulsExtern = direccio.normalized * forca;
        StartCoroutine(EliminarKnockback());
    }
    
    private IEnumerator EliminarKnockback()
    {
        yield return new WaitForSeconds(duracioKnockback);
        impulsExtern = Vector3.zero;
    }

    public void CanviarVelocitatCorrerPerk(float novaVelocitat)
    {
        velocitatCorrer = novaVelocitat;
        Debug.Log($"MovimentJugador: velocitatCorrer canviada a {novaVelocitat}");
    }

}
