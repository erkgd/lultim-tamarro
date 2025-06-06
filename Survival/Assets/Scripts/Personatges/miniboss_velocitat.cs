// Dins de Scripts/Personatges/miniboss_resistencia.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Assegura't que requereix el SistemaVidaEnemic per poder subscriure's
[RequireComponent(typeof(SistemaVidaEnemic))]
public class miniboss_velocitat : Enemic // Encara hereta d'Enemic
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

        if (!perkConcedida)
        {
            if (SistemaPerks.Instance != null)
            {
                SistemaPerks.Instance.DesbloquejarPerk(0);
                perkConcedida = true;

                // Buscar el jugador i modificar la seva velocitat
                GameObject jugadorObj = GameObject.FindWithTag("Player");
                if (jugadorObj != null)
                {
                    MovimentJugador moviment = jugadorObj.GetComponent<MovimentJugador>();
                    if (moviment != null)
                    {
                        moviment.CanviarVelocitatCorrerPerk(15f); // Per exemple, velocitat alta
                        Debug.Log("miniboss_resistencia: Velocitat de córrer del jugador modificada.");
                    }
                    else
                    {
                        Debug.LogWarning("miniboss_resistencia: No s'ha trobat el component MovimentJugador.");
                    }
                }
                else
                {
                    Debug.LogWarning("miniboss_resistencia: No s'ha trobat cap objecte amb el tag 'Jugador'.");
                }
            }
            else
            {
                Debug.LogError("miniboss_resistencia: ERROR - SistemaPerks.Instance no trobat.");
            }
        }
        else
        {
            Debug.Log("miniboss_resistencia: La perk ja havia estat concedida.");
        }
    }



}
