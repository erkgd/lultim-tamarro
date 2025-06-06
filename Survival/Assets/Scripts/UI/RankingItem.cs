using UnityEngine;
using TMPro;

public class RankingItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoEnemigos;

    public void Configurar(string nombre, string tiempo, string enemigos)
    {
        if (textoNombre != null) textoNombre.text = nombre;
        if (textoTiempo != null) textoTiempo.text = tiempo;
        if (textoEnemigos != null) textoEnemigos.text = enemigos;
    }
} 