using UnityEngine;
//Aquest script fa que una llum sembli el sol, movent-se lentament per fer un dia i una nit. 
// També canvia de color segons l'hora, i pots parar el cicle quan vulguis. Fácil!

// MANUAL DE CONFIGURACIÓ AL UNITY:
// 1. Directional Light (menu hierarchy)
// 2. Add component (menu inspector)
// 3. Arrosegar Directional light a llum solar

public class CicleDiaNit : MonoBehaviour
{
    [Header("Configuració")]
    [SerializeField] private float duracioDiaEnSegons = 300f;  // 300 segons (5 min) que tardarà el sol en fer 360º
    [SerializeField] private bool cicleActiu = true;
    [SerializeField] private bool efecteColorActiu = true;  // Opción para activar/desactivar el cambio de color

    [Header("Referències")]
    [SerializeField] private Light llumSolar;

    private float rotacioInicial; // Rotació inicial de la llum

    void Start()
    {
        // Guarda l'angle inicial de rotació de la llum solar
        rotacioInicial = llumSolar.transform.eulerAngles.x;
    }

    void Update()
    {
        if (cicleActiu)
        {
            // Calcula la rotació en funció del temps
            float rotacioActual = (Time.time / duracioDiaEnSegons) * 360f;
            llumSolar.transform.rotation = Quaternion.Euler(rotacioActual + rotacioInicial, -90f, 0);

            // Opcional: Actualitza el color de la llum segons l'hora del dia
            ActualitzarColorLlum(rotacioActual);
        }
    }

    private void ActualitzarColorLlum(float angle)
    {
        if (efecteColorActiu)
        {
            // Canvia a colors càlids per a l'alba i el capvespre
            if (angle < 180f)
            {
                llumSolar.color = Color.Lerp(Color.red, Color.white, angle / 180f);
            }
            else
            {
                llumSolar.color = Color.Lerp(Color.white, Color.blue, (angle - 180f) / 180f);
            }
        }
        else
        {
            // Si el efecto de color está desactivado, mantener un color blanco constante
            llumSolar.color = Color.white;
        }
    }

    // Mètode per pausar/reprendre el cicle (opcional)
    public void AlternarCicle(bool activar)
    {
        cicleActiu = activar;
    }
}