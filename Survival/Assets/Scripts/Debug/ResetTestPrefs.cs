using UnityEngine;

public class ResetTestPrefs : MonoBehaviour
{
    void Start()
    {
        // Esto sólo corre en la primera escena, si este objeto NO es DontDestroyOnLoad
        if (Application.isEditor)
        {
            PlayerPrefs.DeleteKey("AnimalSalvat");
            PlayerPrefs.Save();
            Debug.Log("⚠️ [ResetTestPrefs] ‘AnimalSalvat’ borrado para pruebas");
        }
    }
}
