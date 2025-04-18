using System;
using UnityEngine;

// Sistema de gestió de perks (habilitats/avantatges) i dades persistents del jugador.
public class SistemaPerks : MonoBehaviour
{
    // Singleton per a accés global
    public static SistemaPerks Instance { get; private set; }
    // Claus per a PlayerPrefs relacionades amb el teleport
    private const string KEY_DESTIX = "DestiX";
    private const string KEY_DESTIY = "DestiY";
    private const string KEY_DESTIZ = "DestiZ";
    private const string KEY_NECESSITA_TELEPORT = "NecessitaTeleport";
    
    // Array per controlar les perks/habilidades
    [SerializeField] private bool[] perksDesbloquejades = new bool[4];
    // 0--Velocitat (Sprint+rapid)
    // 1--Resistència (Invencibilitat jugador)
    // 2--Atac (atac més fort)
    // 3--Vida (vida extra)    

    public event Action<int> OnPerkChanged; // Evento para notificar cambios en perks

    private void Awake()
    {
        // Configuració del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantenir entre escenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #region Teleport
    // Guarda la posició de destí per a un teleport.
    // @param position: Posició de destí
    // @param necessitaTeleport: Indica si es requereix teleport
    public void GuardarPosicioTeleport(Vector3 position, bool necessitaTeleport = true)
    {
        PlayerPrefs.SetFloat(KEY_DESTIX, position.x);
        PlayerPrefs.SetFloat(KEY_DESTIY, position.y);
        PlayerPrefs.SetFloat(KEY_DESTIZ, position.z);
        PlayerPrefs.SetInt(KEY_NECESSITA_TELEPORT, necessitaTeleport ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"SistemaPerks: Guardada posició de teleport ({position.x}, {position.y}, {position.z}), NecessitaTeleport={necessitaTeleport}");
    }    
    // Obté la posició guardada per a teleport.
    // @returns: Vector3 amb la posició guardada
    public Vector3 ObtenirPosicioTeleport()
    {
        Vector3 posicio = new Vector3(
            PlayerPrefs.GetFloat(KEY_DESTIX, 0f),
            PlayerPrefs.GetFloat(KEY_DESTIY, 0f),
            PlayerPrefs.GetFloat(KEY_DESTIZ, 0f)
        );
        
        return posicio;
    }
    // Verifica si es requereix teleportar al jugador.
    // @returns: True si és necessari teleportar
    public bool NecessitaTeleport()
    {
        return PlayerPrefs.GetInt(KEY_NECESSITA_TELEPORT, 0) == 1;
    }
    // Marca que el teleport ja ha estat realitzat.
    public void MarcarTeleportCompletat()
    {
        PlayerPrefs.SetInt(KEY_NECESSITA_TELEPORT, 0);
        PlayerPrefs.Save();
        Debug.Log("SistemaPerks: Teleport marcat com completat");
    }
    
    #endregion
    #region Altres Preferències
    
    // Aquí es poden afegir altres mètodes per guardar/carregar diferents tipus de dades
    // Guarda un valor a PlayerPrefs.
    public void GuardarValor(string clau, int valor)
    {
        PlayerPrefs.SetInt(clau, valor);
        PlayerPrefs.Save();
    }
    // Guarda un valor a PlayerPrefs.
    public void GuardarValor(string clau, float valor)
    {
        PlayerPrefs.SetFloat(clau, valor);
        PlayerPrefs.Save();
    }
    // Guarda un valor a PlayerPrefs.
    public void GuardarValor(string clau, string valor)
    {
        PlayerPrefs.SetString(clau, valor);
        PlayerPrefs.Save();
    }      
    
    // Obté un valor de PlayerPrefs.
    public int ObtenirValorInt(string clau, int valorPredeterminat = 0)
    {
        return PlayerPrefs.GetInt(clau, valorPredeterminat);
    }
    // Obté un valor de PlayerPrefs.
    public float ObtenirValorFloat(string clau, float valorPredeterminat = 0f)
    {
        return PlayerPrefs.GetFloat(clau, valorPredeterminat);
    }
    // Obté un valor de PlayerPrefs.
    public string ObtenirValorString(string clau, string valorPredeterminat = "")
    {
        return PlayerPrefs.GetString(clau, valorPredeterminat);
    }
    
    #endregion
    #region Perks
    // Comprova si una perk està desbloquejada.
    // @param indexPerk: Índex de la perk (0: Velocitat, 1: Resistència, 2: Atac, 3: Vida)
    // @returns: True si la perk està desbloquejada, false en cas contrari
    public bool EstaDesbloquejada(int indexPerk)
    {
        if (perksDesbloquejades != null && indexPerk >= 0 && indexPerk < perksDesbloquejades.Length)
        {
            return perksDesbloquejades[indexPerk];
        }
        return false;
    }
    // Desbloqueja una perk específica.
    // @param indexPerk: Índex de la perk a desbloquejar (0: Velocitat, 1: Resistència, 2: Atac, 3: Vida)
    public void DesbloquejarPerk(int indexPerk)
    {
        if (perksDesbloquejades != null && indexPerk >= 0 && indexPerk < perksDesbloquejades.Length)
        {
            perksDesbloquejades[indexPerk] = true;
            GuardarEstatPerks();
            Debug.Log($"Perk desbloquejada: {NomPerk(indexPerk)}");
            OnPerkChanged?.Invoke(indexPerk); // Notificar cambio
        }
    }
    // Guarda l'estat de totes les perks a PlayerPrefs.
    private void GuardarEstatPerks()
    {
        for (int i = 0; i < perksDesbloquejades.Length; i++)
        {
            PlayerPrefs.SetInt($"Perk_{i}", perksDesbloquejades[i] ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    // Carrega l'estat de totes les perks des de PlayerPrefs.
    public void CarregarEstatPerks()
    {
        for (int i = 0; i < perksDesbloquejades.Length; i++)
        {
            perksDesbloquejades[i] = PlayerPrefs.GetInt($"Perk_{i}", 0) == 1;
        }
        Debug.Log("Estat de les perks carregat");
    }
    // Obté el nom d'una perk segons el seu índex.
    // @param indexPerk: Índex de la perk
    // @returns: Nom de la perk
    public string NomPerk(int indexPerk)
    {
        switch (indexPerk)
        {
            case 0: return "Velocitat";
            case 1: return "Resistència";
            case 2: return "Atac";
            case 3: return "Vida";
            default: return "Desconeguda";
        }
    }
    
    #endregion
}