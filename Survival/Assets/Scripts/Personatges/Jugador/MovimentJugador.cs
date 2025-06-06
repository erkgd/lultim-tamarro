using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    // Variables para el nuevo Input System
    private Vector2 inputMovement;
    private bool isSprinting;
    
    // Referencias a las acciones de input
    [SerializeField] private InputActionAsset inputActions; // Asignable desde el inspector
    private InputAction moveAction;
    private InputAction sprintAction;

    private bool useInputSystem = true; // Bandera para determinar qué sistema de input usar
    
    private void Awake()
    {
        jugador = GetComponent<Jugador>();
        characterController = jugador.CharacterController;
        animator = jugador.AnimatorJugador;
        
        InitializeInputSystem();
        
        Debug.Log("MovimentJugador inicializado");
    }

    private void InitializeInputSystem()
    {
        try
        {
            // Buscar el inputActions ya sea por referencia directa o en el componente PlayerInput
            if (inputActions == null)
            {
                var playerInput = GetComponent<PlayerInput>();
                if (playerInput != null && playerInput.actions != null)
                {
                    inputActions = playerInput.actions;
                    Debug.Log("Input Actions obtenido desde PlayerInput");
                }
                else
                {
                    // Intentar buscar el asset en la carpeta de Resources si existe
                    var loadedAsset = Resources.Load<InputActionAsset>("Controls");
                    if (loadedAsset != null)
                    {
                        inputActions = loadedAsset;
                        Debug.Log("Input Actions cargado desde Resources");
                    }
                    else
                    {
                        Debug.LogWarning("No se encontró el InputActionAsset. Se usará input tradicional.");
                        useInputSystem = false;
                        return;
                    }
                }
            }

            // Ahora que tenemos el inputActions, buscamos las acciones específicas
            try
            {
                moveAction = inputActions.FindAction("Player/Move", true);
                sprintAction = inputActions.FindAction("Player/Sprint", true);

                // Registramos callbacks para las acciones de input
                if (moveAction != null)
                {
                    moveAction.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
                    moveAction.canceled += ctx => inputMovement = Vector2.zero;
                    Debug.Log("Acción de movimiento configurada exitosamente");
                }
                else
                {
                    Debug.LogError("Acción 'Move' no encontrada en el InputActionAsset");
                    useInputSystem = false;
                }

                if (sprintAction != null)
                {
                    sprintAction.performed += ctx => isSprinting = true;
                    sprintAction.canceled += ctx => isSprinting = false;
                    Debug.Log("Acción de sprint configurada exitosamente");
                }
                else
                {
                    Debug.LogError("Acción 'Sprint' no encontrada en el InputActionAsset");
                    useInputSystem = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al configurar acciones de input: {e.Message}");
                useInputSystem = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error general al inicializar input system: {e.Message}");
            useInputSystem = false;
        }

        // Si algo falló, usamos el input tradicional
        if (!useInputSystem)
        {
            Debug.LogWarning("Fallback al sistema de input tradicional");
        }
    }

    private void OnEnable()
    {
        if (useInputSystem)
        {
            try
            {
                moveAction?.Enable();
                sprintAction?.Enable();
                Debug.Log("Input Actions habilitadas");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al habilitar acciones de input: {e.Message}");
                useInputSystem = false;
            }
        }
    }

    private void OnDisable()
    {
        if (useInputSystem)
        {
            try
            {
                moveAction?.Disable();
                sprintAction?.Disable();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al deshabilitar acciones de input: {e.Message}");
            }
        }
    }

    public void ConfigurarMoviment(float velocitat, float velocitatRotacio, float velocitatCorrer, float forcaGravetat, float duracioKnockback)
    {
        this.velocitat = velocitat;
        this.velocitatRotacio = velocitatRotacio;
        this.velocitatCorrer = velocitatCorrer;
        this.forcaGravetat = forcaGravetat;
        this.duracioKnockback = duracioKnockback;
        
        Debug.Log($"Movimiento configurado: velocitat={velocitat}, velocitatCorrer={velocitatCorrer}");
    }
    
    public void ActualitzarMoviment()
    {
        Moure();
    }
    
    public void Moure()
    {
        // Inicializar variables para el input
        float h = 0f;
        float v = 0f;
        
        // Determinar qué sistema de input usar
        if (!useInputSystem)
        {
            // Usar input tradicional como respaldo
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
            isSprinting = Input.GetButton("Sprint") || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
        else
        {
            // Usar los valores del Input System
            h = inputMovement.x;
            v = inputMovement.y;
            // isSprinting ya está configurado por los callbacks
        }
        
        // Nova direcció de moviment que barreja ambdós eixos
        Vector3 movementDirection = new(h + v, 0, v - h);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Si está corriendo, aumentar la velocidad
        float currentSpeed = velocitat;
        if (isSprinting)
        {
            currentSpeed = velocitatCorrer;
            animator.SetBool("EstaCorrent", true);
        }
        else
        {
            animator.SetBool("EstaCorrent", false);
        }

        // Assignar la direcció normalitzada
        if (movementDirection.magnitude > 0.1f)
        {
            movementDirection.Normalize();
        }
        direccioMoviment = movementDirection;

        // Calcular velocitat final
        float finalSpeed = magnitude * currentSpeed;

        // Actualitzar velocitat vertical (gravetat)
        if (characterController.isGrounded && ySpeed < 0)
        {
            ySpeed = -0.1f; // Pequeña fuerza para mantener el personaje en el suelo
        }
        else
        {
            ySpeed += Physics.gravity.y * forcaGravetat * Time.deltaTime;
        }

        // Crear vector de velocitat final
        Vector3 velocity = direccioMoviment * finalSpeed;
        velocity.y = ySpeed;
        
        // Afegir l'impuls extern si existeix (knockback)
        if (impulsExtern.magnitude > 0.1f)
        {
            velocity += impulsExtern;
        }
        
        // Moure el personatge
        if (characterController.enabled)
        {
            characterController.Move(velocity * Time.deltaTime);
        }

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
        animator.SetBool("EstaCorrent", false);
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
    
    // Para diagnóstico, útil para debug en builds
    public void ForceTraditionalInput()
    {
        useInputSystem = false;
        Debug.Log("Forzando uso de input tradicional");
    }
}
