using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Jugador))]
public class MovimentJugador : MonoBehaviour
{
    private Jugador jugador;
    private CharacterController characterController;
    private Animator animator;
    
    [Header("Configuració Moviment")]
    [SerializeField] private float velocitat = 5f;
    [SerializeField] private float velocitatRotacio = 120f;
    [SerializeField] private float velocitatCorrer = 10f;
    [SerializeField] private float forcaGravetat = 0.1f;
    
    [Header("Knockback")]
    [SerializeField] private float duracioKnockback = 0.25f;
    
    private float ySpeed;
    private Vector3 direccioMoviment;
    private Vector3 impulsExtern = Vector3.zero;
    
    private void Awake()
    {
        jugador = GetComponent<Jugador>();
        characterController = jugador.CharacterController;
        animator = jugador.AnimatorJugador;
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

        // Nova direcció de moviment que barreja ambdós eixos
        Vector3 movementDirection = new(h + v, 0, v - h);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Si Shift està premut, duplica la velocitat
        float currentSpeed = velocitat;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("EstaCorrent", true);
            currentSpeed = velocitatCorrer;
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
    
    public void AplicarKnockback(Vector3 direccio, float forca)
    {
        impulsExtern = direccio.normalized * forca;
        StartCoroutine(DisminuirImpuls());
    }
    
    private IEnumerator DisminuirImpuls()
    {
        float duracio = duracioKnockback;
        float tempsInicial = Time.time;
        Vector3 impulsInicial = impulsExtern;
        
        while (Time.time - tempsInicial < duracio)
        {
            float factor = 1 - ((Time.time - tempsInicial) / duracio);
            impulsExtern = impulsInicial * factor;
            yield return null;
        }
        
        impulsExtern = Vector3.zero;
    }
}
