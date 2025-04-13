using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VidaUI : MonoBehaviour
{
    public enum DisplayType
    {
        Type1, // Modo actual: 6 corazones en una fila
        Type2  // Nuevo modo: 10 corazones en 2 filas de 5 cada una
    }

    [Header("Configuración de Display")]
    public DisplayType displayType = DisplayType.Type1;

    // Referencia al sistema de vida del jugador
    private SistemaVidaJugador sistemaVida;

    [Header("Elementos de UI para Tipo 1")]
    public List<Image> heartImagesType1;

    [Header("Elementos de UI para Tipo 2")]
    public List<Image> heartImagesType2;

    [Header("Sprites de Corazones")]
    public Sprite fullHeartSprite;
    public Sprite halfHeartSprite;
    public Sprite emptyHeartSprite;

    void Start()
    {
        // Obtener referencia al sistema de vida del jugador
        sistemaVida = FindObjectOfType<SistemaVidaJugador>();
        if (sistemaVida == null)
        {
            Debug.LogError("No se encontró SistemaVidaJugador en la escena.");
        }
        
        // Configura la visibilidad de cada grupo según el tipo seleccionado
        if (displayType == DisplayType.Type1)
        {
            foreach (var img in heartImagesType1) { img.gameObject.SetActive(true); }
            foreach (var img in heartImagesType2) { img.gameObject.SetActive(false); }
        }
        else
        {
            foreach (var img in heartImagesType1) { img.gameObject.SetActive(false); }
            foreach (var img in heartImagesType2) { img.gameObject.SetActive(true); }
        }
        
        // Actualizar UI inicial
        UpdateHeartsUI();
    }

    public void UpdateHealth(int vidaActual)
    {
        UpdateHeartsUI();
    }

    private List<Image> GetHeartList()
    {
        return displayType == DisplayType.Type1 ? heartImagesType1 : heartImagesType2;
    }

    public void UpdateHeartsUI()
    {
        // Si no hay sistema de vida, no podemos actualizar la UI
        if (sistemaVida == null) return;
        
        int currentLife = sistemaVida.VidaActual;
        
        // Log para depuración
        Debug.Log($"Actualizando UI de vida: currentLife={currentLife}");
        
        List<Image> heartImages = GetHeartList();
        
        // Recorremos cada corazón
        for (int i = 0; i < heartImages.Count; i++)
        {
            // LÓGICA ACTUALIZADA:
            // Cada corazón representa 4 puntos de vida (antes eran 2).
            // Calcular cuántos puntos de vida corresponden a este corazón.
            
            if (currentLife >= (i + 1) * 4)
            {
                // Corazón completo: Si la vida es suficiente para llenar este corazón
                heartImages[i].sprite = fullHeartSprite;
                Debug.Log($"Corazón {i}: COMPLETO (vida={currentLife}, índice={i*4})");
            }
            else if (currentLife >= i * 4 + 2)
            {
                // Medio corazón: Si la vida es mayor o igual a la mitad de este corazón
                heartImages[i].sprite = halfHeartSprite;
                Debug.Log($"Corazón {i}: MITAD (vida={currentLife}, índice={i*4})");
            }
            else
            {
                // Corazón vacío: Si no hay suficiente vida para este corazón
                heartImages[i].sprite = emptyHeartSprite;
                Debug.Log($"Corazón {i}: VACÍO (vida={currentLife}, índice={i*4})");
            }
        }
    }

    // Función que alterna el modo de visualización entre Type1 y Type2
    public void ToggleDisplayType()
    {
        if (displayType == DisplayType.Type1)
        {
            displayType = DisplayType.Type2;
            
            foreach (var img in heartImagesType1)
            {
                img.gameObject.SetActive(false);
            }
            foreach (var img in heartImagesType2)
            {
                img.gameObject.SetActive(true);
            }
        }
        else
        {
            displayType = DisplayType.Type1;
            
            foreach (var img in heartImagesType1)
            {
                img.gameObject.SetActive(true);
            }
            foreach (var img in heartImagesType2)
            {
                img.gameObject.SetActive(false);
            }
        }
        UpdateHeartsUI();
    }
}