using UnityEngine;
using TMPro;

/// <summary>
/// Componente simple para mostrar la cantidad de enemigos eliminados en la UI
/// </summary>
public class ContadorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoContador;
    
    private void Start()
    {
        // Buscar texto si no está asignado
        if (textoContador == null)
        {
            textoContador = GetComponent<TextMeshProUGUI>();
        }
        
        // Registrar al sistema contador si existe
        if (SistemaCounter.Instance != null && textoContador != null)
        {
            // Registrar el texto para actualizaciones
            SistemaCounter.Instance.OnEnemigoDerrotado += ActualizarTexto;
            
            // Actualización inicial
            ActualizarTexto(SistemaCounter.Instance.ObtenerTotalEnemigos());
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        if (SistemaCounter.Instance != null)
        {
            SistemaCounter.Instance.OnEnemigoDerrotado -= ActualizarTexto;
        }
    }
    
    /// <summary>
    /// Actualiza el texto con el número de enemigos derrotados
    /// </summary>
    private void ActualizarTexto(int total)
    {
        if (textoContador != null)
        {
            textoContador.text = $"Enemigos derrotados: {total}";
        }
    }
}
