using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cortinilla : MonoBehaviour
{
    [SerializeField] private Image imatgeCortinilla;
    [SerializeField] private float duradaEfecte = 2.0f;
    [SerializeField] private AnimationCurve corbaTransicio;
    [SerializeField] private Material materialCercle;
    [SerializeField] private bool inverseEffect = true; // Canviat a true perquè tanqui primer

    private static readonly int RadioProperty = Shader.PropertyToID("_Radius");
    
    private void Awake() // Canviat a Awake per garantir que s'inicialitza abans
    {
        Debug.Log("Cortinilla inicialitzada");
        
        if (imatgeCortinilla == null)
        {
            Debug.LogError("Error: imatgeCortinilla no assignada!");
            return;
        }
        
        if (materialCercle == null)
        {
            Debug.LogError("Error: materialCercle no assignat!");
            return;
        }
        
        // Assegurem que la imatge està utilitzant el material correcte
        imatgeCortinilla.material = new Material(materialCercle); // Crear instància del material
        
        // Configurem el radi inicial
        if (inverseEffect)
            imatgeCortinilla.material.SetFloat(RadioProperty, 1f); // Cercle completament obert
        else
            imatgeCortinilla.material.SetFloat(RadioProperty, 0f); // Cercle completament tancat
            
        // Ocultem la imatge inicialment
        imatgeCortinilla.gameObject.SetActive(false);
    }

    public void MostrarCortinilla()
    {
        Debug.Log("MostrarCortinilla cridat");
        
        if (imatgeCortinilla == null)
        {
            Debug.LogError("Error: imatgeCortinilla és null!");
            return;
        }
        
        imatgeCortinilla.gameObject.SetActive(true);
        StartCoroutine(AnimarCortinilla());
    }
    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.T)) // Tecla T per provar
        { 
            Debug.Log("Iniciant animació de cortinilla");
            MostrarCortinilla();
        }
    }
    private IEnumerator AnimarCortinilla()
    {
        Debug.Log("Iniciant animació de cortinilla");
        float tempsInici = Time.time;
        float percentatgeCompletat = 0f;
        
        // Guardem el valor inicial i final
        float radiInicial = inverseEffect ? 1f : 0f;
        float radiFinal = inverseEffect ? 0f : 1f;
        
        // Ens assegurem que comencem amb el valor correcte
        imatgeCortinilla.material.SetFloat(RadioProperty, radiInicial);
        
        // Animem el radi del cercle
        while (percentatgeCompletat < 1.0f)
        {
            percentatgeCompletat = (Time.time - tempsInici) / duradaEfecte;
            percentatgeCompletat = Mathf.Clamp01(percentatgeCompletat); // Limitem entre 0 i 1
            
            // Interpolem linealment entre el radi inicial i final
            float valorRadi = Mathf.Lerp(radiInicial, radiFinal, percentatgeCompletat);
            
            // Apliquem el valor al shader
            imatgeCortinilla.material.SetFloat(RadioProperty, valorRadi);
            Debug.Log($"Radi: {valorRadi} (Percentatge: {percentatgeCompletat:0.00})");
            
            yield return null;
        }
        
        // Assegurem que acabi amb el valor exacte final
        imatgeCortinilla.material.SetFloat(RadioProperty, radiFinal);
        Debug.Log("Animació de cortinilla completada");
    }
}