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

        // COPILOT: Aplicar la configuración inicial de visibilidad
        ApplyDisplayType();
        // Actualizar UI inicial de corazones
        UpdateHeartsUI();
    }

    /// <summary>
    /// Cambia el modo de visualización (Type1/Type2) y refresca la UI.
    /// </summary>
    public void SetDisplayType(DisplayType type)
    {
        // COPILOT: Ajustar el displayType y actualizar la UI
        displayType = type;
        ApplyDisplayType();
        UpdateHeartsUI();
    }

    /// <summary>
    /// Oculta o muestra cada grupo de corazones según el displayType.
    /// </summary>
    private void ApplyDisplayType()
    {
        // COPILOT: Lógica para alternar entre Type1 y Type2
        bool isType1 = displayType == DisplayType.Type1;
        foreach (var img in heartImagesType1)
            img.gameObject.SetActive(isType1);
        foreach (var img in heartImagesType2)
            img.gameObject.SetActive(!isType1);
    }

    public void UpdateHealth(int vidaActual)
    {
        UpdateHeartsUI();
    }

    private List<Image> GetHeartList()
    {
        return displayType == DisplayType.Type1
            ? heartImagesType1
            : heartImagesType2;
    }

    public void UpdateHeartsUI()
    {
        if (sistemaVida == null) return;

        int currentLife = sistemaVida.VidaActual;
        Debug.Log($"Actualizando UI de vida: currentLife={currentLife}");

        List<Image> heartImages = GetHeartList();
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (currentLife >= (i + 1) * 2)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else if (currentLife >= i * 2 + 1)
            {
                heartImages[i].sprite = halfHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
}