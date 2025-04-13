using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    // Zona de combat
    public GameObject vallaCombat; // Objeto a mover desde el inspector+
    public float velocidad = 0.2f; // Velocidad de movimiento
    public Collider entradaCombat;

    private Vector3 vallaCombatEntradaPInicial;
    private Vector3 vallaCombatEntradaPFinal;
    private bool entratCombat = false;

    public GameObject enemicPractica;
    public GameObject vallaSortidaCombat;
    public GameObject camiSortidaCombat;

    private Vector3 vallaCombatSortidaPInicial;
    private Vector3 vallaCombatSortidaPFinal;

    private Vector3 camiSortidaCombatPInicial;
    private Vector3 camiSortidaCombatPFinal;

    public Collider enemicPracticaCollider;

    // Zona Vida
    public GameObject barreraEntradaVidaEsquerra;
    private Vector3 barreraEntradaVidaEsquerraPInicial;
    private Vector3 barreraEntradaVidaEsquerraPFinal;

    public GameObject barreraEntradaVidaDreta;
    private Vector3 barreraEntradaVidaDretaPInicial;
    private Vector3 barreraEntradaVidaDretaPFinal;

    public Collider entradaVida;
    public Collider zonaVida;

    public GameObject pinya;

    public GameObject barreraSortidaVidaEsquerra;
    private Vector3 barreraSortidaVidaEsquerraPInicial;
    private Vector3 barreraSortidaVidaEsquerraPFinal;

    public GameObject barreraSortidaVidaDreta;
    private Vector3 barreraSortidaVidaDretaPInicial;
    private Vector3 barreraSortidaVidaDretaPFinal;

    // Zona Fogata
    public GameObject arbreSortidaZona;
    private Vector3 arbreSortidaZonaPInicial;
    private Vector3 arbreSortidaZonaPFinal;

    public Collider zonaFogata;


    private float tempsEnFogata = 0f;
    private bool arbreFogataObert = false;



    // Zona Pont
    public CanvasGroup textCanvaGroup;
    public float druacioFade = 1.0f;

    public Collider colliderPontTextEnable;

    public Collider colliderPontTextDisable;


    public GameObject barreraEntradaHub;
    private Vector3 barreraEntradaHubPInicial;
    private Vector3 barreraEntradaHubPFinal;

    public Collider colliderEntradaHub;

    public GameObject pontArribadaHub;
    private Vector3 pontArribadaHubPInicial;
    private Vector3 pontArribadaHubPFinal;

    // Start is called before the first frame update
    void Start()
    {
        if (vallaCombat != null)
        {
            vallaCombatEntradaPInicial  = new Vector3(vallaCombat.transform.position.x, 21.67f, vallaCombat.transform.position.z);
            vallaCombatEntradaPFinal    = new Vector3(vallaCombat.transform.position.x, 26.6f, vallaCombat.transform.position.z);
        }

        if (vallaSortidaCombat != null)
        {
            vallaCombatSortidaPInicial  = new Vector3(vallaSortidaCombat.transform.position.x, 26.67f, vallaSortidaCombat.transform.position.z);
            vallaCombatSortidaPFinal    = new Vector3(vallaSortidaCombat.transform.position.x, 21.6f, vallaSortidaCombat.transform.position.z);
        }

        if (camiSortidaCombat != null)
        {
            camiSortidaCombatPInicial   = new Vector3(camiSortidaCombat.transform.position.x, 25.8f, camiSortidaCombat.transform.position.z);
            camiSortidaCombatPFinal     = new Vector3(camiSortidaCombat.transform.position.x, 26.3f, camiSortidaCombat.transform.position.z);
        }

        if (barreraEntradaVidaDreta != null)
        {
            barreraEntradaVidaDretaPInicial = new Vector3(barreraEntradaVidaDreta.transform.localScale.x, barreraEntradaVidaDreta.transform.localScale.y,14.1998f);
            barreraEntradaVidaDretaPFinal   = new Vector3(barreraEntradaVidaDreta.transform.localScale.x,barreraEntradaVidaDreta.transform.localScale.y,19.9f);
        }

        if (barreraEntradaVidaEsquerra != null)
        {
            barreraEntradaVidaEsquerraPInicial  = new Vector3(barreraEntradaVidaEsquerra.transform.localScale.x, barreraEntradaVidaEsquerra.transform.localScale.y, 13.13898f);
            barreraEntradaVidaEsquerraPFinal    = new Vector3(barreraEntradaVidaEsquerra.transform.localScale.x, barreraEntradaVidaEsquerra.transform.localScale.y, 19.81226f);
        }

        if (barreraSortidaVidaDreta != null)
        {
            barreraSortidaVidaDretaPInicial = new Vector3(barreraSortidaVidaDreta.transform.localScale.x, barreraSortidaVidaDreta.transform.localScale.y, 21.52365f);
            barreraSortidaVidaDretaPFinal = new Vector3(barreraSortidaVidaDreta.transform.localScale.x, barreraSortidaVidaDreta.transform.localScale.y, 13.11996f);
        }

        if (barreraSortidaVidaEsquerra != null) 
        {
            barreraSortidaVidaEsquerraPInicial = new Vector3(barreraSortidaVidaEsquerra.transform.localScale.x, barreraSortidaVidaEsquerra.transform.localScale.y, 17.544f);
            barreraSortidaVidaEsquerraPFinal = new Vector3(barreraSortidaVidaEsquerra.transform.localScale.x, barreraSortidaVidaEsquerra.transform.localScale.y, 14.70195f);
        }

        if (textCanvaGroup != null)
        {
            textCanvaGroup.alpha = 0;
            textCanvaGroup.gameObject.SetActive(false);
        }

        if (arbreSortidaZona != null)
        {
            arbreSortidaZonaPInicial = new Vector3(21.76676f, arbreSortidaZona.transform.position.y, arbreSortidaZona.transform.position.z);
            arbreSortidaZonaPFinal = new Vector3(15.54f, arbreSortidaZona.transform.position.y, arbreSortidaZona.transform.position.z + 5);
        }

        if (barreraEntradaHub != null)
        {
            barreraEntradaHubPInicial = new Vector3(barreraEntradaHub.transform.position.x, 61.26f, barreraEntradaHub.transform.position.z);
            barreraEntradaHubPFinal = new Vector3(barreraEntradaHub.transform.position.x, 66.58437f, barreraEntradaHub.transform.position.z);
        }

        if (pontArribadaHub != null)
        {
            pontArribadaHubPInicial = new Vector3(pontArribadaHub.transform.position.x, pontArribadaHub.transform.position.y, 325.7f);
            pontArribadaHubPFinal = new Vector3(pontArribadaHub.transform.position.x, pontArribadaHub.transform.position.y, 289.1f);

        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (vallaCombat != null & other == entradaCombat & !entratCombat)
        {
            entratCombat = true;
            StartCoroutine(TancarZonaCombat());
            entradaCombat.enabled = false;
        }

        if (barreraEntradaVidaDreta != null & barreraEntradaVidaEsquerra != null & other == entradaVida)
        {
            StartCoroutine(TancarZonaVida());

        }

        if (other == colliderPontTextEnable)
        {
            if (textCanvaGroup != null)
            {
                // Activa el objeto del texto
                textCanvaGroup.gameObject.SetActive(true);
                // Inicia el efecto de fade in.
                StartCoroutine(FadeIn());
            }
        }

        if (other == colliderPontTextDisable)
        {
            if (textCanvaGroup != null)
            {
                StartCoroutine(FadeOut());
            }
        }

        if(other == colliderEntradaHub)
        {
            StartCoroutine(TancarZonaHUB());
            colliderEntradaHub.enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (vallaSortidaCombat != null & other == enemicPracticaCollider)
        {
            if (enemicPractica == null)
            {
                StartCoroutine(ObrirZonaCombat());
                enemicPracticaCollider.enabled = false;
            }
        }

        if (pinya == null & other == zonaVida)
        {
            
            StartCoroutine(ObrirZonaVida());
            zonaVida.enabled = false;
            
        }

        if (other == zonaFogata && !arbreFogataObert)
        {
            tempsEnFogata += Time.deltaTime;

            if (tempsEnFogata >= 3f)
            {
                arbreFogataObert = true;
                StartCoroutine(ObrirZonaFogata());
            }
        }

    }


    private void Update()
    {
        
    }


    IEnumerator TancarZonaCombat()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            vallaCombat.transform.position = Vector3.Lerp(vallaCombatEntradaPInicial, vallaCombatEntradaPFinal, tiempo);
            yield return null;
        }
    }

    IEnumerator TancarZonaVida()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            barreraEntradaVidaDreta.transform.localScale = Vector3.Lerp(barreraEntradaVidaDretaPInicial, barreraEntradaVidaDretaPFinal, tiempo);
            barreraEntradaVidaEsquerra.transform.localScale = Vector3.Lerp(barreraEntradaVidaEsquerraPInicial, barreraEntradaVidaEsquerraPFinal, tiempo);

            yield return null;
        }
    }

    IEnumerator ObrirZonaVida()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            barreraSortidaVidaDreta.transform.localScale = Vector3.Lerp(barreraSortidaVidaDretaPInicial, barreraSortidaVidaDretaPFinal, tiempo);
            barreraSortidaVidaEsquerra.transform.localScale = Vector3.Lerp(barreraSortidaVidaEsquerraPInicial, barreraSortidaVidaEsquerraPFinal, tiempo);

            yield return null;
        }
    }

    IEnumerator ObrirZonaCombat()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            vallaSortidaCombat.transform.position = Vector3.Lerp(vallaCombatSortidaPInicial, vallaCombatSortidaPFinal, tiempo);

            camiSortidaCombat.transform.position = Vector3.Lerp(camiSortidaCombatPInicial, camiSortidaCombatPFinal, tiempo);

            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        float tiempo = 0f;
        while (tiempo < druacioFade)
        {
            tiempo += Time.deltaTime;
            // Calcula el valor de alpha proporcional al tiempo transcurrido.
            textCanvaGroup.alpha = Mathf.Clamp01(tiempo / druacioFade);
            yield return null;
        }
        // Asegura que al final el alpha quede en 1.
        textCanvaGroup.alpha = 1;
    }

    IEnumerator FadeOut()
    {
        float tiempo = 1f;
        float contador = 0f;
        while (contador < druacioFade)
        {
            contador += Time.deltaTime;
            tiempo -= Time.deltaTime;
            textCanvaGroup.alpha = Mathf.Clamp01(druacioFade * tiempo);
            yield return null;
        }
        textCanvaGroup.alpha = 0;

        textCanvaGroup.gameObject.SetActive(false);
    }


    IEnumerator ObrirZonaFogata()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            arbreSortidaZona.transform.position = Vector3.Lerp(arbreSortidaZonaPInicial, arbreSortidaZonaPFinal, tiempo);
            yield return null;
        }
    }

    IEnumerator TancarZonaHUB()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            barreraEntradaHub.transform.position = Vector3.Lerp(barreraEntradaHubPInicial, barreraEntradaHubPFinal, tiempo);
            pontArribadaHub.transform.position = Vector3.Lerp(pontArribadaHubPInicial, pontArribadaHubPFinal, tiempo);

            yield return null;
        }

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            barreraEntradaHub.transform.position = Vector3.Lerp(barreraEntradaHubPInicial, barreraEntradaHubPFinal, tiempo);
            pontArribadaHub.transform.position = Vector3.Lerp(pontArribadaHubPInicial, pontArribadaHubPFinal, tiempo);

            yield return null;
        }
    }
}
