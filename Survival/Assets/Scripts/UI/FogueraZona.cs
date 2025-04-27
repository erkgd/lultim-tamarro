using UnityEngine;

public class FogueraZona : MonoBehaviour
{
    [SerializeField] private float radi = 5f; // Radio de calor de la hoguera
    [SerializeField] private float quantitatAugment = 2f; // Cuánto sube la temperatura por ciclo
    [SerializeField] private float tempsAugment = 1f; // Cada cuánto sube la temperatura

    private TemperaturaUI temperaturaUI;
    private bool jugadorDintre = false;

    private void Start()
    {
        temperaturaUI = FindObjectOfType<TemperaturaUI>();
    }

    private void Update()
    {
        if (jugadorDintre && temperaturaUI != null)
        {
            // El augment es gestiona per corrutina, així que aquí no fem res
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDintre = true;
            StopAllCoroutines();
            StartCoroutine(AugmentarTemperatura());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDintre = false;
            StopAllCoroutines();
        }
    }

    private System.Collections.IEnumerator AugmentarTemperatura()
    {
        while (jugadorDintre)
        {
            temperaturaUI.AugmentarTemperatura(quantitatAugment);
            yield return new WaitForSeconds(tempsAugment);
        }
    }

    // Opcional: dibujar el área de calor en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radi);
    }
}