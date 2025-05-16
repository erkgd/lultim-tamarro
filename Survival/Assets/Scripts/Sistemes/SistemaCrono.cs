using UnityEngine;
using System;


[ExecuteInEditMode]
public class SistemaCrono : MonoBehaviour
{
    float elapsedTime = 0.0f;

    string route="api/cronometro";
    
    void Awake()
    {
        // Make this gameObject persist between scene loads
        DontDestroyOnLoad(this.gameObject);
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
