using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject temperatureUIPrefab;
    private static bool created;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!created)
        {
            if (temperatureUIPrefab == null)
            {
                Debug.LogError("[GameManager] ¡No has asignado el prefab de temperatura!");
            }
            else
            {
                Debug.Log("[GameManager] Instanciando TemperatureContainer prefabricado");
                Instantiate(temperatureUIPrefab);
                created = true;
            }
        }
    }
}
