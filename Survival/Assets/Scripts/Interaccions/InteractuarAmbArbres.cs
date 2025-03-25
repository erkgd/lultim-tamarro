using System.Collections;
using UnityEngine;

public class InteractuarAmbArbre : MonoBehaviour
{
    public float radi = 4.0f;
    public Transform personatge;
    public float velocitatRotacioPinya = 50f;

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

        // Comprova si s'ha premut el clic i si el personatge està assignat
        if (!Input.GetMouseButtonDown(0) || personatge == null) return;

        // Si el personatge està dins del radi i la pinya encara no s'ha "disparat"
        if ((personatge.position - transform.position).sqrMagnitude < radiQuadrat && !pinyadisparada)
        {
            if (pinya != null)
            {
                pinyadisparada = true;
                HabilitarGravedadPinya();
            }
        }
    }

    // Mètode que habilita la gravetat en el Rigidbody de la pinya
    private void HabilitarGravedadPinya()
    {
        Rigidbody rb = pinya.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
        }
        else
        {
            Debug.LogWarning("No s'ha trobat el component Rigidbody a la pinya.");
        }
    }
}
