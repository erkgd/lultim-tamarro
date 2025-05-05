using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeleportJugador : MonoBehaviour
{
    // Enum for predefined teleport destinations
    public enum TeleportDestination
    {
        Custom,
        Pedrosa,
        Hub,
        Tutorial,
        Bosc,
        Engo,
        Ramio
    }

    [Header("Destí")]
    [SerializeField] private TeleportDestination destinacioSeleccionada = TeleportDestination.Custom;
    [SerializeField] private string nomEscenaDestí = "Escena Principal";
    [SerializeField] private Vector3 posicioDestí;

    [Header("Configuració")]
    [SerializeField] private string etiquetaJugador = "Player";
    [SerializeField] private bool mostrarDebug = true;

    // ***** NOU CAMP *****
    [Header("Requisit de Perk")]
    [Tooltip("Índex de la perk requerida per activar aquest teleport (-1 si no es requereix cap perk). Índexs: 0=Velocitat, 1=Resistència, 2=Atac, 3=Vida")]
    [SerializeField] private int indexPerkRequerit = -1; // Per defecte, no es requereix cap perk

    [Tooltip("Si està activat, el camí es tancarà si el jugador té la perk requerida, en comptes d'obrir-se")]
    [SerializeField] private bool tancarCamiSiTePerk = false;

    private bool teletransportant = false; // Per evitar múltiples activacions

    private void OnValidate()
    {
        // Update destination values when selection changes in inspector
        switch (destinacioSeleccionada)
        {
            case TeleportDestination.Pedrosa:
                nomEscenaDestí = TPConstants.PEDROSA_SCENE;
                posicioDestí = TPConstants.PEDROSA_SPAWN_POINT;
                break;

            case TeleportDestination.Hub:
                nomEscenaDestí = TPConstants.HUB_SCENE;
                posicioDestí = TPConstants.HUB_SPAWN_POINT;
                break;

            case TeleportDestination.Tutorial:
                nomEscenaDestí = TPConstants.TUTORIAL_SCENE;
                posicioDestí = TPConstants.TUTORIAL_SPAWN_POINT;
                break;

            case TeleportDestination.Bosc:
                nomEscenaDestí = TPConstants.BOSC_SCENE;
                posicioDestí = TPConstants.BOSC_SPAWN_POINT;
                break;

            case TeleportDestination.Engo:
                nomEscenaDestí = TPConstants.ENGO_SCENE;
                posicioDestí = TPConstants.ENGO_SPAWN_POINT;
                break;

            case TeleportDestination.Ramio:
                nomEscenaDestí = TPConstants.RAMIO_SCENE;
                posicioDestí = TPConstants.RAMIO_SPAWN_POINT;
                break;

            case TeleportDestination.Custom:
                // Do nothing, keep custom values
                break;
        }
    }

    void Start()
    {
        if (mostrarDebug) Debug.Log($"TeleportJugador inicialitzat a {destinacioSeleccionada}. Requereix perk: {indexPerkRequerit}");

        if (string.IsNullOrEmpty(nomEscenaDestí))
        {
            Debug.LogError($"TeleportJugador ({name}): El nom de l'escena de destí no pot estar buit.");
        }

        // Verificar si las coordenadas del destino seleccionado son (0,0,0)
        if (posicioDestí == Vector3.zero && destinacioSeleccionada != TeleportDestination.Custom)
        {
            Debug.LogWarning($"TeleportJugador ({name}): La posición de destino para {destinacioSeleccionada} es (0,0,0). Verifica TPConstants.cs");
        }
    }

    private IEnumerator OnTriggerEnter(Collider algo)
    {
        if (teletransportant) yield break; // Evita execucions múltiples si ja s'està processant

        if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Col·lisió detectada amb: {algo.name}");

        if (algo.CompareTag(etiquetaJugador))
        {
            if (algo.GetComponent<Jugador>() != null)
            {
                // ***** COMPROVACIÓ DE LA PERK *****
                bool perkDesbloquejada = false;
                if (indexPerkRequerit < 0) // Si no es requereix cap perk (índex -1)
                {
                    perkDesbloquejada = true;
                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): No es requereix cap perk específica.");
                }
                else if (SistemaPerks.Instance != null)
                {
                    perkDesbloquejada = SistemaPerks.Instance.EstaDesbloquejada(indexPerkRequerit);
                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Comprovant perk índex {indexPerkRequerit}. Estat: {(perkDesbloquejada ? "DESBLOQUEJADA" : "BLOQUEJADA")}");
                }
                else
                {
                    Debug.LogError($"TeleportJugador ({name}): SistemaPerks.Instance no trobat! No es pot comprovar la perk requerida.");
                    // Decideix si vols permetre el teleport igualment o bloquejar-lo si el sistema de perks falla
                    // perkDesbloquejada = false; // Bloquejar per seguretat
                    // perkDesbloquejada = true; // Permetre si falla (compte!)
                }

                // Si la perk està desbloquejada (o no es requereix), procedim
                if (perkDesbloquejada && !tancarCamiSiTePerk)
                {
                    teletransportant = true; // Marquem que estem processant el teleport

                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Jugador vàlid detectat: {algo.name}. Perk requerida ({indexPerkRequerit}) complerta. Iniciant teletransport a {nomEscenaDestí} en posició {posicioDestí}");

                    Cortinilla cortinilla = FindObjectOfType<Cortinilla>();
                    if (cortinilla != null)
                    {
                        cortinilla.ResetearCortinilla();
                        cortinilla.MostrarCortinilla(); // Activa la cortinilla (tancament)
                        if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Mostrant cortinilla...");
                        yield return new WaitForSeconds(1.5f); // Espera a que la cortinilla faci l'efecte (ajusta durada si cal)
                    }
                    else
                    {
                        Debug.LogError($"TeleportJugador ({name}): No s'ha trobat la cortinilla a l'escena.");
                        // Espera igualment una mica si no hi ha cortinilla?
                        yield return new WaitForSeconds(0.2f);
                    }

                    TeletransportarJugador(algo.gameObject);
                    // No resetegem teletransportant aquí, ja que es carrega una nova escena
                }
                else
                {
                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): El jugador no té la perk requerida (índex {indexPerkRequerit}). Teleport bloquejat.");
                    // Opcional: Pots afegir algun feedback visual o sonor per indicar que el teleport està bloquejat.
                }
            }
            else
            {
                 if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): L'objecte {algo.name} té l'etiqueta correcta però no el component Jugador.");
            }
        }
    }

    private void TeletransportarJugador(GameObject jugador)
    {
        if (jugador != null)
        {
            if (posicioDestí == Vector3.zero && destinacioSeleccionada != TeleportDestination.Custom)
            {
                Debug.LogWarning($"TeleportJugador ({name}): ¡Advertència! Teleportant a posició (0,0,0) des de TeleportDestination.{destinacioSeleccionada}");
            }

            if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Teleportant jugador a: {posicioDestí} en escena: {nomEscenaDestí}");

            PosicionadorJugador posicionador = jugador.GetComponent<PosicionadorJugador>();
            if (posicionador == null)
            {
                 if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): No s'ha trobat component PosicionadorJugador, afegint-lo automàticament");
                posicionador = jugador.AddComponent<PosicionadorJugador>();
            }

            if (posicionador != null)
            {
                 if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Utilitzant PosicionadorJugador.IniciarTeleport");
                // Guardem la informació del teleport per a la següent escena
                SistemaPerks.Instance?.GuardarPosicioTeleport(posicioDestí, true);
                // Carreguem l'escena directament. PosicionadorJugador a la nova escena s'encarregarà de la posició.
                SceneManager.LoadScene(nomEscenaDestí);
            }
            else
            {
                Debug.LogError($"TeleportJugador ({name}): No s'ha pogut crear el component PosicionadorJugador, fallant al teleportar");
                // Com a fallback, intentem carregar l'escena igualment
                 SistemaPerks.Instance?.GuardarPosicioTeleport(posicioDestí, true);
                SceneManager.LoadScene(nomEscenaDestí);
            }
        }
        else
        {
            Debug.LogError($"TeleportJugador ({name}): L'objecte jugador és nul. No es pot teletransportar.");
        }
    }
}