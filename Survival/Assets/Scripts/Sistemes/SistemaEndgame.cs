// filepath: c:\Users\aleja\Desktop\BINFO\VIDEOJOCS\lultim-tamarro\Survival\Assets\Scripts\Sistemes\SistemaEndgame.cs
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Sistema que gestiona el final del juego y envía estadísticas al servidor 
/// cuando el jugador desbloquea todos los perks
/// </summary>
public class SistemaEndgame : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaEndgame Instance { get; private set; }
    
    [Header("Configuración")]
    [SerializeField] private bool mostrarDebug = true;  // Mostrar mensajes de debug
    
    // Endpoint para enviar los datos
    private string endpoint = "http://localhost:8080/puntuacions/";

    // Tracking de estado
    private bool todosPerksDesbloqueados = false;
    private bool datosEnviados = false;
    
    // Evento para notificar cuando se completa el juego
    public event Action OnJuegoCompletado;
    
    private void Awake()
    {
        // Configurar el singleton
        Instance = this;
        
        if (mostrarDebug)
            Debug.Log("SistemaEndgame: Sistema inicializado.");
    }
    private void Start()
    {
        // Comprobar perks solo una vez al iniciar
        ComprobarPerks();
        
        if (mostrarDebug)
            Debug.Log("SistemaEndgame: Sistema iniciado. Comprobando perks al inicio.");
    }

    /// <summary>
    /// Comprueba si todos los perks están desbloqueados y envía datos si es necesario
    /// </summary>
    public void ComprobarPerks()
    {
        // Si ya se completó el juego, no seguir comprobando
        if (datosEnviados)
            return;

        // Verificar que todos los sistemas necesarios estén disponibles
        if (SistemaPerks.Instance == null)
        {
            Debug.LogWarning("SistemaEndgame: SistemaPerks no encontrado.");
            return;
        }

        // Comprobar si todos los perks están desbloqueados
        bool todosDesbloqueados = ComprobarTodosPerksDesbloqueados();

        if (todosDesbloqueados && !todosPerksDesbloqueados)
        {
            // Primera vez que detectamos todos los perks desbloqueados
            todosPerksDesbloqueados = true;
            
            if (mostrarDebug)
                Debug.Log("SistemaEndgame: ¡Todos los perks están desbloqueados! Enviando datos al servidor...");
            
            // Enviar datos al servidor
            EnviarDatosAlServidor();
            
            // Notificar que el juego se ha completado
            OnJuegoCompletado?.Invoke();
        }
    }

    /// <summary>
    /// Comprueba si todos los perks están desbloqueados
    /// </summary>
    private bool ComprobarTodosPerksDesbloqueados()
    {
        // El SistemaPerks tiene 4 perks definidos (0: Velocitat, 1: Resistència, 2: Atac, 3: Vida)
        for (int i = 0; i < 4; i++)
        {
            if (!SistemaPerks.Instance.EstaDesbloquejada(i))
            {
                if (mostrarDebug)
                    Debug.Log($"SistemaEndgame: Perk {SistemaPerks.Instance.NomPerk(i)} aún no desbloqueado.");
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Envía los datos de tiempo y enemigos al servidor
    /// </summary>
    private void EnviarDatosAlServidor()
    {
        // Obtener referencia a SistemaCrono
        SistemaCrono cronometro = FindObjectOfType<SistemaCrono>();
        if (cronometro == null)
        {
            Debug.LogError("SistemaEndgame: No se pudo encontrar SistemaCrono.");
            return;
        }
        
        // Obtener el tiempo transcurrido
        float tiempoJuego = cronometro.GetElapsedTime();
        
        // Obtener conteo de enemigos
        int enemigosEliminados = 0;
        if (SistemaCounter.Instance != null)
        {
            enemigosEliminados = SistemaCounter.Instance.ObtenerTotalEnemigos();
        }        else
        {
            Debug.LogWarning("SistemaEndgame: SistemaCounter no encontrado.");
        }
        
        // Crear objeto de datos para enviar
        EndgameData datos = new EndgameData
        {
            nom_usuari = "ERK",
            tiemps_jugat = (int)tiempoJuego,
            enemics_derrotats = enemigosEliminados,
        };
          // Convertir a JSON
        string jsonData = JsonUtility.ToJson(datos);
        
        // Log detallado del objeto y la URL antes de enviar
        Debug.Log($"[SistemaEndgame] Enviando datos a {endpoint}:");
        Debug.Log($"[SistemaEndgame] JSON: {jsonData}");
        Debug.Log($"[SistemaEndgame] nom_usuari: {datos.nom_usuari}, tiemps_jugat: {datos.tiemps_jugat}, enemics_derrotats: {datos.enemics_derrotats}");
        
        if (mostrarDebug)
            Debug.Log($"SistemaEndgame: Enviando datos al servidor: {jsonData}");
        
        // Enviar datos usando HttpSystem
        if (HttpSystem.Instance != null)
        {
            HttpSystem.Instance.PostRequest(endpoint, jsonData, OnDatosEnviados);
        }
        else
        {
            Debug.LogError("SistemaEndgame: HttpSystem no encontrado.");
        }
    }
    
    /// <summary>
    /// Callback cuando se recibe respuesta del servidor
    /// </summary>
    private void OnDatosEnviados(string respuesta)
    {
        if (respuesta != null)
        {
            datosEnviados = true;
            Debug.Log($"SistemaEndgame: Datos enviados correctamente. Respuesta: {respuesta}");
        }
        else
        {
            Debug.LogError("SistemaEndgame: Error al enviar datos al servidor.");
        }
    }
}

/// <summary>
/// Clase para serializar los datos que se enviarán al servidor
/// </summary>
[Serializable]
public class EndgameData
{
    public string nom_usuari;
    public int tiemps_jugat;
    public int enemics_derrotats;
}