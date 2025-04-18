using UnityEngine;

public class SalvarAnimal : MonoBehaviour
{
    [Tooltip("Índice de perk a desbloquear al salvar este animal (3 = Vida extra)")]
    [SerializeField] private int indexPerkVidaExtra = 3;
    [SerializeField] private bool mostrarDebug = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        bool yaSalvado = PlayerPrefs.GetInt("AnimalSalvat", 0) == 1;
        if (yaSalvado)
        {
            Debug.Log("ℹ️ Ya habías salvado este animal antes.");
        }
        else
        {
            // Marcar que lo has salvado
            PlayerPrefs.SetInt("AnimalSalvat", 1);
            PlayerPrefs.Save();

            // 1) Desbloquear la perk en SistemaPerks
            SistemaPerks.Instance.DesbloquejarPerk(indexPerkVidaExtra);
            Debug.Log("🎉 Perk de vida desbloqueada (index 3)");

            // 2) Activarla en tiempo real
            var svj = other.GetComponent<SistemaVidaJugador>() 
                ?? FindObjectOfType<SistemaVidaJugador>();
            if (svj != null)
            {
                svj.ActivarPerkVidaExtra();
                Debug.Log("💖 ActivarPerkVidaExtra() invocado desde SalvarAnimal");
            }
            else Debug.LogError("❌ No se encontró SistemaVidaJugador en el Player");
        }

        // 3) Desactivar el animal
        Debug.Log("🐾 Animal salvado: desactivando objeto");
        gameObject.SetActive(false);
    }

}