using UnityEngine;
using System;

public class FogueraZona : MonoBehaviour
{
    // Variables de la foguera
    [Header("Configuració de la foguera")]
    [SerializeField] private float radi = 5f; // Radi de calor de la foguera
    [SerializeField] private float quantitatAugment = 5f; // Quantitat que augmenta la temperatura per cicle
    [SerializeField] private float tempsAugment = 1f; // Temps que tarda en augmentar la temperatura

    private TemperaturaUI temperaturaUI;
    private bool jugadorDintre = false;
    private Coroutine reduccioCoroutine;

    // Inicialització del script, troba el component TemperaturaUI
    private void Start()
    {
        try
        {
            temperaturaUI = FindObjectOfType<TemperaturaUI>();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al trobar el component TemperaturaUI: " + e.Message);
        }
    }

    // Actualització del script, no fa res
    private void Update()
    {
        if (jugadorDintre && temperaturaUI != null)
        {
            // El augment es gestiona per corrutina, així que aquí no fem res
        }
    }

    // Quan el jugador entra en el trigger, canvia el bool i crida a la corrutina
    private void OnTriggerEnter(Collider other)
    {
        try
        {
            if (other.CompareTag("Player"))
            {
                jugadorDintre = true;
                StopAllCoroutines();
                // Detener la corrutina de reducción de temperatura
                if (temperaturaUI != null)
                {
                    temperaturaUI.DetenerReduccionTemperatura();
                }
                StartCoroutine(AugmentarTemperatura());
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al trobar el component TemperaturaUI: " + e.Message);
        }
    }

    // Quan el jugador surt del trigger, canvia el bool i para la corrutina
    private void OnTriggerExit(Collider other)
    {
        try
        {
            if (other.CompareTag("Player"))
            {
                jugadorDintre = false;
                StopAllCoroutines();
                // Reiniciar la corrutina de reducción de temperatura
                if (temperaturaUI != null)
                {
                    temperaturaUI.ReiniciarReduccionTemperatura();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al trobar el component TemperaturaUI: " + e.Message);
        }
    }

    // Corrutina que augmenta la temperatura del jugador
    private System.Collections.IEnumerator AugmentarTemperatura()
    {
        while (jugadorDintre)
        {
            try
            {
                temperaturaUI.AugmentarTemperatura(quantitatAugment);
            }
            catch (Exception e)
            {
                Debug.LogError("Error al augmentar la temperatura: " + e.Message);
            }
            yield return new WaitForSeconds(tempsAugment);
        }
    }

    // Opcional: dibuixar el radi de calor en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radi);
    }
}