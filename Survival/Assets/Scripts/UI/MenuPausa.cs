using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{

    public static bool JocPausat = false;

    public GameObject MenuPausaUI;
    public GameObject MenuConfirmUI;

    void Update()
    {
        // Mirar si volem entrar o sortir del menu
        if (Input.GetButtonDown("Menu"))
        {
            if (JocPausat)
            {
                // Si el joc es troba pausat i fem ESC, el joc es torna a iniciar
                Resume();
            }
            else
            {
                // Si el joc est� en marxa i fem ESC, pausem el joc i mostrem el menu
                Pausa();
            }
        }
        
    }

    public void Resume()
    {
        // Treiem el menu
        MenuPausaUI.SetActive(false);
        // Activem el time del joc perqu� sigui funcionant
        Time.timeScale = 1.0f;
        // Indiquem que el joc torna a estar iniciat
        JocPausat = false;
    }

    void Pausa()
    {
        // Mostrem el menu de pausa
        MenuPausaUI.SetActive(true);
        // Parem el temps del joc, el que fa es parar tot el seu funcinament
        Time.timeScale = 0.0f;
        // Indiquem que el joc es troba pausa
        JocPausat = true;
    }

    
    // Si el jugardor fa restart, l'enviarem al HUB
    public void BotoRestart()
    {
        // Quan faci click, el joc es torna a activar i carreguem l'escena del HUB
        JocPausat = false;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("David");
    }


    public void SortirJoc()
    {
        MenuPausaUI.SetActive(false);
        MenuConfirmUI.SetActive(true);
    }

    public void SortidaConfirmada()
    {
        Application.Quit();
    }

    public void ConfiacioDenegada()
    {
        MenuPausaUI.SetActive(true);
        MenuConfirmUI.SetActive(false);
    }
}
