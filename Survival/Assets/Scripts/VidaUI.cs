using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    public enum DisplayType
    {
        Type1, // Modo actual: 6 corazones en una fila
        Type2  // Nuevo modo: 10 corazones en 2 filas de 5 cada una
    }

    [Header("Configuración de Display")]
    public DisplayType displayType = DisplayType.Type1;

    [Header("Configuración de Vida")]
    public int currentLife = 12;

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
        // Configura la vida inicial y la visibilidad de cada grupo según el tipo seleccionado
        if (displayType == DisplayType.Type1)
        {
            currentLife = heartImagesType1.Count * 2;
            foreach (var img in heartImagesType1) { img.gameObject.SetActive(true); }
            foreach (var img in heartImagesType2) { img.gameObject.SetActive(false); }
        }
        else
        {
            currentLife = heartImagesType2.Count * 2;
            foreach (var img in heartImagesType1) { img.gameObject.SetActive(false); }
            foreach (var img in heartImagesType2) { img.gameObject.SetActive(true); }
        }
        UpdateHeartsUI();
    }

    private List<Image> GetHeartList()
    {
        return displayType == DisplayType.Type1 ? heartImagesType1 : heartImagesType2;
    }

    public void UpdateHeartsUI()
    {
        List<Image> heartImages = GetHeartList();
        for (int i = 0; i < heartImages.Count; i++)
        {
            int heartLife = currentLife - (i * 2);
            if (heartLife >= 2)
                heartImages[i].sprite = fullHeartSprite;
            else if (heartLife == 1)
                heartImages[i].sprite = halfHeartSprite;
            else
                heartImages[i].sprite = emptyHeartSprite;
        }
    }

    public void IncreaseLife()
    {
        currentLife++;
        int maxLife = GetHeartList().Count * 2;
        if (currentLife > maxLife)
            currentLife = maxLife;
        UpdateHeartsUI();
    }

    public void DecreaseLife()
    {
        currentLife--;
        if (currentLife < 0)
            currentLife = 0;
        UpdateHeartsUI();
    }

    // Función que alterna el modo de visualización entre Type1 y Type2
    public void ToggleDisplayType()
    {
        if (displayType == DisplayType.Type1)
        {
            displayType = DisplayType.Type2;
            int maxLife = heartImagesType2.Count * 2;
            currentLife = Mathf.Min(currentLife, maxLife);

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
            int maxLife = heartImagesType1.Count * 2;
            currentLife = Mathf.Min(currentLife, maxLife);

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
