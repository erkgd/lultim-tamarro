using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPausa : MonoBehaviour
{

    public static bool JocPausat = false;

    public GameObject MenuPausaUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (JocPausat)
            {
                Resume();
            }
            else
            {
                Pausa();
            }
        }
        
    }

    public void Resume()
    {
        MenuPausaUI.SetActive(false);
        Time.timeScale = 1.0f;
        JocPausat = false;
    }

    void Pausa()
    {
        MenuPausaUI.SetActive(true);
        Time.timeScale = 0.0f;
        JocPausat = true;
    }
}
