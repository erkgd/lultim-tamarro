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

    void Awake()
    {
        // Asegurarnos de tener un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 1) Desbloquear el perk 3 para cambiar la UI de Type1 a Type2
        if (SistemaPerks.Instance != null && !SistemaPerks.Instance.EstaDesbloquejada(3))
        {
            SistemaPerks.Instance.DesbloquejarPerk(3);
            Debug.Log("SaveAnimal: Perk 3 desbloqueada, UI debería actualizarse a Type2");
        }

        // 2) Instanciar efecto visual
        if (saveEffect != null)
        {
            GameObject fx = Instantiate(saveEffect, transform.position, Quaternion.identity);
            Destroy(fx, 4f);
        }

        // 3) Reproducir sonido y desactivar objeto tras el clip
        if (saveSound != null)
        {
            audioSource.PlayOneShot(saveSound);
            StartCoroutine(DisableAfterSound());
        }
        else
        {
            // Si no hay sonido, desactivar inmediatamente
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DisableAfterSound()
    {
        yield return new WaitForSeconds(saveSound.length);
        gameObject.SetActive(false);
    }
}
