using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject vallaCombat; // Objeto a mover desde el inspector+
    public float velocidad = 0.2f; // Velocidad de movimiento
    public Collider entradaCombat;

    private Vector3 vallaCombatPInicial;
    private Vector3 vallaCombatPFinal;
    private bool entratCombat = false;

    // Start is called before the first frame update
    void Start()
    {
        if (vallaCombat != null)
        {
            vallaCombatPInicial = new Vector3(vallaCombat.transform.position.x, 21.67f, vallaCombat.transform.position.z);
            vallaCombatPFinal = new Vector3(vallaCombat.transform.position.x, 26.6f, vallaCombat.transform.position.z);
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

    IEnumerator TancarZonaCombat()
    {
        float tiempo = 0;
        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidad;
            vallaCombat.transform.position = Vector3.Lerp(vallaCombatPInicial, vallaCombatPFinal, tiempo);
            yield return null;
        }
    }
}
