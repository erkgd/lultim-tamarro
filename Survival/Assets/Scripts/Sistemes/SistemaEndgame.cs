// filepath: c:\Users\aleja\Desktop\BINFO\VIDEOJOCS\lultim-tamarro\Survival\Assets\Scripts\Sistemes\SistemaEndgame.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Networking;

/// <summary>
/// Sistema que gestiona el final del juego y envía estadísticas al servidor 
/// cuando el jugador desbloquea todos los perks
/// </summary>
public class SistemaEndgame : MonoBehaviour
{
    // Singleton para acceso global
    public static SistemaEndgame Instance { get; private set; }
    
    [Header("Elements UI")]
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private TMP_InputField inputNomJugador;
    [SerializeField] private Button botonEnviar;
    [SerializeField] private Button botonCerrar;
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private GameObject rankingItemPrefab;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoEnemigos;
    [SerializeField] private Transform contenedorRankingTemps;
    [SerializeField] private Transform contenedorRankingEnemics;

    [Header("Configuració")]
    [SerializeField] private bool mostrarDebug = true;  // Mostrar missatges de debug
    [SerializeField] private string apiUrl = "http://localhost:8080";

    // Endpoint per enviar les dades
    private string endpoint = "http://localhost:8080/puntuacions/";

    // Tracking de l'estat
    private bool todosPerksDesbloqueados = false;
    private bool datosEnviados = false;
    private bool rankingEnviado = false;
    private bool puntuacionEnviada = false;
    
    // Evento para notificar cuando se completa el juego
    public event Action OnJuegoCompletado;
    
