// Dins de Scripts/Personatges/miniboss_resistencia.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Assegura't que requereix el SistemaVidaEnemic per poder subscriure's
[RequireComponent(typeof(SistemaVidaEnemic))]
public class miniboss_resistencia : Enemic // Encara hereta d'Enemic
{
    private bool perkConcedida = false;
    private SistemaVidaEnemic vidaEnemicComponent; // Referència al sistema de vida

    // Fem servir Awake per obtenir la referència i subscriure'ns aviat
    protected override void Awake()
    {
        base.Awake(); // Crida a l'Awake de la classe Enemic si en té

        // Obtenim el component SistemaVidaEnemic del mateix GameObject
        vidaEnemicComponent = GetComponent<SistemaVidaEnemic>();

        if (vidaEnemicComponent != null)
        {
            // Ens subscrivim a l'esdeveniment QuanMoriEnemic
            vidaEnemicComponent.QuanMoriEnemic += HandleMinibossDeath;
            Debug.Log($"miniboss_resistencia ({name}): Subscrit a QuanMoriEnemic.");
        }
        else
        {
            Debug.LogError($"miniboss_resistencia ({name}): No s'ha trobat el component SistemaVidaEnemic per subscriure's a l'esdeveniment de mort.");
        }
    }

    // Mètode que s'executarà quan s'invoqui l'esdeveniment QuanMoriEnemic
    private void HandleMinibossDeath()
    {
        Debug.Log($"miniboss_resistencia ({name}): Rebut l'esdeveniment QuanMoriEnemic.");

        // Comprovem si la perk ja s'ha concedit per evitar múltiples crides
        if (!perkConcedida)
        {
            // Intentem accedir al Singleton SistemaPerks per desbloquejar la perk
            if (SistemaPerks.Instance != null)
            {
                // Desbloqueja la perk de Resistència (índex 1)
                SistemaPerks.Instance.DesbloquejarPerk(1);
                perkConcedida = true; // Marquem que ja l'hem donat
                Debug.Log($"miniboss_resistencia ({name}): Perk de Resistència (índex 1) DESBLOQUEJADA pel jugador.");
            }
            else
            {
                // Mostrem un error si no trobem l'instància del SistemaPerks
                Debug.LogError($"miniboss_resistencia ({name}): ERROR - No s'ha trobat SistemaPerks.Instance per desbloquejar la perk.");
            }
        }
        else
        {
             Debug.Log($"miniboss_resistencia ({name}): La perk ja havia estat concedida.");
        }
    }
}
