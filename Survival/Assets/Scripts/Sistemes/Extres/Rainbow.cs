using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rainbow : MonoBehaviour
{
    private KeyCode[] rainbowCode = new KeyCode[]
    {
        KeyCode.R,
        KeyCode.A,
        KeyCode.I,
        KeyCode.N,
        KeyCode.B,
        KeyCode.O,
        KeyCode.W
    };

    private int currentIndex = 0;
    private bool codeActivated = false;
    private float duracionRainbow = 20f; // 20 segundos de efecto arcoíris
    private float tiempoCambioColor = 0.2f; // Cambio de color cada 0.2 segundos
    private Color[] coloresArcoiris = new Color[]
    {
        Color.red,
        new Color(1f, 0.5f, 0f), // Naranja
        Color.yellow,
        Color.green,
        Color.blue,
        new Color(0.5f, 0f, 0.5f), // Púrpura
        new Color(0.5f, 0f, 1f) // Violeta
    };
    private int indiceColorActual = 0;
    private Coroutine coroutineRainbow;
    private SkinnedMeshRenderer[] meshRenderers;

    // Start is called before the first frame update
    void Start()
    {
        // Buscar los renderers del jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            meshRenderers = jugador.GetComponentsInChildren<SkinnedMeshRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (codeActivated) return;

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(rainbowCode[currentIndex]))
            {
                currentIndex++;
                if (currentIndex == rainbowCode.Length)
                {
                    ActivarRainbow();
                }
            }
            else
            {
                currentIndex = 0;
            }
        }
    }

    private void ActivarRainbow()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            Debug.LogError("No s'han trobat els renderers del jugador");
            return;
        }

        codeActivated = true;
        Debug.Log("¡Codi RAINBOW activat! Efecte arcoíris activat.");

        // Iniciar la corutina del efecto arcoíris
        if (coroutineRainbow != null)
        {
            StopCoroutine(coroutineRainbow);
        }
        coroutineRainbow = StartCoroutine(EfecteArcoiris());

        // Programar la desactivación automática
        StartCoroutine(DesactivarRainbow());
    }

    private IEnumerator EfecteArcoiris()
    {
        while (codeActivated)
        {
            // Cambiar al siguiente color
            indiceColorActual = (indiceColorActual + 1) % coloresArcoiris.Length;
            Color colorActual = coloresArcoiris[indiceColorActual];

            // Aplicar el color a todos los renderers
            foreach (var renderer in meshRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = colorActual;
                }
            }

            yield return new WaitForSeconds(tiempoCambioColor);
        }
    }

    private IEnumerator DesactivarRainbow()
    {
        yield return new WaitForSeconds(duracionRainbow);
        
        // Detener la corutina del arcoíris
        if (coroutineRainbow != null)
        {
            StopCoroutine(coroutineRainbow);
        }

        // Restaurar los colores originales
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = Color.white;
            }
        }

        codeActivated = false;
        Debug.Log("Efecte arcoíris desactivat.");
    }
}
