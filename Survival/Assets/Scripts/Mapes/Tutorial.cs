using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject vallaCombat; // Objeto a mover desde el inspector+
    public float velocidad = 0.2f; // Velocidad de movimiento
    public Collider entradaCombat;

    private Vector3 vallaCombatEntradaPInicial;
    private Vector3 vallaCombatEntradaPFinal;
    private bool entratCombat = false;

    public GameObject enemicPractica;
    public GameObject vallaSortidaCombat;
    private Vector3 vallaCombatSortidaPInicial;
    private Vector3 vallaCombatSortidaPFinal;
    public Collider enemicPracticaCollider;

    // Start is called before the first frame update
    void Start()
    {
        if (vallaCombat != null)
        {
            vallaCombatEntradaPInicial = new Vector3(vallaCombat.transform.position.x, 21.67f, vallaCombat.transform.position.z);
            vallaCombatEntradaPFinal = new Vector3(vallaCombat.transform.position.x, 26.6f, vallaCombat.transform.position.z);
        }

        if (vallaSortidaCombat != null)
        {
            vallaCombatSortidaPInicial = new Vector3(vallaSortidaCombat.transform.position.x, 26.67f, vallaSortidaCombat.transform.position.z);
            vallaCombatSortidaPFinal = new Vector3(vallaSortidaCombat.transform.position.x, 21.6f, vallaSortidaCombat.transform.position.z);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (vallaCombat != null & other == entradaCombat & !entratCombat)
        {
            entratCombat = true;
            StartCoroutine(TancarZonaCombat());
        }

        
    }

    private void OnTriggerStay(Collider other)
    {

        if (vallaSortidaCombat != null & other == enemicPracticaCollider)
        {
            if (enemicPractica != null && !enemicPractica.activeInHierarchy)
            {
                Debug.Log("El objeto enemigo se ha inhabilitado");
                StartCoroutine(ObrirZonaCombat());
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

    IEnumerator ObrirZonaCombat()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            vallaSortidaCombat.transform.position = Vector3.Lerp(vallaCombatSortidaPInicial, vallaCombatSortidaPFinal, tiempo);
            yield return null;
        }
    }
}
