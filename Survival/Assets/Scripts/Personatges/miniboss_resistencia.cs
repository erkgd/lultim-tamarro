using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SistemaVidaEnemic))]
public class miniboss_resistencia : Enemic // hereda d'Enemic
{
    private bool perkConcedida = false;
    private SistemaVidaEnemic vidaEnemicComponent; // referència al sistema de vida

    
    protected override void Awake() // fem servir Awake per obtenir la referència i subscriure'ns aviat
    {
        base.Awake(); // crida a l'Awake de la classe Enemic

        // obtenim el component SistemaVidaEnemic del mateix GameObject
        vidaEnemicComponent = GetComponent<SistemaVidaEnemic>();

        if (vidaEnemicComponent != null)
        {
            // ens subscrivim a l'esdeveniment QuanMoriEnemic
            vidaEnemicComponent.QuanMoriEnemic += HandleMinibossDeath;
            Debug.Log($"miniboss_resistencia ({name}): Subscrit a QuanMoriEnemic.");
        }
        else
        {
            Debug.LogError($"miniboss_resistencia ({name}): No s'ha trobat el component SistemaVidaEnemic per subscriure's a l'esdeveniment de mort.");
        }
    }

    
    private void HandleMinibossDeath()// mètode que s'executarà quan s'invoqui l'esdeveniment QuanMoriEnemic
    {
        Debug.Log($"miniboss_resistencia ({name}): Rebut l'esdeveniment QuanMoriEnemic.");

        // comprovem si el perk ja s'ha concedit per evitar múltiples crides
        if (!perkConcedida)
        {
            // intentem accedir al Singleton SistemaPerks per desbloquejar el perk
            if (SistemaPerks.Instance != null)
            {
                // desbloqueja el perk de Resistència (índex 1)
                SistemaPerks.Instance.DesbloquejarPerk(1);
                perkConcedida = true; // marquem que ja l'hem donat
                Debug.Log($"miniboss_resistencia ({name}): Perk de Resistència (índex 1) DESBLOQUEJADA pel jugador.");
            }
            else
            {
                // mostrem un error si no trobem l'instància del SistemaPerks
                Debug.LogError($"miniboss_resistencia ({name}): ERROR - No s'ha trobat SistemaPerks.Instance per desbloquejar la perk.");
            }
        }
        else
        {
             Debug.Log($"miniboss_resistencia ({name}): La perk ja havia estat concedida.");
        }
    }
}
