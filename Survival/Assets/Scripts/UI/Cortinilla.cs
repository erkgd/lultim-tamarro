using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Cortinilla : MonoBehaviour
{
    // Singleton para acceso global
    public static Cortinilla Instance { get; private set; }
    
    [Header("Referències")]
    [SerializeField] private Image imatgeCortinilla;
    [SerializeField] private Material materialCortinilla;

    [Header("Configuració")]
    [SerializeField] private float duradaEfecte = 1.5f;
    [SerializeField] private AnimationCurve corbaTransicio;
    [SerializeField] private bool inverseEffect = true; // Si és true, l'efecte va des de fora cap a dins (tancament)

    // Propiedad pública para acceder a duradaEfecte desde otras clases
    public float DuradaEfecte => duradaEfecte;

    // Propietat del shader
    private static readonly int RadioProperty = Shader.PropertyToID("_Radius");
    
    // Control para activar solo una vez
    private bool yaSeHaMostrado = false;
    
    // Control para la auto-destrucción después de una transición
    private bool transicionEnProgreso = false;

    private void Awake()
    {
        // Implementación de singleton persistente
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Registramos un evento para detectar cambios de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            
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
        else
        {
            // Ya existe una instancia, destruimos esta para evitar duplicados
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Importante: eliminar el listener cuando se destruye el objeto
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si no hay una transición en progreso, este objeto ya cumplió su propósito
        if (!transicionEnProgreso && Instance == this)
        {
            // Verificar si hay un UIManager en la nueva escena que necesite esta referencia
            UIManager nuevoUIManager = FindObjectOfType<UIManager>();
            if (nuevoUIManager != null && nuevoUIManager != UIManager.Instance)
            {
                // Si hay un nuevo UIManager, nos auto-asignamos
                nuevoUIManager.AsignarCortinilla(this);
                Debug.Log("Cortinilla: Asignada a un nuevo UIManager en la escena " + scene.name);
            }
            else if (!imatgeCortinilla.gameObject.activeInHierarchy)
            {
                // Si no hay transición en progreso y estamos inactivos, podemos destruirnos
                // Solo si no somos la única cortinilla en la escena
                Cortinilla[] cortinillas = FindObjectsOfType<Cortinilla>();
                if (cortinillas.Length > 1)
                {
                    Instance = null;
                    Destroy(gameObject);
                    Debug.Log("Cortinilla: Instancia destruida por no ser necesaria");
                }
            }
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
    }

    // Método para deshacer la cortinilla si está activa
    public void DesferCortinilla()
    {
        if (imatgeCortinilla != null && imatgeCortinilla.gameObject.activeInHierarchy)
        {
            // Iniciar la animación en sentido inverso
            bool originalInverseEffect = inverseEffect;
            inverseEffect = !originalInverseEffect; // Invertimos el efecto
            
            StartCoroutine(AnimarCortinillaInversa(originalInverseEffect));
            
            Debug.Log("Cortinilla: Deshaciendo el efecto");
        }
        else
        {
            Debug.Log("Cortinilla: No está activa, no hay nada que deshacer");
        }
    }

    // Corrutina especial para la animación inversa
    private IEnumerator AnimarCortinillaInversa(bool originalInverseEffect)
    {
        float tempsInici = Time.time;
        float percentatgeCompletat = 0f;
        
        // Valor inicial i final del radi (invertidos respecto a la animación normal)
        float radiInicial = inverseEffect ? 1f : 0f;
        float radiFinal = inverseEffect ? 0f : 1f;
        
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
        
        // Aseguramos que termine con el valor exacto
        imatgeCortinilla.material.SetFloat(RadioProperty, radiFinal);
        
        // Restauramos el valor original del efecto inverso
        inverseEffect = originalInverseEffect;
        
        // Ocultamos la cortinilla al finalizar la animación inversa
        imatgeCortinilla.gameObject.SetActive(false);
        
        // Como se ha deshecho, podemos permitir que se muestre nuevamente
        yaSeHaMostrado = false;
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
    
    // Método para hacer una transición completa entre escenas
    public void HacerTransicionCompleta(string nombreEscena, Vector3 posicionDestino = default)
    {
        if (imatgeCortinilla != null)
        {
            transicionEnProgreso = true;
            ResetearCortinilla();
            MostrarCortinilla();
            StartCoroutine(CargarEscenaDespuesDeTransicion(nombreEscena, posicionDestino));
        }
        else
        {
            Debug.LogError("Cortinilla: No se puede realizar la transición porque falta la referencia a la imagen");
        }
    }
    
    // Corrutina para cargar una escena después de la transición
    private IEnumerator CargarEscenaDespuesDeTransicion(string nombreEscena, Vector3 posicionDestino)
    {
        // Esperar a que termine la animación de la cortinilla
        yield return new WaitForSeconds(duradaEfecte);
        
        // Guardar posición si es necesario usando SistemaPerks
        if (posicionDestino != default)
        {
            if (SistemaPerks.Instance != null)
            {
                // Usar SistemaPerks para guardar la posición
                SistemaPerks.Instance.GuardarPosicioTeleport(posicionDestino, true);
                Debug.Log($"Cortinilla: Guardada posición de destino {posicionDestino} para teleport usando SistemaPerks");
            }
            else
            {
                Debug.LogError("Cortinilla: No se encontró la instancia de SistemaPerks para guardar la posición");
            }
        }
        
        // Cargar la nueva escena
        SceneManager.LoadScene(nombreEscena);
        
        // La cortinilla seguirá visible durante la carga de la escena
        // El evento OnSceneLoaded se encargará de completar el proceso
    }
}