// filepath: c:\Users\aleja\Desktop\BINFO\VIDEOJOCS\lultim-tamarro\Survival\Assets\Scripts\Sistemes\HttpSystem.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;

/// <summary>
/// Sistema para manejar peticiones HTTP a APIs REST
/// </summary>
public class HttpSystem : MonoBehaviour
{
    private static HttpSystem instance;

    /// <summary>
    /// Obtiene la instancia singleton del sistema HTTP
    /// </summary>
    public static HttpSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject httpSystemObj = new GameObject("HttpSystem");
                instance = httpSystemObj.AddComponent<HttpSystem>();
                DontDestroyOnLoad(httpSystemObj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Envía una petición GET a la URL especificada
    /// </summary>
    /// <param name="url">URL de la API</param>
    /// <param name="callback">Función que se ejecutará al recibir la respuesta</param>
    public void GetRequest(string url, Action<string> callback)
    {
        StartCoroutine(GetRequestCoroutine(url, callback));
    }

    private IEnumerator GetRequestCoroutine(string url, Action<string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error en la petición HTTP: {webRequest.error}");
                callback(null);
            }
        }
    }

    /// <summary>
    /// Envía una petición POST a la URL especificada con los datos proporcionados
    /// </summary>
    /// <param name="url">URL de la API</param>
    /// <param name="data">Datos a enviar en formato JSON</param>
    /// <param name="callback">Función que se ejecutará al recibir la respuesta</param>
    public void PostRequest(string url, string data, Action<string> callback)
    {
        StartCoroutine(PostRequestCoroutine(url, data, callback));
    }    private IEnumerator PostRequestCoroutine(string url, string data, Action<string> callback)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error en la petición HTTP: {webRequest.error} - URL: {url}");
                callback(null);
            }
        }
    }

    /// <summary>
    /// Envía una petición a la API REST con los parámetros especificados
    /// </summary>
    /// <param name="url">URL de la API</param>
    /// <param name="data">Datos a enviar en formato JSON</param>
    /// <param name="method">Método HTTP (GET, POST, PUT, DELETE)</param>
    /// <param name="callback">Función que se ejecutará al recibir la respuesta</param>
    public void SendRequest(string url, string data, string method, Action<string> callback)
    {
        StartCoroutine(SendRequestCoroutine(url, data, method, callback));
    }

    private IEnumerator SendRequestCoroutine(string url, string data, string method, Action<string> callback)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.SetRequestHeader("Content-Type", "application/json");
            }
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error en la petición HTTP: {webRequest.error} - URL: {url}");
                callback(null);
            }
        }
    }
}