using UnityEngine;
using System.Collections;

public class SaveAnimal : MonoBehaviour
{
    [Header("Efectos y Sonido")]
    [Tooltip("Prefab de partículas que se instanciará al salvar.")]
    [SerializeField] private GameObject saveEffect;
    [Tooltip("AudioClip que se reproducirá al salvar.")]
    [SerializeField] private AudioClip saveSound;

    private AudioSource audioSource;
    private VidaUI vidaUI;

    void Awake()
    {
        // Obtener o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;

        // Cachear referencia a la UI de vida para actualización inmediata
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI == null)
            Debug.LogWarning("SaveAnimal: VidaUI no encontrada en la escena.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Desbloquear perk 3 si aún no está activo
        if (SistemaPerks.Instance != null && !SistemaPerks.Instance.EstaDesbloquejada(3))
        {
            SistemaPerks.Instance.DesbloquejarPerk(3);
            Debug.Log("SaveAnimal: Perk 3 desbloqueado");
        }

        // Actualizar la UI de corazones a Type2 al instante
        if (vidaUI != null)
        {
            // Si aún no está en Type2, alternar
            if (vidaUI.displayType != VidaUI.DisplayType.Type2)
                vidaUI.ToggleDisplayType();
            vidaUI.UpdateHeartsUI();
        }

        // Instanciar efecto visual si está asignado
        if (saveEffect != null)
        {
            GameObject fx = Instantiate(saveEffect, transform.position, Quaternion.identity);
            Destroy(fx, 4f);
        }

        // Reproducir sonido y desactivar después
        if (saveSound != null)
        {
            audioSource.PlayOneShot(saveSound);
            StartCoroutine(DisableAfterSound());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DisableAfterSound()
    {
        yield return new WaitForSeconds(saveSound.length);
        gameObject.SetActive(false);
    }
}
