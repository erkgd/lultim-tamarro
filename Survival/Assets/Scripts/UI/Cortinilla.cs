using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cortinilla : MonoBehaviour
{
    [Header("Referències")]
    [SerializeField] private Image imatgeCortinilla;
    [SerializeField] private Material materialCortinilla;

    [Header("Configuració")]
    [SerializeField] private float duradaEfecte = 1.5f;
    [SerializeField] private AnimationCurve corbaTransicio;
    [SerializeField] private bool inverseEffect = true; // Si és true, l'efecte va des de fora cap a dins (tancament)

    // Propietat del shader
    private static readonly int RadioProperty = Shader.PropertyToID("_Radius");
    
    // Control para activar solo una vez
    private bool yaSeHaMostrado = false;

    // Control para la cortinilla inversa
    private bool cortinillaInversaActiva = false;

    private void Awake()
    {
        // Comprobem que tenim la imatge
        if (imatgeCortinilla == null)
        {
            imatgeCortinilla = GetComponent<Image>();
        }
        
        // Comprovem que tenim el material
        if (imatgeCortinilla != null && materialCortinilla != null)
        {
            // Creem una instància del material per no modificar l'original
            imatgeCortinilla.material = new Material(materialCortinilla);
        }
        
        // Ocultem la cortinilla inicialment
        if (imatgeCortinilla != null)
        {
            imatgeCortinilla.gameObject.SetActive(false);
        }
    }

    // Mètode públic per mostrar la cortinilla
    public void MostrarCortinilla()
    {
        // Si ya se ha mostrado una vez, no lo volvemos a hacer
        if (yaSeHaMostrado)
        {
            Debug.Log("Cortinilla: Ya se ha mostrado anteriormente, no se volverá a mostrar");
            return;
        }
        
        // Activem el GameObject
        if (imatgeCortinilla != null)
        {
            imatgeCortinilla.gameObject.SetActive(true);
            StartCoroutine(AnimarCortinilla());
            
            // Marcamos que ya se ha mostrado
            yaSeHaMostrado = true;
        }
        else
        {
            Debug.LogError("Cortinilla: No s'ha trobat la imatge de la cortinilla");
        }
    }
    
    // Método para resetear la cortinilla (usar solo en casos específicos)
    public void ResetearCortinilla()
    {
        yaSeHaMostrado = false;
        cortinillaInversaActiva = false;
    }

    // Método público para mostrar la cortinilla en sentido inverso
    public void MostrarCortinillaInversa()
    {
        // Si ya está activa la cortinilla inversa, no lo volvemos a hacer
        if (cortinillaInversaActiva)
        {
            Debug.Log("Cortinilla: La cortinilla inversa ya está activa");
            return;
        }
        
        // Activamos el GameObject
        if (imatgeCortinilla != null)
        {
            imatgeCortinilla.gameObject.SetActive(true);
            StartCoroutine(AnimarCortinillaInversa());
            
            // Marcamos que ya está activa la cortinilla inversa
            cortinillaInversaActiva = true;
        }
        else
        {
            Debug.LogError("Cortinilla: No s'ha trobat la imatge de la cortinilla");
        }
    }

    // Corrutina para la animación inversa
    private IEnumerator AnimarCortinillaInversa()
    {
        float tempsInici = Time.time;
        float percentatgeCompletat = 0f;
        
        // Valor inicial y final del radio (inverso al normal)
        float radiInicial = inverseEffect ? 0f : 1f; // Inverso al efecto normal
        float radiFinal = inverseEffect ? 1f : 0f;   // Inverso al efecto normal
        
        // Establecemos el valor inicial
        imatgeCortinilla.material.SetFloat(RadioProperty, radiInicial);
        
        // Animamos el radio
        while (percentatgeCompletat < 1.0f)
        {
            percentatgeCompletat = (Time.time - tempsInici) / duradaEfecte;
            percentatgeCompletat = Mathf.Clamp01(percentatgeCompletat);
            
            // Utilizamos la curva de transición para una animación más suave
            float valorAnimacio = corbaTransicio.Evaluate(percentatgeCompletat);
            float valorRadi = Mathf.Lerp(radiInicial, radiFinal, valorAnimacio);
            
            imatgeCortinilla.material.SetFloat(RadioProperty, valorRadi);
            
            yield return null;
        }
        
        // Aseguramos que termina con el valor exacto
        imatgeCortinilla.material.SetFloat(RadioProperty, radiFinal);
        
        // Opcional: desactivar la imagen al terminar la animación, dependiendo de la necesidad
        // imatgeCortinilla.gameObject.SetActive(false);
    }

    // Corrutina per a l'animació
    private IEnumerator AnimarCortinilla()
    {
        float tempsInici = Time.time;
        float percentatgeCompletat = 0f;
        
        // Valor inicial i final del radi
        float radiInicial = inverseEffect ? 1f : 0f;
        float radiFinal = inverseEffect ? 0f : 1f;
        
        // Establim el valor inicial
        imatgeCortinilla.material.SetFloat(RadioProperty, radiInicial);
        
        // Animem el radi
        while (percentatgeCompletat < 1.0f)
        {
            percentatgeCompletat = (Time.time - tempsInici) / duradaEfecte;
            percentatgeCompletat = Mathf.Clamp01(percentatgeCompletat);
            
            // Utilitzem la corba de transició per a una animació més suau
            float valorAnimacio = corbaTransicio.Evaluate(percentatgeCompletat);
            float valorRadi = Mathf.Lerp(radiInicial, radiFinal, valorAnimacio);
            
            imatgeCortinilla.material.SetFloat(RadioProperty, valorRadi);
            
            yield return null;
        }
        
        // Assegurem que acaba amb el valor exacte
        imatgeCortinilla.material.SetFloat(RadioProperty, radiFinal);
    }
}