    [Header("Estadístiques en temps real")]
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
            Debug.Log("SistemaEndgame: Sistema iniciat.");
    }
    private void Start()
    {
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (botonEnviar != null)
        {
            botonEnviar.onClick.AddListener(EnviarPuntuacion);
        }
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarPanelRanking);
        }
        
        if (mostrarDebug)
            Debug.Log("SistemaEndgame: Sistema iniciat.");
    }

    private void Update()
    {
        if (SistemaCrono.Instance != null)
            tempsTranscorregut = SistemaCrono.Instance.GetElapsedTime();

        if (SistemaCounter.Instance != null)
            enemicsDerrotats = SistemaCounter.Instance.ObtenerTotalEnemigos();
    }

    /// <summary>
    /// Comprueba si todos los perks están desbloqueados
    /// </summary>
    private bool ComprobarTodosPerksDesbloqueados()
    {
        if (SistemaPerks.Instance == null)
        {
            Debug.LogWarning("SistemaEndgame: SistemaPerks no trobat.");
            return false;
        }

        // El SistemaPerks tiene 4 perks definidos (0: Velocitat, 1: Resistència, 2: Atac, 3: Vida)
        for (int i = 0; i < 4; i++)
        {
            if (!SistemaPerks.Instance.EstaDesbloquejada(i))
            {
                if (mostrarDebug)
                    Debug.Log($"SistemaEndgame: Perk {SistemaPerks.Instance.NomPerk(i)} encara no desbloquejat.");
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Envia les dades de temps i enemics al servidor
    /// </summary>
    private void EnviarDatosAlServidor()
    {
        // Obtén la referencia a SistemaCrono
        SistemaCrono cronometro = FindObjectOfType<SistemaCrono>();
        if (cronometro == null)
        {
            Debug.LogError("SistemaEndgame: No s'ha trobat SistemaCrono.");
            return;
        }
        
        // Obtén el temps transcorregut
        float tiempoJuego = cronometro.GetElapsedTime();
        
        // Obtén el conteo d'enemics
        int enemigosEliminados = 0;
        if (SistemaCounter.Instance != null)
        {
            enemigosEliminados = SistemaCounter.Instance.ObtenerTotalEnemigos();
        }        else
        {
            Debug.LogWarning("SistemaEndgame: SistemaCounter no trobat.");
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
        Debug.Log($"[SistemaEndgame] Enviant dades a {endpoint}:");
        Debug.Log($"[SistemaEndgame] JSON: {jsonData}");
        Debug.Log($"[SistemaEndgame] nom_usuari: {datos.nom_usuari}, temps_jugat: {datos.temps_jugat}, enemics_derrotats: {datos.enemics_derrotats}");
        
        if (mostrarDebug)
            Debug.Log($"SistemaEndgame: Enviant dades al servidor: {jsonData}");
        
        // Enviar dades usant HttpSystem
        if (HttpSystem.Instance != null)
        {
            HttpSystem.Instance.PostRequest(endpoint, jsonData, OnDatosEnviados);
        }
        else
        {
            Debug.LogError("SistemaEndgame: HttpSystem no trobat.");
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
            Debug.Log($"SistemaEndgame: Dades enviades correctament. Resposta: {respuesta}");
        }
        else
        {
            Debug.LogError("SistemaEndgame: Error en enviar dades al servidor.");
        }
    }

    // Quan el jugador completi el joc
    public void MostrarPanelRanking()
    {
        Debug.Log("[SistemaEndgame][LOG] -> MostrarPanelRanking llamado. rankingPanel=" + (rankingPanel != null) + ", rankingEnviado=" + rankingEnviado);
        if (rankingPanel != null && !rankingEnviado)
        {
            // PAUSAR EL CRONÒMETRE
            if (SistemaCrono.Instance != null)
                SistemaCrono.Instance.PausarCronometro();

            rankingPanel.SetActive(true);
            Debug.Log("[SistemaEndgame][LOG] -> rankingPanel.SetActive(true) executat");
            ActualizarEstadisticas();
            Debug.Log("[SistemaEndgame][LOG] -> ActualizarEstadisticas anomenats");
            Debug.Log("[SistemaEndgame] Panel de ranking ACTIVAT");
        }
        else
        {
            Debug.Log("[SistemaEndgame][LOG] -> No es pot activar el panel de ranking (ja enviat o panel nul)");
        }
    }

    private void ActualizarEstadisticas()
    {
        Debug.Log("[SistemaEndgame][LOG] -> ActualizarEstadisticas executat");
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
        if (puntuacionEnviada)
        {
            Debug.LogWarning("La puntuació ja ha estat enviada. No es pot enviar més d'una vegada.");
            return;
        }
        if (string.IsNullOrEmpty(inputNomJugador.text))
        {
            Debug.LogWarning("Si us plau, introdueix un nom de jugador");
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
        Debug.Log("JSON enviat: " + json);
        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/puntuacions/", "POST"))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error en enviar puntuació: {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                puntuacionEnviada = true;
                // Després d'enviar, obtén el ranking i mostra-lo per la consola
                using (UnityWebRequest getRanking = UnityWebRequest.Get($"{apiUrl}/puntuacions/temps"))
                {
                    yield return getRanking.SendWebRequest();
                    if (getRanking.result == UnityWebRequest.Result.Success)
                    {
                        string rankingJson = getRanking.downloadHandler.text;
                        Debug.Log("[DEBUG] JSON ranking rebut després d'enviar: " + rankingJson);
                        var puntuaciones = JsonHelper.FromJson<Puntuacio>(rankingJson);
                        Debug.Log("[DEBUG] --- TOP RANKING ---");
                        for (int i = 0; i < puntuaciones.Length; i++)
                        {
                            Debug.Log($"[DEBUG] Puesto {i+1}: Nom={puntuaciones[i].nom_usuari}, Temps={puntuaciones[i].temps_jugat}, Enemics={puntuaciones[i].enemics_derrotats}");
                        }
                    }
                    else
                    {
                        Debug.LogError("[DEBUG] Error en obtenir el ranking després d'enviar: " + getRanking.error);
                    }
                }
                // Lógica original
                StartCoroutine(ObtenerYMostrarRankingTemps());
                StartCoroutine(ObtenerYMostrarRankingEnemics());
                rankingEnviado = true;
            }
        }
    }

    private IEnumerator ObtenerYMostrarRankingTemps()
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/puntuacions/temps"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var puntuaciones = JsonHelper.FromJson<Puntuacio>(www.downloadHandler.text);
                MostrarRanking(contenedorRankingTemps, puntuaciones);
            }
            else
            {
                Debug.LogError("Error obtenint ranking temps: " + www.error);
            }
        }
    }

    private IEnumerator ObtenerYMostrarRankingEnemics()
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}/puntuacions/enemics"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var puntuaciones = JsonHelper.FromJson<Puntuacio>(www.downloadHandler.text);
                MostrarRanking(contenedorRankingEnemics, puntuaciones);
            }
            else
            {
                Debug.LogError("Error obtenint ranking enemics: " + www.error);
            }
        }
    }

    private void MostrarRanking(Transform contenedor, Puntuacio[] puntuaciones)
    {
        Debug.Log("Mostrant ranking. Elements rebuts: " + puntuaciones.Length);

        foreach (Transform child in contenedor)
        {
            if (child.name != "TituloRankingTemps" && child.name != "TituloRankingEnemics")
                Destroy(child.gameObject);
        }

        int max = Mathf.Min(10, puntuaciones.Length);
        for (int i = 0; i < max; i++)
        {
            var puntuacion = puntuaciones[i];
            Debug.Log($"Instanciando ranking: {puntuacion.nom_usuari}, tiempo: {puntuacion.temps_jugat}, enemigos: {puntuacion.enemics_derrotats}");
            GameObject item = Instantiate(rankingItemPrefab, contenedor);
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
                Debug.LogWarning("El prefab ItemRanking no té els textos esperats per nom (TextNom, TextTemps, TextEnemics).");
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

    public void OnPlayerEnterHub()
    {
        Debug.Log("[SistemaEndgame][LOG] -> OnPlayerEnterHub llamado. todosPerksDesbloqueados=" + todosPerksDesbloqueados + ", rankingEnviado=" + rankingEnviado);
        
        // Comprobar si tots els perks estan desbloqueados
        bool todosDesbloqueados = ComprobarTodosPerksDesbloqueados();
        Debug.Log("[SistemaEndgame][LOG] -> ComprobarTodosPerksDesbloqueados() = " + todosDesbloqueados);
        
        // Si tots els perks estan desbloqueados i encara no s'ha mostrat el ranking
        if (todosDesbloqueados && !rankingEnviado)
        {
            todosPerksDesbloqueados = true;
            Debug.Log("[SistemaEndgame][LOG] -> Condició per mostrar ranking CUMPLIDA. Llamant a MostrarRankingConDelay...");
            StartCoroutine(MostrarRankingConDelay());
        }
        else
        {
            Debug.Log("[SistemaEndgame][LOG] -> No es compleixen les condicions per mostrar el ranking. todosDesbloqueados=" + todosDesbloqueados + ", rankingEnviado=" + rankingEnviado);
        }
    }

    private IEnumerator MostrarRankingConDelay()
    {
        Debug.Log("[SistemaEndgame][LOG] -> MostrarRankingConDelay iniciat. Esperant 1 segon...");
        yield return new WaitForSeconds(1f);
        Debug.Log("[SistemaEndgame][LOG] -> Llamant a MostrarPanelRanking...");
        MostrarPanelRanking();
    }

    private void CerrarPanelRanking()
    {
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
            Debug.Log("[SistemaEndgame] Panel de ranking TANCAT");
        }
    }
}

/// <summary>
/// Classe per serialitzar les dades que es enviaran al servidor
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