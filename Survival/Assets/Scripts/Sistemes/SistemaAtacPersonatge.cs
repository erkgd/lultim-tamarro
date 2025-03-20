using System.Collections;
using UnityEngine;

public class SistemaAtacPersonatge : MonoBehaviour
{
    [Header("Configuració d'Atac")]
    [SerializeField] private int dany = 2; // Dany de l'atac
    [SerializeField] private float tempsAtac = 0.3f; // Temps de duració de l'atac

    private Animator animator;
    private bool atacant = false;
    private BoxCollider boxCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false; // Deshabilitar el Box Collider inicialment
    }

    void Update()
    {
        // Detectar clic dret del ratolí
        if (Input.GetMouseButtonDown(0) && !atacant)
        {
            StartCoroutine(Atacar());
        }
    }

    private IEnumerator Atacar()
    {
        atacant = true;
        // Habilitar el Box Collider durant el temps d'atac
        boxCollider.enabled = true;
        animator.SetTrigger("TrAtac");
        Debug.Log("Atac realitzat");

        // Esperar el temps d'atac
        yield return new WaitForSeconds(tempsAtac);

        // Deshabilitar el Box Collider després del temps d'atac
        boxCollider.enabled = false;
        atacant = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (atacant && other.CompareTag("Enemy"))
        {
            // Aplicar dany a l'enemic
            SistemaVida sistemaVidaEnemic = other.GetComponent<SistemaVida>();
            if (sistemaVidaEnemic != null)
            {
                sistemaVidaEnemic.DecrementarVida(dany, gameObject.name);
                Debug.Log($"Enemic {other.gameObject.name} ha rebut {dany} de dany");
            } else
            {
                Debug.Log($"Enemic {other.gameObject.name} no ha rebut dany");
            }
        }
    }
}