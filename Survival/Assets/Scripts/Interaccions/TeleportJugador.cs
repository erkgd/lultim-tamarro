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
    [SerializeField] private bool bloquejarSiTePerk = false;

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
        if (teletransportant) yield break; // Evita execucions múltiples

        if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Col·lisió detectada amb: {algo.name}");

        if (algo.CompareTag(etiquetaJugador))
        {
            if (algo.GetComponent<Jugador>() != null)
            {
                // ***** LÒGICA DE COMPROVACIÓ DE PERK MODIFICADA *****

                bool potProcedir = false; // Indica si el teleport hauria de continuar

                if (indexPerkRequerit < 0)
                {
                    // Si no es requereix/comprova cap perk, siempre es pot procedir
                    potProcedir = true;
                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): No es comprova cap perk específica. Teleport permès.");
                }
                else
                {
                    // Es requereix comprovar una perk específica
                    if (SistemaPerks.Instance != null)
                    {
                        bool teLaPerk = SistemaPerks.Instance.EstaDesbloquejada(indexPerkRequerit);

                        if (bloquejarSiTePerk)
                        {
                            // Lògica INVERSA: es pot procedir si NO té la perk
                            potProcedir = !teLaPerk;
                            if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Mode BloquejarSiTePerk activat. Requereix NO tenir perk {indexPerkRequerit}. Estat actual: {(teLaPerk ? "TÉ PERK" : "NO TÉ PERK")}. Teleport {(potProcedir ? "permès" : "bloquejat")}.");
                        }
                        else
                        {
                            // Lògica NORMAL: es pot procedir si SÍ té la perk
                            potProcedir = teLaPerk;
                            if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Mode Normal activat. Requereix TENIR perk {indexPerkRequerit}. Estat actual: {(teLaPerk ? "TÉ PERK" : "NO TÉ PERK")}. Teleport {(potProcedir ? "permès" : "bloquejat")}.");
                        }
                    }
                    else
                    {
                        // Error: no es troba el sistema de perks
                        Debug.LogError($"TeleportJugador ({name}): SistemaPerks.Instance no trobat! No es pot comprovar la perk requerida {indexPerkRequerit}. Bloquejant teleport per seguretat.");
                        potProcedir = false; // Bloqueja el teleport si falla el sistema
                    }
                }


                // Si la condició es compleix (segons la lògica normal o inversa)
                if (potProcedir)
                {
                    teletransportant = true; // Marquem que estem processant el teleport

                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Jugador vàlid detectat: {algo.name}. Condició complerta. Iniciant teletransport a {nomEscenaDestí}...");

                    // Gestió de la cortinilla
                    Cortinilla cortinilla = FindObjectOfType<Cortinilla>();
                    if (cortinilla != null)
                    {
                        cortinilla.ResetearCortinilla();
                        cortinilla.MostrarCortinilla();
                        if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Mostrant cortinilla...");
                        yield return new WaitForSeconds(1.5f); // Temps per l'animació de la cortinilla
                    }
                    else
                    {
                        Debug.LogError($"TeleportJugador ({name}): No s'ha trobat la cortinilla a l'escena.");
                        yield return new WaitForSeconds(0.2f); // Petita espera igualment
                    }

                    // Inicia el procés de canvi d'escena
                    TeletransportarJugador(algo.gameObject);
                }
                else
                {
                    // La condició no s'ha complert
                    if (mostrarDebug) Debug.Log($"TeleportJugador ({name}): Condició de la perk no complerta. Teleport bloquejat.");
                    // Opcional: Feedback visual/sonor per indicar bloqueig
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

                // Si el destino es el HUB, avisamos a SistemaEndgame
                if (nomEscenaDestí == TPConstants.HUB_SCENE && SistemaEndgame.Instance != null)
                {
                    SistemaEndgame.Instance.OnPlayerEnterHub();
                }

                // Carreguem l'escena directament. PosicionadorJugador a la nova escena s'encarregarà de la posició.
                SceneManager.LoadScene(nomEscenaDestí);
            }
            else
            {
                Debug.LogError($"TeleportJugador ({name}): No s'ha pogut crear el component PosicionadorJugador, fallant al teleportar");
                // Com a fallback, intentem carregar l'escena igualment
                 SistemaPerks.Instance?.GuardarPosicioTeleport(posicioDestí, true);

                if (nomEscenaDestí == TPConstants.HUB_SCENE && SistemaEndgame.Instance != null)
                {
                    SistemaEndgame.Instance.OnPlayerEnterHub();
                }

                SceneManager.LoadScene(nomEscenaDestí);
            }
        }
        else
        {
            Debug.LogError($"TeleportJugador ({name}): L'objecte jugador és nul. No es pot teletransportar.");
        }
    }
}