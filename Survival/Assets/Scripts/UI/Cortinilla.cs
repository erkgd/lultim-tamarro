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
    
    // Control per activar solo una vez
    private bool yaSeHaMostrado = false;
    
    // Control per la auto-destrucción después de una transición
    private bool transicionEnProgreso = false;    
    
    private void Awake()
    {
        // Implementación de singleton persistente
        if (Instance == null)
        {
            // SOLUCIÓN AL ERROR: Convertir el GameObject en un objeto raíz antes de usar DontDestroyOnLoad
            // Guardamos una referencia al transform padre original
            Transform parentOriginal = transform.parent;
            
            // Hacemos el objeto independiente (raíz) antes de usar DontDestroyOnLoad
            transform.SetParent(null);
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("Cortinilla: GameObject convertido en raíz para poder usar DontDestroyOnLoad");
            
            // Registramos un evento para detectar cambios de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // IMPORTANTE: Primero buscamos específicamente el GameObject ImageCortinilla en la escena actual
            GameObject[] allGameObjects = FindObjectsOfType<GameObject>(true); // incluye objetos inactivos
            GameObject imageCortinillaGO = null;
            
            foreach (GameObject go in allGameObjects)
            {
                if (go.name == "ImageCortinilla")
                {
                    imageCortinillaGO = go;
                    Debug.Log($"Cortinilla: Encontrado ImageCortinilla en {go.transform.parent?.name ?? "root"}");
                    break;
                }
            }
              // Si lo encontramos, lo preservamos
            if (imageCortinillaGO != null)
            {
                // Guardamos la referencia a la imagen
                imatgeCortinilla = imageCortinillaGO.GetComponent<Image>();
                
                // Hacemos que ImageCortinilla sea un objeto raíz para poder usar DontDestroyOnLoad
                Transform imageCortinillaParent = imageCortinillaGO.transform.parent;
                imageCortinillaGO.transform.SetParent(null);
                
                // Preservamos el GameObject ImageCortinilla entre escenas
                DontDestroyOnLoad(imageCortinillaGO);
                Debug.Log("Cortinilla: ImageCortinilla convertido en raíz y preservado con DontDestroyOnLoad");
                
                // Asegurar que ImageCortinilla tiene un Canvas configurado correctamente
                Canvas canvas = imageCortinillaGO.GetComponent<Canvas>();
                if (canvas == null)
                {
                    // Añadir Canvas al objeto
                    canvas = imageCortinillaGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 9999; // Prioridad máxima para estar por encima de todo
                    
                    // Añadir CanvasScaler para que se adapte a la resolución
                    CanvasScaler scaler = imageCortinillaGO.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    
                    // Añadir GraphicRaycaster necesario para interacciones
                    imageCortinillaGO.AddComponent<GraphicRaycaster>();
                    
                    Debug.Log("Cortinilla: Configurado nuevo Canvas para la imagen");
                }
                else
                {
                    // Asegurar que el canvas existente está bien configurado
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 9999;
                    Debug.Log("Cortinilla: Canvas existente configurado correctamente");
                }
                
                // Configurar correctamente el RectTransform de la imagen
                RectTransform rectTransform = imageCortinillaGO.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Configurar para que cubra toda la pantalla
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    Debug.Log("Cortinilla: RectTransform configurado para cubrir toda la pantalla");
                }
            }
            else
            {
                // Si no encontramos ImageCortinilla específicamente, buscamos la imagen en el componente local
                if (imatgeCortinilla == null)
                {
                    imatgeCortinilla = GetComponent<Image>();
                    if (imatgeCortinilla == null)
                    {
                        // Si no se encuentra en este GameObject, buscamos en los hijos
                        imatgeCortinilla = GetComponentInChildren<Image>();
                        
                        if (imatgeCortinilla == null)
                        {
                            Debug.LogError("Cortinilla: No se encontró el componente Image ni ImageCortinilla en la escena");
                        }
                    }
                }
            }
            
            // Comprovem que tenim el material
            if (imatgeCortinilla != null && materialCortinilla != null)
            {
                // Creem una instància del material per no modificar l'original
                imatgeCortinilla.material = new Material(materialCortinilla);
                Debug.Log("Cortinilla: Material asignado correctamente a la imagen");
            }
            else if (imatgeCortinilla != null)
            {
                Debug.LogWarning("Cortinilla: No hay material asignado para la cortinilla");
            }
            
            // Ocultem la cortinilla inicialment
            if (imatgeCortinilla != null)
            {
                imatgeCortinilla.gameObject.SetActive(false);
                Debug.Log("Cortinilla: Imagen oculta inicialmente");
            }
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
        Debug.Log($"Cortinilla: OnSceneLoaded detectado en escena {scene.name}, transicionEnProgreso={transicionEnProgreso}");
        
        // Si hay una transición en progreso, esta cortinilla debe manejar la apertura en la nueva escena
        if (transicionEnProgreso && Instance == this)
        {
            Debug.Log("Cortinilla: Transición en progreso, buscando referencias en la nueva escena");
            
            // Aseguramos que la cortinilla sea visible en la jerarquía
            if (gameObject != null && imatgeCortinilla != null) 
            {
                gameObject.SetActive(true);
                Debug.Log("Cortinilla: Objeto activado en DontDestroyOnLoad");
                
                // Cuando hay transición en progreso, comprobamos si debemos deshacer el efecto
                string usarCortinillaStr = SistemaPerks.Instance != null 
                    ? SistemaPerks.Instance.ObtenirValorString("UsarCortinilla", "1") 
                    : PlayerPrefs.GetString("UsarCortinilla", "1");
                    
                bool usarCortinilla = usarCortinillaStr == "1";
                
                if (usarCortinilla)
                {
                    Debug.Log("Cortinilla: Deshaciendo efecto de cortinilla después de transición entre escenas");
                    // Esperar un momento antes de deshacer el efecto
                    StartCoroutine(DesferCortinillaConRetardo());
                }
                
                // Marcamos la transición como completada
                transicionEnProgreso = false;
            }
        }
        else if (!transicionEnProgreso && Instance == this)
        {
            // No hay transición en progreso, comprobamos si esta cortinilla es necesaria
            GameObject uiObject = GameObject.Find("UI");
            bool hayOtraCortinilla = false;
            
            if (uiObject != null)
            {
                Transform imageCortinillaTransform = uiObject.transform.Find("ImageCortinilla");
                if (imageCortinillaTransform != null && imageCortinillaTransform.GetComponent<Cortinilla>() != null
                    && imageCortinillaTransform.GetComponent<Cortinilla>() != this)
                {
                    hayOtraCortinilla = true;
                }
            }
            
            // Si hay otra cortinilla en la escena y no estamos en transición, podemos destruir esta
            if (hayOtraCortinilla && !imatgeCortinilla.gameObject.activeInHierarchy)
            {
                Debug.Log("Cortinilla: Instancia destruida por existir otra cortinilla en la escena");
                if (Instance == this)
                {
                    Instance = null;
                }
                Destroy(gameObject);
            }
        }
    }    // Mètode públic per mostrar la cortinilla
    public void MostrarCortinilla()
    {
        // Si ya se ha mostrado una vez y estamos en una muerte, forzamos su reinicio
        if (yaSeHaMostrado)
        {
            Debug.Log("Cortinilla: Forzando reinicio de la cortinilla para la transición por muerte");
            ResetearCortinilla();
        }
        
        // Activem el GameObject
        if (imatgeCortinilla != null)
        {
            // Marcamos que hay una transición en progreso
            transicionEnProgreso = true;
            
            // Nos aseguramos que la imagen está activa
            if (!imatgeCortinilla.gameObject.activeInHierarchy)
            {
                imatgeCortinilla.gameObject.SetActive(true);
            }
            
            // Animamos la cortinilla (efecto de cierre)
            StartCoroutine(AnimarCortinilla());
            
            // Marcamos que ya se ha mostrado
            yaSeHaMostrado = true;
            
            Debug.Log("Cortinilla: Efecto de cierre iniciado correctamente");
        }
        else
        {
            Debug.LogError("Cortinilla: No se ha encontrado la imagen de la cortinilla");
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

    // Corrutina especial per a l'animació inversa
    private IEnumerator AnimarCortinillaInversa(bool originalInverseEffect)
    {
        float tempsInici = Time.time;
        float percentatgeCompletat = 0f;
        
        // Valor inicial i final del radi (invertits respecto a la animació normal)
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
        
        // Restauramos el valor original del efecte invers
        inverseEffect = originalInverseEffect;
        
        // Ocultem la cortinilla al finalitzar l'animació inversa
        imatgeCortinilla.gameObject.SetActive(false);
        
        // Com que s'ha desfet, podem permetre que es mostri novament
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
      // Corrutina per desfer la cortinilla amb un petit retard després d'un canvi d'escena
    private IEnumerator DesferCortinillaConRetardo()
    {
        // Petit retard per assegurar que l'escena està completament carregada
        // Aumentado para dar tiempo a que todos los objetos estén inicializados
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("Cortinilla: Iniciando apertura de cortinilla tras cambio de escena");
        
        // Verificar si por alguna razón perdimos la referencia a la imagen
        if (imatgeCortinilla == null)
        {
            // Buscar ImageCortinilla en DontDestroyOnLoad
            GameObject imageCortinillaGO = GameObject.Find("ImageCortinilla");
            if (imageCortinillaGO != null)
            {
                imatgeCortinilla = imageCortinillaGO.GetComponent<Image>();
                Debug.Log("Cortinilla: Recuperada referencia a ImageCortinilla");
            }
        }
        
        // Assegurar-nos que la cortinilla està activa i llesta per obrir-se
        if (imatgeCortinilla != null)
        {
            // Aseguramos que el GameObject de la imagen esté activo
            if (!imatgeCortinilla.gameObject.activeInHierarchy)
            {
                imatgeCortinilla.gameObject.SetActive(true);
                Debug.Log("Cortinilla: Imagen activada para la apertura");
            }
            
            // Restaurar el valor del shader si es necesario
            imatgeCortinilla.material.SetFloat(RadioProperty, inverseEffect ? 0f : 1f);
            
            // Desfer l'efecte (obrir la cortinilla)
            ResetearCortinilla();
            DesferCortinilla();
            Debug.Log("Cortinilla: Efecto de apertura iniciado correctamente");
        }
        else
        {
            Debug.LogError("Cortinilla: No se puede deshacer el efecto porque falta la referencia a la imagen");
        }
    }
}