// filepath: c:\Users\aleja\Desktop\BINFO\VIDEOJOCS\lultim-tamarro\Survival\Assets\Scripts\Sistemes\SistemaEndgame.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Sistema que gestiona el final del juego y envía estadísticas al servidor 
/// cuando el jugador desbloquea todos los perks
/// </summary>
public class SistemaEndgame : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaEndgame Instance { get; private set; }
    
    [Header("UI Elements")]
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private TMP_InputField inputNomJugador;
    [SerializeField] private Button botonEnviar;
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private GameObject rankingItemPrefab;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoEnemigos;

    [Header("Configuración")]
    [SerializeField] private bool mostrarDebug = true;  // Mostrar mensajes de debug
    [SerializeField] private string apiUrl = "http://localhost:8080";

    // Endpoint para enviar los datos
    private string endpoint = "http://localhost:8080/puntuacions/";

    // Tracking de estado
    private bool todosPerksDesbloqueados = false;
    private bool datosEnviados = false;
    private bool rankingEnviado = false;
    
    // Evento para notificar cuando se completa el juego
    public event Action OnJuegoCompletado;
    
    [Header("Estadísticas en tiempo real")]
    [SerializeField] private float tempsTranscorregut;
    [SerializeField] private int enemicsDerrotats;

    private void Awake()
    {
        // Configurar el singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (mostrarDebug)
            Debug.Log("SistemaEndgame: Sistema inicializado.");
    }
    private void Start()
    {
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (botonEnviar != null)
        {
            botonEnviar.onClick.AddListener(EnviarPuntuacion);
        }
        // Comprobar perks solo una vez al iniciar
        ComprobarPerks();
        
        if (mostrarDebug)
            Debug.Log("SistemaEndgame: Sistema iniciado. Comprobando perks al inicio.");
    }

    private void Update()
    {
        if (SistemaCrono.Instance != null)
            tempsTranscorregut = SistemaCrono.Instance.GetElapsedTime();

        if (SistemaCounter.Instance != null)
            enemicsDerrotats = SistemaCounter.Instance.ObtenerTotalEnemigos();
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
                Debug.Log("SistemaEndgame: ¡Todos los perks están desbloqueados! Esperando regreso al HUB para mostrar ranking...");
            
            // NO ENVIAR DATOS AQUÍ
            // Solo mostrar el ranking cuando el jugador regrese al HUB
            // El método OnVolverAlHubTrasTodasPerks() debe ser llamado desde el TP del HUB
            // y ahí se mostrará el menú de ranking

            // Notificar que el juego se ha completado (opcional)
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
            temps_jugat = (int)tiempoJuego,
            enemics_derrotats = enemigosEliminados,
        };
          // Convertir a JSON
        string jsonData = JsonUtility.ToJson(datos);
        
        // Log detallado del objeto y la URL antes de enviar
        Debug.Log($"[SistemaEndgame] Enviando datos a {endpoint}:");
        Debug.Log($"[SistemaEndgame] JSON: {jsonData}");
        Debug.Log($"[SistemaEndgame] nom_usuari: {datos.nom_usuari}, temps_jugat: {datos.temps_jugat}, enemics_derrotats: {datos.enemics_derrotats}");
        
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

    // Llama a este método cuando el jugador complete el juego
    public void MostrarPanelRanking()
    {
        Debug.Log("[SistemaEndgame][LOG] -> MostrarPanelRanking llamado. rankingPanel=" + (rankingPanel != null) + ", rankingEnviado=" + rankingEnviado);
        if (rankingPanel != null && !rankingEnviado)
        {
            // PAUSAR EL CRONÓMETRO
            if (SistemaCrono.Instance != null)
                SistemaCrono.Instance.PausarCronometro();

            rankingPanel.SetActive(true);
            Debug.Log("[SistemaEndgame][LOG] -> rankingPanel.SetActive(true) ejecutado");
            ActualizarEstadisticas();
            Debug.Log("[SistemaEndgame][LOG] -> ActualizarEstadisticas llamado");
            Debug.Log("[SistemaEndgame] Panel de ranking ACTIVADO");
        }
        else
        {
            Debug.Log("[SistemaEndgame][LOG] -> No se puede activar el panel de ranking (ya enviado o panel nulo)");
        }
    }

    private void ActualizarEstadisticas()
    {
        Debug.Log("[SistemaEndgame][LOG] -> ActualizarEstadisticas ejecutado");
        float tiempo = 0f;
        if (SistemaCrono.Instance != null)
            tiempo = SistemaCrono.Instance.GetElapsedTime();

        TimeSpan tiempoSpan = TimeSpan.FromSeconds(tiempo);
        string tiempoFormateado = string.Format("{0:D2}:{1:D2}:{2:D2}", tiempoSpan.Hours, tiempoSpan.Minutes, tiempoSpan.Seconds);

        int enemigos = 0;
        if (SistemaCounter.Instance != null)
            enemigos = SistemaCounter.Instance.ObtenerTotalEnemigos();

        textoTiempo.text = $"Temps: {tiempoFormateado}";
        textoEnemigos.text = $"Enemics: {enemigos}";
    }

    private void EnviarPuntuacion()
    {
        if (string.IsNullOrEmpty(inputNomJugador.text))
        {
            Debug.LogWarning("Por favor, introduce un nombre de jugador");
            return;
        }

        var puntuacion = new Puntuacio
        {
            nom_usuari = inputNomJugador.text,
            temps_jugat = (int)tempsTranscorregut,
            enemics_derrotats = enemicsDerrotats
        };

        StartCoroutine(EnviarPuntuacionAlServidor(puntuacion));
    }

    private IEnumerator EnviarPuntuacionAlServidor(Puntuacio puntuacion)
    {
        string json = JsonUtility.ToJson(puntuacion);
        Debug.Log("JSON enviado: " + json);
        using (UnityEngine.Networking.UnityWebRequest www = new UnityEngine.Networking.UnityWebRequest($"{apiUrl}/puntuacions/", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error al enviar puntuación: {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                // Tras enviar, obtener el ranking y mostrarlo por consola
                using (UnityEngine.Networking.UnityWebRequest getRanking = UnityEngine.Networking.UnityWebRequest.Get($"{apiUrl}/puntuacions/temps"))
                {
                    yield return getRanking.SendWebRequest();
                    if (getRanking.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        string rankingJson = getRanking.downloadHandler.text;
                        Debug.Log("[DEBUG] JSON ranking recibido tras enviar: " + rankingJson);
                        var puntuaciones = JsonHelper.FromJson<Puntuacio>(rankingJson);
                        Debug.Log("[DEBUG] --- TOP RANKING ---");
                        for (int i = 0; i < puntuaciones.Length; i++)
                        {
                            Debug.Log($"[DEBUG] Puesto {i+1}: Nombre={puntuaciones[i].nom_usuari}, Tiempo={puntuaciones[i].temps_jugat}, Enemics={puntuaciones[i].enemics_derrotats}");
                        }
                    }
                    else
                    {
                        Debug.LogError("[DEBUG] Error al obtener ranking tras enviar: " + getRanking.error);
                    }
                }
                // Lógica original
                StartCoroutine(ObtenerYMostrarRanking());
                rankingEnviado = true;
            }
        }
    }

    private IEnumerator ObtenerYMostrarRanking()
    {
        Debug.Log("Obteniendo ranking de la API...");
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get($"{apiUrl}/puntuacions/temps"))
        {
            yield return www.SendWebRequest();

            Debug.Log("Resultado petición ranking: " + www.result + " | Código: " + www.responseCode);
            Debug.Log("Texto recibido: " + www.downloadHandler.text);

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                Debug.Log("JSON recibido: " + json);
                // Usar JsonHelper para parsear el array
                var puntuaciones = JsonHelper.FromJson<Puntuacio>(json);
                MostrarRanking(new List<Puntuacio>(puntuaciones));
            }
            else
            {
                Debug.LogError($"Error al obtener ranking: {www.error}");
            }
        }
    }

    private void MostrarRanking(List<Puntuacio> puntuaciones)
    {
        Debug.Log("Mostrando ranking. Elementos recibidos: " + puntuaciones.Count);

        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }

        int max = Mathf.Min(10, puntuaciones.Count);
        for (int i = 0; i < max; i++)
        {
            var puntuacion = puntuaciones[i];
            Debug.Log($"Instanciando ranking: {puntuacion.nom_usuari}, tiempo: {puntuacion.temps_jugat}, enemigos: {puntuacion.enemics_derrotats}");
            GameObject item = Instantiate(rankingItemPrefab, rankingContainer);
            var textos = item.GetComponentsInChildren<TMPro.TMP_Text>(true);

            TMPro.TMP_Text textNom = null;
            TMPro.TMP_Text textTemps = null;
            TMPro.TMP_Text textEnemics = null;
            foreach (var t in textos)
            {
                if (t.name == "TextNom") textNom = t;
                else if (t.name == "TextTemps") textTemps = t;
                else if (t.name == "TextEnemics") textEnemics = t;
            }

            if (textNom != null && textTemps != null && textEnemics != null)
            {
                textNom.text = puntuacion.nom_usuari;
                textTemps.text = FormatearTiempo(puntuacion.temps_jugat);
                textEnemics.text = puntuacion.enemics_derrotats.ToString();
            }
            else
            {
                Debug.LogWarning("El prefab ItemRanking no tiene los textos esperados por nombre (TextNom, TextTemps, TextEnemics).");
            }
        }
    }

    private string FormatearTiempo(int segundos)
    {
        TimeSpan tiempo = TimeSpan.FromSeconds(segundos);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", 
            tiempo.Hours, 
            tiempo.Minutes, 
            tiempo.Seconds);
    }

    public void OnVolverAlHubTrasTodasPerks()
    {
        StartCoroutine(MostrarRankingConDelay());
    }

    private IEnumerator MostrarRankingConDelay()
    {
        Debug.Log("[SistemaEndgame][LOG] -> MostrarRankingConDelay iniciado. Esperando 1 segundo...");
        yield return new WaitForSeconds(1f);
        Debug.Log("[SistemaEndgame][LOG] -> Llamando a MostrarPanelRanking...");
        MostrarPanelRanking();
    }

    // Añade este método para que lo llame el TP del HUB cuando el jugador entra
    public void OnPlayerEnterHub()
    {
        Debug.Log("[SistemaEndgame][LOG] -> OnPlayerEnterHub llamado. todosPerksDesbloqueados=" + todosPerksDesbloqueados + ", rankingEnviado=" + rankingEnviado);
        
        // Comprobar si todos los perks están desbloqueados
        bool todosDesbloqueados = ComprobarTodosPerksDesbloqueados();
        Debug.Log("[SistemaEndgame][LOG] -> ComprobarTodosPerksDesbloqueados() = " + todosDesbloqueados);
        
        // Si todos los perks están desbloqueados y aún no se ha mostrado el ranking
        if (todosDesbloqueados && !rankingEnviado)
        {
            todosPerksDesbloqueados = true; // Asegurarnos de que esta variable esté actualizada
            Debug.Log("[SistemaEndgame][LOG] -> Condición para mostrar ranking CUMPLIDA. Llamando a MostrarRankingConDelay...");
            StartCoroutine(MostrarRankingConDelay());
        }
        else
        {
            Debug.Log("[SistemaEndgame][LOG] -> No se cumplen las condiciones para mostrar el ranking. todosDesbloqueados=" + todosDesbloqueados + ", rankingEnviado=" + rankingEnviado);
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
    public int temps_jugat;
    public int enemics_derrotats;
}

[System.Serializable]
public class Puntuacio
{
    public string nom_usuari;
    public int temps_jugat;
    public int enemics_derrotats;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}