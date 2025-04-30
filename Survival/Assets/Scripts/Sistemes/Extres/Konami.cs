using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Konami : MonoBehaviour
{
    private KeyCode[] konamiCode = new KeyCode[]
    {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };

    private int currentIndex = 0;
    private bool codeActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (codeActivated) return;

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(konamiCode[currentIndex]))
            {
                currentIndex++;
                if (currentIndex == konamiCode.Length)
                {
                    ActivarKonamiCode();
                }
            }
            else
            {
                currentIndex = 0;
            }
        }
    }

    private void ActivarKonamiCode()
    {
        codeActivated = true;
        Debug.Log("¡Codi Konami activat!");

        // Desbloquejar tots els Perks
        if (SistemaPerks.Instance != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!SistemaPerks.Instance.EstaDesbloquejada(i))
                {
                    SistemaPerks.Instance.DesbloquejarPerk(i);
                    Debug.Log($"Perk {i} desbloquejat per codi Konami");
                }
            }
        }
        else
        {
            Debug.LogError("No se encontró SistemaPerks.Instance");
        }
    }
}
