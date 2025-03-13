using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterHealth characterHealth;

    void Start()
    {
        characterHealth = GetComponent<CharacterHealth>(); // Obtiene el componente de salud
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // Asegúrate de que los enemigos tienen el tag "Enemy"
        {
            characterHealth.TakeDamage(2);
        }
    }
}