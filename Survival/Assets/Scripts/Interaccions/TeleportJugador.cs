using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// MANUAL DE CONFIGURACIÓ AL UNITY:
// 1. Click dret a l'escena (menu hierarchy) -> Create Empty
// 2. En el inspector d'aquest objecte Add Component > Scripts > TeleportJugador
// 3. Crear dos objectes buids (fills del primer) veure foto --> https://discord.com/channels/1297237163582160966/1297237163582160969/1349798225791549472
// 4. Assignar els dos objectes buids a puntA i puntB i configurar la posició de cada un 
// 5. Ficar el nom de l'escena a la que es vol teletransportar el jugador (ex. Escena Principal)
// 6. Designar la posició on es vol que aparegui el jugador a la nova escena (ex. 0, 0, 0)
// 7. Assignar l'etiqueta del jugador (ex. Player)
// 8. Assignar la distància de detecció (ex. 0.5)
// 9. Assignar si es vol dibuixar la línia a l'editor (ex. true)


// Aquesta classe gestiona el teletransport del jugador quan travessa una línia definida entre dos punts, enviant-lo a una altre escena i col·locant-lo en una posició concreta
public class TeleportJugador : MonoBehaviour
{
    // Secció: Punts de la línia
    [Header("Punts de la línia")]
    public Transform puntA; // Primer extrem de la línia
    public Transform puntB; // Segon extrem de la línia

    // Secció: Destí
    [Header("Destí")]
    public string nomEscenaDestí; // Nom de l'escena on es teletransportarà el jugador
    public Vector3 posicioDestí;   // Posició dins la nova escena on apareixerà el jugador

    // Secció: Configuració
    [Header("Configuració")]
    public string etiquetaJugador = "Player"; // Etiqueta per identificar l'objecte del jugador a l'escena
    public float distanciaDeteccion = 0.7f;      // Distància mínima per considerar que el jugador ha travessat la línia
    public bool dibuixarLineaEnEditor = true;    // Indica si es mostra la línia a l'editor per visualitzar-la

    // Mètode que dibuixa gizmos a l'editor per ajudar a visualitzar la línia
    private void OnDrawGizmos()
    {
        // Comprova que estiguem dibuixant la línia a l'editor i que els punts estiguin definits
        if (dibuixarLineaEnEditor && puntA != null && puntB != null)
        {
            Gizmos.color = Color.red; // Defineix el color del gizmo com a vermell
            Gizmos.DrawLine(puntA.position, puntB.position); // Dibuixa una línia entre els dos punts
        }
    }

    // Mètode que s'executa en cada frame per vigilar si el jugador travessa la línia
    private void Update()
    {
        // Cerca a la escena l'objecte que té la etiqueta definida per al jugador
        GameObject jugador = GameObject.FindGameObjectWithTag(etiquetaJugador);
        if (jugador != null)
        {
            // Si el jugador travessa la línia, es procedeix a teletransportar-lo
            if (JugadorTravessaLínia(jugador.transform.position))
            {
                TeletransportarJugador();
            }
        }
    }

    // Mètode que comprova si la posició del jugador travessa la línia definida pels dos punts
    private bool JugadorTravessaLínia(Vector3 posicioJugador)
    {
        // Assegura que els dos punts de la línia estan definits
        if (puntA == null || puntB == null) return false;

        // Calcula la direcció normalitzada de la línia (de puntA a puntB)
        Vector3 direccioLínia = (puntB.position - puntA.position).normalized;
        
        // Vector que conecta puntA amb la posició del jugador
        Vector3 jugadorAPunt = posicioJugador - puntA.position;
        
        // Determina la projecció del vector jugador-punt sobre la direcció de la línia
        float producteEscalar = Vector3.Dot(jugadorAPunt, direccioLínia);
        
        // Assegura que el jugador es trobi entre els extrems de la línia
        if (producteEscalar >= 0 && producteEscalar <= Vector3.Distance(puntA.position, puntB.position))
        {
            // Calcula el punt de la línia que està més a prop del jugador
            Vector3 puntMesProper = puntA.position + direccioLínia * producteEscalar;
            // Comprova si la distància entre el jugador i aquest punt és suficientment petita
            if (Vector3.Distance(posicioJugador, puntMesProper) <= distanciaDeteccion)
            {
                return true; // El jugador ha travessat la línia
            }
        }
        return false; // No es compleixen les condicions, per tant, el jugador no ha travessat la línia
    }

    // Mètode que teletransporta el jugador a una nova escena
    private void TeletransportarJugador()
    {
        // Emmagatzema la posició de destí als PlayerPrefs per poder-la recuperar a la nova escena
        PlayerPrefs.SetFloat("DestíX", posicioDestí.x);
        PlayerPrefs.SetFloat("DestíY", posicioDestí.y);
        PlayerPrefs.SetFloat("DestíZ", posicioDestí.z);
        PlayerPrefs.SetInt("NecessitaTeleport", 1);

        // Carrega la nova escena utilitzant el nom definit
        SceneManager.LoadScene(nomEscenaDestí);
    }
}
