using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    public int maxLife = 12;
    public int currentLife;
    private HealthUI healthUI; // Referencia a la UI de vida

    void Start()
    {
        currentLife = maxLife;
        Debug.Log("Vida inicial del personaje: " + currentLife);

        healthUI = FindObjectOfType<HealthUI>(); // Asigna la referencia una sola vez
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        Debug.Log("Vida restante: " + currentLife);

        if (currentLife <= 0)
        {
            currentLife = 0;
        }
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHeartsUI();
        }
        else
        {
            Debug.LogWarning("HealthUI no encontrado en la escena.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "TurtleShellPBR" || collision.gameObject.name == "SlimePBR")
        {
            TakeDamage(4); // 4 puntos de vida (equivalente a 2 corazones)
            Debug.Log("Colisión con " + collision.gameObject.name + ". Vida actual: " + currentLife);
        }
    }
}
