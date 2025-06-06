using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Intro : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Clip de música para la introducción")]
    public AudioClip musica;
    
    [Tooltip("Componente AudioSource para reproducir la música")]
    public AudioSource audioSource;

    [Header("Configuración")]
    [Tooltip("Nombre de la escena a cargar después de la intro")]
    [SerializeField] private string siguienteEscena = "David";

    void Start()
    {
        // Iniciar la corrutina que manejará la secuencia de introducción
        StartCoroutine(ReproducirIntro());
    }

    private IEnumerator ReproducirIntro()
    {
        // Verificar que tenemos los componentes necesarios
        if (audioSource != null && musica != null)
        {
            // Configurar y reproducir la música
            audioSource.clip = musica;
            audioSource.Play();
            
            Debug.Log($"Reproduciendo intro: {musica.name}, duración: {musica.length} segundos");
            
            // Esperar a que termine la música
            yield return new WaitForSeconds(musica.length);
        }
        else
        {
            Debug.LogWarning("Intro: Falta AudioSource o AudioClip. Saltar intro.");
            yield return new WaitForSeconds(2.0f); // Esperar un poco si no hay música
        }
        
        // Cargar la siguiente escena
        Debug.Log($"Intro finalizada. Cargando escena: {siguienteEscena}");
        SceneManager.LoadScene(siguienteEscena);
    }
}