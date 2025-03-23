using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

//*************************************************************************************************
// Aquest script s'ocupa de veure a quin enemic ha atacat el personatje i baixar la vida del enemic
//*************************************************************************************************

public class AtacAEnemics : MonoBehaviour
{
    // Radi d'atac que té el personatje
    [SerializeField] private float radiAtacArma;
    // El punt d'atac en aquest cas fa referencia a les mans del tamarro
    [SerializeField] private Transform puntAtacArma;
    // El dany que fará el tamarro al enemic per cada cop
    [SerializeField] private int dany = 1;
    // Agafar el layer del objecte que esta al radi d'atac (En aquest cas enemic)
    [SerializeField] private LayerMask layerObjectiu;

    // Aquesta funció es truca desde Jugador.cs al moment de que el jugador executa un atac per comprobar si hi ha un enemic aprop
    public void DetectarCop()
    {
        // Busquem els colliders dels enemics
        Collider[] cop = Physics.OverlapSphere(puntAtacArma.position, radiAtacArma, layerObjectiu);

        // Si tenim algun enemic en el radi al moment del atac
        if (cop.Length > 0)
        {
            Enemic enemic = cop[0].GetComponent<Enemic>();
            if (enemic != null)
            {
                // Si trobem correctament el enemic, li decrementem la vida que esta al script de Enemic.cs
                enemic.DecrementarVida(dany, gameObject.name);

                // Agafem el component de animacions del enemic al que efectuem el dany per ficar l'animació de mort si vida <= 0
                Animator animatorEnemic = enemic.GetComponent<Animator>();
                if (enemic.VidaActual <= 0)
                {
                    if (animatorEnemic != null)
                    {
                        animatorEnemic.SetBool("EnemicMort", true); // Activa la animació de mort
                    }
                }
            }
        }
    }
    

}
