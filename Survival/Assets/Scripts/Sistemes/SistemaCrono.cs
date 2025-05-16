using UnityEngine;
using System;



public class SistemaCrono : MonoBehaviour
{
    public static SistemaCrono Instance { get; private set; }
    
    float elapsedTime = 0.0f;

    string route="api/cronometro";
    
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            // Make this gameObject persist between scene loads
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    void Update() {
        elapsedTime += Time.deltaTime;
        int seconds = (int)elapsedTime;
    }

    /// <summary>
    /// Obtiene el tiempo transcurrido en segundos
    /// </summary>
    /// <returns>Tiempo transcurrido en segundos</returns>
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
