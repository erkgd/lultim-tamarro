using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class VidaUI : MonoBehaviour
{
    public enum DisplayType
    {
        Type1, // 6 corazones en una fila
        Type2  // 10 corazones en 2 filas de 5 cada una
    }

    [Header("Configuración de Display")]
    public DisplayType displayType = DisplayType.Type1;

    [Header("Elementos de UI para Tipo 1")]
    public List<Image> heartImagesType1;
    [Header("Elementos de UI para Tipo 2")]
    public List<Image> heartImagesType2;

    [Header("Sprites de Corazones")]
    public Sprite fullHeartSprite;
    public Sprite halfHeartSprite;
    public Sprite emptyHeartSprite;

    private SistemaVidaJugador sistemaVida;

    void Awake()
    {
        // 1) Encontrar el sistema de vida
        sistemaVida = FindObjectOfType<SistemaVidaJugador>();
        if (sistemaVida == null)
            Debug.LogError("No se encontró SistemaVidaJugador en la escena.");

        // 2) Suscribirse ANTES de que se llame a DesbloquejarPerk
        if (SistemaPerks.Instance != null)
        {
            SistemaPerks.Instance.OnPerkChanged += HandlePerkChanged;  // :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
            Debug.Log("VidaUI: suscrito a OnPerkChanged en Awake");
        }
    }

    void OnDestroy()
    {
        // Desuscribir para evitar fugas de memoria
        if (SistemaPerks.Instance != null)
            SistemaPerks.Instance.OnPerkChanged -= HandlePerkChanged;
    }

    void Start()
    {
        // 3) Estado inicial de la UI basado en si el perk 3 ya está desbloqueado
        bool perk3Activo = SistemaPerks.Instance != null 
                            && SistemaPerks.Instance.EstaDesbloquejada(3);    // :contentReference[oaicite:2]{index=2}&#8203;:contentReference[oaicite:3]{index=3}
        UpdateDisplayType(perk3Activo);
        UpdateHeartsUI();
    }

    // Se llama inmediatamente cuando DesbloquejarPerk(3) invoca el evento
    private void HandlePerkChanged(int perkIndex)
    {
        if (perkIndex == 3)
        {
            bool isActive = SistemaPerks.Instance.EstaDesbloquejada(3);
            Debug.Log($"VidaUI: evento Perk 3 cambiado a: {(isActive ? "activo" : "inactivo")}");
            UpdateDisplayType(isActive);
            UpdateDisplayType(displayType == DisplayType.Type1);
            UpdateHeartsUI();
        }
    }

    // Aplica el tipo correcto y muestra/oculta los grupos de corazones
    private void UpdateDisplayType(bool useType2)
    {
        displayType = useType2 ? DisplayType.Type2 : DisplayType.Type1;
        foreach (var img in heartImagesType1)
            if (img != null) img.gameObject.SetActive(displayType == DisplayType.Type1);
        foreach (var img in heartImagesType2)
            if (img != null) img.gameObject.SetActive(displayType == DisplayType.Type2);

        Debug.Log($"VidaUI: DisplayType ahora es {displayType}");
    }

    // Actualiza los sprites de acuerdo a la vida actual
    public void UpdateHeartsUI()
    {
        if (sistemaVida == null) return;
        int currentLife = sistemaVida.VidaActual;
        var heartList = (displayType == DisplayType.Type1) 
            ? heartImagesType1 
            : heartImagesType2;

        for (int i = 0; i < heartList.Count; i++)
        {
            if (currentLife >= (i + 1) * 2)
                heartList[i].sprite = fullHeartSprite;
            else if (currentLife >= i * 2 + 1)
                heartList[i].sprite = halfHeartSprite;
            else
                heartList[i].sprite = emptyHeartSprite;
        }
    }

    // Método opcional si quieres alternar manualmente desde un botón
    public void ToggleDisplayType()
    {
        UpdateDisplayType(displayType == DisplayType.Type1);
        UpdateHeartsUI();
    }

    public void UpdateHealth(int vidaActual)
    {
        UpdateHeartsUI();
    }
}