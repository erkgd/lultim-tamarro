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
    [SerializeField] private float duracioEstaticsEnSegons = 40f;  // 40 segons amb el sol estatic (dia o nit)
    [SerializeField] private float duracioTransicioEnSegons = 5f;   // 5 segons de transició entre dia i nit
    [SerializeField] private bool cicleActiu = true;
    [SerializeField] private bool efecteColorActiu = true;  // Opción para activar/desactivar el cambio de color
    [SerializeField] private bool mostrarDebug = false;     // Activar para ver mensajes de depuración
    
    [Header("Configuració de Llum")]
    [SerializeField] [Range(0.5f, 1.5f)] private float intensitatDiurna = 0.8f;  // Intensidad reducida para evitar posterización
    [SerializeField] [Range(0.0f, 0.5f)] private float intensitatNocturna = 0.2f;
    [SerializeField] [Range(0f, 2f)] private float suavitzatTransicio = 1.2f;    // Factor de suavizado para la transición

    [Header("Referències")]
    [SerializeField] private Light llumSolar;

    private float tempsActual = 0f;
    private float cicleTotalDuracio;
    private bool esDia = false; // Comienza en noche
    // Removed unused field enTransicio
    
    private string estatActual = "Nit";
    private float tempsFaseActual = 0f;
    private int cicleComplet = 0;

    void Start()
    {
        // Verificar que tenemos una referencia a la luz
        if (llumSolar == null)
        {
            Debug.LogError("CicleDiaNit: No s'ha assignat una llum solar!");
            enabled = false;
            return;
        }
        
        // Calcular la duració total d'un cicle complet (dia-transició-nit-transició)
        cicleTotalDuracio = (duracioEstaticsEnSegons * 2) + (duracioTransicioEnSegons * 2);
        
        // Iniciar en la fase de nit
        ConfigurarSolNit();
        
        // Configure el sol per evitar efectes de posterització
        ConfigurarQualitat();
        
        // Establecer el tiempo actual para que empiece en la fase nocturna
        tempsActual = duracioEstaticsEnSegons + duracioTransicioEnSegons;
        
        if (mostrarDebug)
        {
            Debug.Log($"CicleDiaNit iniciat. Duració estàtics: {duracioEstaticsEnSegons}s, Transició: {duracioTransicioEnSegons}s, Total cicle: {cicleTotalDuracio}s");
        }
    }

    void Update()
    {
        if (cicleActiu)
        {
            // Incrementar el temps amb fixation per evitar variacions
            tempsActual += Time.deltaTime;
            tempsFaseActual += Time.deltaTime;
            
            // Reset del cicle quan s'arriba al final
            if (tempsActual >= cicleTotalDuracio)
            {
                tempsActual = 0f;
                cicleComplet++;
                if (mostrarDebug)
                {
                    Debug.Log($"Cicle complet #{cicleComplet}");
                }
            }
            
            ActualitzarCicle();
        }
    }
    
    // Configura la qualitat del sol per evitar posterització
    private void ConfigurarQualitat()
    {
        if (llumSolar != null)
        {
            // Configurar propietats avançades de la llum per evitar posterització
            llumSolar.shadows = LightShadows.Soft;
            llumSolar.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
            llumSolar.shadowBias = 0.05f;
            llumSolar.shadowNormalBias = 0.4f;
            
            // Reducir la intensidad para minimizar el efecto de posterización
            llumSolar.intensity = intensitatDiurna;
        }
    }
    
    // Configura el sol a la posició de migdia exacte
    private void ConfigurarSolMigdia()
    {
        // Posició del sol al migdia (el sol completament amunt)
        llumSolar.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        
        // Color del sol a migdia (blanc amb un toc càlid per ser més natural)
        if (efecteColorActiu)
        {
            llumSolar.color = new Color(1f, 0.98f, 0.95f);
            llumSolar.intensity = intensitatDiurna;
        }
    }
    
    // Configura el sol a la posició de nit (nou mètode)
    private void ConfigurarSolNit()
    {
        // Posició del sol a la nit (el sol completament sota l'horitzó)
        llumSolar.transform.rotation = Quaternion.Euler(90f, -90f, 0f);
        
        // Color del sol a la nit (blau fosc)
        if (efecteColorActiu)
        {
            llumSolar.color = new Color(0.15f, 0.15f, 0.3f);
            llumSolar.intensity = intensitatNocturna;
        }
    }
    
    void ActualitzarCicle()
    {
        string estatAnterior = estatActual;
        
        // Calcular en quina fase del cicle estem
        
        // Fase 1: Dia estàtic (0 a duracioEstaticsEnSegons)
        if (tempsActual < duracioEstaticsEnSegons)
        {
            if (estatActual != "Dia")
            {
                estatActual = "Dia";
                tempsFaseActual = 0f;
                if (mostrarDebug) Debug.Log($"Canvi a fase: {estatActual}, durada: {duracioEstaticsEnSegons}s");
            }
            
            esDia = true;
            // Removed assignment to unused enTransicio
            ActualitzarPosicioSol(0f); // Posició de dia
            ActualitzarColorLlum(0f);  // Color de dia
        }
        // Fase 2: Transició dia a nit (duracioEstaticsEnSegons a duracioEstaticsEnSegons + duracioTransicioEnSegons)
        else if (tempsActual < duracioEstaticsEnSegons + duracioTransicioEnSegons)
        {
            if (estatActual != "TransicioDiaNit")
            {
                estatActual = "TransicioDiaNit";
                tempsFaseActual = 0f;
                if (mostrarDebug) Debug.Log($"Canvi a fase: {estatActual}, durada: {duracioTransicioEnSegons}s");
            }
            
            // Removed assignment to unused enTransicio
            // Calcular el progrés de la transició (0 a 1)
            float progres = (tempsActual - duracioEstaticsEnSegons) / duracioTransicioEnSegons;
            progres = Mathf.Clamp01(progres); // Asegurar que está entre 0 y 1
            ActualitzarPosicioSol(progres);
            ActualitzarColorLlum(progres);
        }
        // Fase 3: Nit estàtica (duracioEstaticsEnSegons + duracioTransicioEnSegons a 2*duracioEstaticsEnSegons + duracioTransicioEnSegons)
        else if (tempsActual < 2 * duracioEstaticsEnSegons + duracioTransicioEnSegons)
        {
            if (estatActual != "Nit")
            {
                estatActual = "Nit";
                tempsFaseActual = 0f;
                if (mostrarDebug) Debug.Log($"Canvi a fase: {estatActual}, durada: {duracioEstaticsEnSegons}s");
            }
            
            esDia = false;
            // Removed assignment to unused enTransicio
            ActualitzarPosicioSol(1f); // Posició de nit
            ActualitzarColorLlum(1f);  // Color de nit
        }
        // Fase 4: Transició nit a dia (2*duracioEstaticsEnSegons + duracioTransicioEnSegons a cicleTotalDuracio)
        else
        {
            if (estatActual != "TransicioNitDia")
            {
                estatActual = "TransicioNitDia";
                tempsFaseActual = 0f;
                if (mostrarDebug) Debug.Log($"Canvi a fase: {estatActual}, durada: {duracioTransicioEnSegons}s");
            }
            
            // Removed assignment to unused enTransicio
            // Calcular el progrés de la transició (1 a 0, invertit)
            float progres = 1f - ((tempsActual - (2 * duracioEstaticsEnSegons + duracioTransicioEnSegons)) / duracioTransicioEnSegons);
            progres = Mathf.Clamp01(progres); // Asegurar que está entre 0 y 1
            ActualitzarPosicioSol(progres);
            ActualitzarColorLlum(progres);
        }
    }
    
    private void ActualitzarPosicioSol(float progres)
    {
        // Progres 0 = dia (sol amunt a -90 graus - mirant cap avall des del cel)
        // Progres 1 = nit (sol avall a 90 graus - mirant cap amunt des de sota terra)
        float angle = -90f + (progres * 180f);
        llumSolar.transform.rotation = Quaternion.Euler(angle, -90f, 0);
    }

    private void ActualitzarColorLlum(float progres)
    {
        if (efecteColorActiu)
        {
            // Aplicar corba de suavització a la transició per fer-la més gradual
            float progresAjustat = SuavitzarTransicio(progres);
            
            // Transició entre colors: dia (blanc càlid) i nit (blau fosc suau)
            Color colorDia = new Color(1f, 0.98f, 0.92f);  // Blanc càlid més suau
            Color colorNit = new Color(0.15f, 0.15f, 0.3f); // Blau fosc per a la nit
            
            llumSolar.color = Color.Lerp(colorDia, colorNit, progresAjustat);
            
            // Ajustar la intensitat amb una corba més gradual per evitar canvis bruscs
            llumSolar.intensity = Mathf.Lerp(intensitatDiurna, intensitatNocturna, progresAjustat);
        }
        else
        {
            // Si el efecto de color está desactivado, mantener un color blanco constante
            llumSolar.color = Color.white;
        }
    }
    
    // Funció per suavitzar la transició i evitar canvis bruscs que causen posterització
    private float SuavitzarTransicio(float valor)
    {
        // Aplica una funció de suavització (Smoothstep)
        if (suavitzatTransicio <= 0)
            return valor;  // Sense suavitzat
            
        // Smoothstep suavitza la transició
        return valor * valor * (3 - 2 * valor);
    }

    // Mètode per pausar/reprendre el cicle (opcional)
    public void AlternarCicle(bool activar)
    {
        cicleActiu = activar;
    }
    
    // Métodos para uso desde otros scripts o para depuración
    public string ObtenerEstadoActual() => estatActual;
    public float ObtenerTiempoRestante()
    {
        switch (estatActual)
        {
            case "Dia":
                return duracioEstaticsEnSegons - tempsFaseActual;
            case "Nit":
                return duracioEstaticsEnSegons - tempsFaseActual;
            case "TransicioDiaNit":
            case "TransicioNitDia":
                return duracioTransicioEnSegons - tempsFaseActual;
            default:
                return 0f;
        }
    }
    
    // New method to check if we're currently in a transition phase
    public bool EstaEnTransicio()
    {
        return estatActual == "TransicioDiaNit" || estatActual == "TransicioNitDia";
    }
}

/*using UnityEngine;
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
}*/