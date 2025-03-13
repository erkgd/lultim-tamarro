using UnityEngine;

public class InteractuarAmbArbre : MonoBehaviour
{
    public float radi = 4.0f;
    public GameObject arbreNou;
    public Transform personatge;

    // Cache del quadrat del radi per evitar càlculs de arrels quadrades
    private float radiQuadrat;

    private void Start()
    {
        // Validació de referències
        if (personatge == null)
        {
            Debug.LogWarning("Personatge no assignat a " + gameObject.name);
        }
        
        // Pre-calculem el quadrat del radi
        radiQuadrat = radi * radi;
    }

    void Update()
    {
        // Sortir ràpidament si no hi ha clic o falta el personatge
        if (!Input.GetMouseButtonDown(0) || personatge == null) return;
        
        // Utilitzar sqrMagnitude en comptes de Vector3.Distance per millor rendiment
        if ((personatge.position - transform.position).sqrMagnitude < radiQuadrat)
        {
            ReemplaçarArbre();
        }
    }
    
    private void ReemplaçarArbre()
    {
        if (arbreNou != null)
        {
            GameObject nouArbreInstanciat = Instantiate(arbreNou, transform.position, transform.rotation);
            nouArbreInstanciat.transform.localScale = transform.localScale;
        }
        Destroy(gameObject);
    }
}