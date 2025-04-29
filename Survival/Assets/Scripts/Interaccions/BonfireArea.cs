using UnityEngine;

public class BonfireArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TemperatureSystemUI tempSystem = FindObjectOfType<TemperatureSystemUI>();
            if (tempSystem != null)
            {
                tempSystem.SetNearBonfire(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TemperatureSystemUI tempSystem = FindObjectOfType<TemperatureSystemUI>();
            if (tempSystem != null)
            {
                tempSystem.SetNearBonfire(false);
            }
        }
    }
}
