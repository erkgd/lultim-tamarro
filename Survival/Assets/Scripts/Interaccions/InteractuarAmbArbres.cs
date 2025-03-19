using System.Collections;
using UnityEngine;

public class InteractuarAmbArbre : MonoBehaviour
{
    public float radi = 4.0f;
    public Transform personatge;
    public float velocitatRotacioPinya = 50f;
    public float velocitatReduccioPinya = 2f;

    // Cache del quadrat del radi per evitar càlculs de arrels quadrades
    private float radiQuadrat;
    
    // Referència a la pinya
    private Transform pinya;
    private bool pinyadisparada = false;

    private void Start()
    {
        // Validació de referències
        if (personatge == null)
        {
            Debug.LogWarning("Personatge no assignat a " + gameObject.name);
        }
        
        // Pre-calculem el quadrat del radi
        radiQuadrat = radi * radi;
        
        // Busquem la pinya entre els fills
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Pinya"))
            {
                pinya = child;
                break;
            }
        }
        
        if (pinya == null)
        {
            Debug.LogWarning("No s'ha trobat cap fill amb el tag 'Pinya' en " + gameObject.name);
        }
    }

    void Update()
    {
        // Rotem la pinya al voltant de l'eix vertical
        if (pinya != null)
        {
            pinya.Rotate(0, velocitatRotacioPinya * Time.deltaTime, 0);
        }
        
        // Sortir ràpidament si no hi ha clic o falta el personatge
        if (!Input.GetMouseButtonDown(0) || personatge == null) return;
        
        // Utilitzar sqrMagnitude en comptes de Vector3.Distance per millor rendiment
        if ((personatge.position - transform.position).sqrMagnitude < radiQuadrat && !pinyadisparada)
        {
            if (pinya != null)
            {
                pinyadisparada = true;
                StartCoroutine(EncongirPinya());
            }
        }
    }
    
    private IEnumerator EncongirPinya()
    {
        // Encongim la pinya gradualment
        while (pinya.localScale.x > 0.01f)
        {
            float novaEscala = Mathf.Max(0.01f, pinya.localScale.x - velocitatReduccioPinya * Time.deltaTime);
            pinya.localScale = new Vector3(novaEscala, novaEscala, novaEscala);
            yield return null;
        }
        
        // Un cop la pinya és prou petita, la fem desaparèixer
        pinya.gameObject.SetActive(false);
    }
}