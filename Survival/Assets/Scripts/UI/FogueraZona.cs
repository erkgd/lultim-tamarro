using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
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
    private SphereCollider sphereCollider;
    
    // Variable per mantenir referència a l'àudio actual
    private GameObject audioActual;

    // Efecte de so quan estigui el jugador dins de la zona de calor
    public AudioClip efecteSo;
    [Range(0f, 3f)]
    public float volum = 1f;

    // Inicialització del script, troba el component TemperaturaUI
    private void Start()
    {
        try
        {
            temperaturaUI = FindObjectOfType<TemperaturaUI>();
            sphereCollider = GetComponent<SphereCollider>();
            ActualitzarCollider();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al trobar el component TemperaturaUI: " + e.Message);
        }
    }

    private void OnValidate()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();
            
        ActualitzarCollider();
    }

    private void ActualitzarCollider()
    {
        if (sphereCollider != null)
        {
            sphereCollider.radius = radi;
            sphereCollider.isTrigger = true;
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
                
                // Crear l'àudio una sola vegada en entrar
                CrearAudio();
                
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
                
                // Parar l'àudio actual si s'està reproduint
                if (audioActual != null)
                {
                    Destroy(audioActual);
                    audioActual = null;
                }
                
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
    
    // Mètode per crear l'àudio
    private void CrearAudio()
    {
        if (efecteSo != null)
        {
            // 1) Creem un GameObject temporal
            audioActual = new GameObject("AudioTemp");
            audioActual.transform.position = transform.position;

            // 2) Fiquem el AudioSource
            AudioSource aSource = audioActual.AddComponent<AudioSource>();
            aSource.clip = efecteSo;
            aSource.volume = volum;
            aSource.spatialBlend = 0f; // 0 = 2D (sense roll-off)
            aSource.loop = true; // Fer que l'àudio es reprodueixi en bucle
            aSource.Play();
        }
    }

    // Opcional: dibuixar el radi de calor en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radi);
    }
}