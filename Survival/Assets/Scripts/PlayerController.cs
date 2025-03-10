using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpSpeed;

    private Animator animator;
    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
    }

    void Update()
    {
        // Obtenemos los inputs de Horizontal y Vertical
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Nueva dirección de movimiento que mezcla ambos ejes
        Vector3 movementDirection = new Vector3(h + v, 0, v - h);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude);

        // Si Shift está presionado, duplica la velocidad
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("EstaCorrent", true);
            currentSpeed *= 2;
        }
        else
        {
            animator.SetBool("EstaCorrent", false);
        }

        movementDirection.Normalize();
        float finalSpeed = magnitude * currentSpeed;

        ySpeed += Physics.gravity.y * Time.deltaTime;

        Vector3 velocity = movementDirection * finalSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("EstaMoviment", true);
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("EstaMoviment", false);
        }
    }
}
