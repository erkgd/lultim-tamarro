using UnityEngine;
using TMPro;

// Este componente se encarga de mostrar información de los contadores de enemigos derrotados en la UI
public class CounterEnemicsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI textoTotalEnemigos;
    [SerializeField] private TextMeshProUGUI[] textosPorTipo;
    
    [Header("Configuración")]
    [SerializeField] private bool actualizarAutomaticamente = true;
    [SerializeField] private float intervaloActualizacion = 1f;
    
    private SistemaCounter sistemaCounter;
    private float tiempoUltimaActualizacion;
    
    private void Start()
    {
        // Buscar el sistema de contadores
        sistemaCounter = SistemaCounter.Instance;
        
        if (sistemaCounter == null)
        {
            Debug.LogWarning("CounterEnemicsUI: No se ha encontrado SistemaCounter en la escena");
            return;
        }
        
        // Subscribirse a eventos
        sistemaCounter.OnEnemigoDerrotado += (total) => ActualizarUI();
        
        // Configurar el texto principal si existe
        if (textoTotalEnemigos != null)
        {
            sistemaCounter.ConfigurarUITexto(textoTotalEnemigos);
        }
        
        // Actualización inicial
        ActualizarUI();
    }
    
    private void Update()
    {
        if (!actualizarAutomaticamente || sistemaCounter == null) return;
        
        // Actualizar periódicamente
        if (Time.time - tiempoUltimaActualizacion >= intervaloActualizacion)
        {
            ActualizarUI();
            tiempoUltimaActualizacion = Time.time;
        }
    }
    
    // Actualiza todos los elementos de UI con los datos actuales
    public void ActualizarUI()
    {
        if (sistemaCounter == null) return;
        
        // Actualizar contador total
        if (textoTotalEnemigos != null)
        {
            textoTotalEnemigos.text = $"Enemigos derrotados: {sistemaCounter.ObtenerTotalEnemigos()}";
        }
        
        // Actualizar contadores por tipo
        if (textosPorTipo != null && textosPorTipo.Length > 0)
        {
            int[] contadores = sistemaCounter.ObtenerTodosLosContadores();
            
            for (int i = 0; i < textosPorTipo.Length && i < contadores.Length; i++)
            {
                if (textosPorTipo[i] != null)
                {
                    string nombreTipo = sistemaCounter.ObtenerNombreTipoEnemigo(i);
                    textosPorTipo[i].text = $"{nombreTipo}: {contadores[i]}";
                }
            }
        }
    }
}
