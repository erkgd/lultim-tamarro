using System.Collections;
using UnityEngine;

// Asegúrate de que requiera SistemaVidaEnemic para suscribirse al evento de muerte
[RequireComponent(typeof(SistemaVidaEnemic))]
public class miniboss_vida : Enemic
{
    private bool perkConcedida = false;
    private SistemaVidaEnemic vidaEnemicComponent;
    private VidaUI vidaUI; // Referencia a la UI de vida

    protected override void Awake()
    {
        base.Awake();

        // Obtener el componente de vida del enemigo
        vidaEnemicComponent = GetComponent<SistemaVidaEnemic>();
        if (vidaEnemicComponent != null)
        {
            vidaEnemicComponent.QuanMoriEnemic += HandleMinibossDeath;
            Debug.Log($"miniboss_vida ({name}): Subscrit a QuanMoriEnemic.");
        }
        else
        {
            Debug.LogError($"miniboss_vida ({name}): No s'ha trobat SistemaVidaEnemic.");
        }

        // Cachear referencia a la UI de vida para actualización inmediata
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI == null)
            Debug.LogWarning($"miniboss_vida ({name}): VidaUI no encontrada en la escena.");
    }

    private void HandleMinibossDeath()
    {
        Debug.Log($"miniboss_vida ({name}): Rebut esdeveniment QuanMoriEnemic.");

        // Solo desbloquear una vez
        if (!perkConcedida)
        {
            if (SistemaPerks.Instance != null)
            {
                // Desbloquear perk de Vida (índex 3)
                SistemaPerks.Instance.DesbloquejarPerk(3);
                perkConcedida = true;
                Debug.Log($"miniboss_vida ({name}): Perk de Vida (índex 3) desbloquejada.");

                // Actualizar UI de corazones al instante
                if (vidaUI != null)
                {
                    if (vidaUI.displayType != VidaUI.DisplayType.Type2)
                        vidaUI.ToggleDisplayType();
                    vidaUI.UpdateHeartsUI();
                    Debug.Log($"miniboss_vida ({name}): UI de vida actualizada a Type2 por perk de Vida.");
                }
            }
            else
            {
                Debug.LogError($"miniboss_vida ({name}): ERROR - SistemaPerks.Instance no encontrado.");
            }
        }
        else
        {
            Debug.Log($"miniboss_vida ({name}): La perk ja havia estat concedida.");
        }
    }
}
