using UnityEngine;

/// <summary>
/// Componente simple para asignar directamente un tipo de enemigo en el Inspector
/// </summary>
public class TipoEnemigo : MonoBehaviour
{
    // Enum para los tipos de enemigos (más claro en el Inspector)
    public enum TipoEnemics
    {
        Blobo = 0,     // rojo ciclope
        Amanita = 1,   // seta
        Greko = 2,     // volador
        Tatxo = 3      // pinchitos
    }

    [Tooltip("Selecciona el tipo de enemigo directamente")]
    [SerializeField] private TipoEnemics tipoEnemigo = TipoEnemics.Blobo;

    // Referencia al sistema de vida
    private SistemaVidaEnemic sistemaVida;

    private void Awake()
    {
        // Obtener el sistema de vida
        sistemaVida = GetComponent<SistemaVidaEnemic>();
        
        if (sistemaVida != null)
        {
            // Subscribirse al evento de muerte
            sistemaVida.QuanMoriEnemic += RegistrarMuerteEnemigo;
        }
    }

    private void OnDestroy()
    {
        // Limpieza al destruirse
        if (sistemaVida != null)
        {
            sistemaVida.QuanMoriEnemic -= RegistrarMuerteEnemigo;
        }
    }

    /// <summary>
    /// Cuando el enemigo muere, registra su tipo en el contador
    /// </summary>
    private void RegistrarMuerteEnemigo()
    {
        if (SistemaCounter.Instance != null)
        {
            // Registrar con el valor del enum directamente
            SistemaCounter.Instance.RegistrarEnemigoEliminado((int)tipoEnemigo);
        }
    }

    /// <summary>
    /// Retorna el índice del tipo de enemigo configurado
    /// </summary>
    public int ObtenerTipoEnemigo()
    {
        return (int)tipoEnemigo;
    }
}
