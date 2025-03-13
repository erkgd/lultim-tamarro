using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractuarAmbArbre : MonoBehaviour
{
    // Radi per detectar si el personatge està prou a prop
    public float radi = 4.0f;
    // Prefab del nou arbre que s'utilitzarà per reemplaçar l'arbre actual
    public GameObject arbreNou;
    // Referència al transform del personatge
    public Transform personatge;

    void Update()
    {
        // Detectem el clic esquerre del ratolí
        if (Input.GetMouseButtonDown(0))
        {
            // Es verifica si el personatge està dins del radi definit respecte a l'arbre.
            if (Vector3.Distance(personatge.position, transform.position) < radi)
            {
                if (arbreNou != null)
                {
                    // Instanciem el nou arbre a la mateixa posició i rotació que l'arbre actual
                    GameObject nouArbreInstanciat = Instantiate(arbreNou, transform.position, transform.rotation);
                    // Copiem també l'escala per mantenir el mateix tamany
                    nouArbreInstanciat.transform.localScale = transform.localScale;
                }
                // Destruïm l'arbre original
                Destroy(gameObject);
            }
        }
    }
}