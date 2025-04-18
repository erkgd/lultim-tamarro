using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaJugador : SistemaVida
{
    // Componentes UI y efectos
    [SerializeField] private VidaUI vidaUI;
    [SerializeField] private Cortinilla cortinilla;
    [SerializeField] private float tempsReviure = 1f;

    // Propiedades internas para gestionar la vida
    [SerializeField] private int vidaMaxima = 24;
    [SerializeField] private int vidaActual = 12;
    
    // Eventos para comunicación
    public event Action OnVidaCanviada;

    // Referencias
    private Animator animator;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        if (vidaUI == null) vidaUI = FindObjectOfType<VidaUI>();
        if (cortinilla == null) cortinilla = FindObjectOfType<Cortinilla>();
    }

    private void Start()
    {
        // COPILOT: Leer si el animal fue salvado y ajustar la UI
        bool animalSalvado = PlayerPrefs.GetInt("AnimalSalvat", 0) == 1;

        if (animalSalvado)
        {
            // Ya salvado → activa la perk de vida extra
            ActivarPerkVidaExtra();
        }
        else
        {
            // No salvado → forzar UI de 6 corazones
            Debug.Log("🐾 Animal NO salvado: UI en Type1 (6 corazones)");
            vidaMaxima = 12;  // 6 corazones × 2 HP
            vidaActual = Mathf.Min(vidaActual, vidaMaxima);

            if (vidaUI != null)
            {
                vidaUI.SetDisplayType(VidaUI.DisplayType.Type1); // COPILOT: Usar el nuevo método
            }
        }

        // Refrescar siempre la UI según vidaActual
        ActualitzarUI();
    }

    /// <summary>
    /// Incrementa la vida máxima a 20 (10 corazones) y cambia la UI.
    /// </summary>
    public void ActivarPerkVidaExtra()
    {
        Debug.Log("🟢 [SistemaVidaJugador] Activando perk de vida extra");
        vidaMaxima = 20;
        vidaActual = Mathf.Min(vidaActual, vidaMaxima);

        if (vidaUI != null)
        {
            vidaUI.SetDisplayType(VidaUI.DisplayType.Type2); // COPILOT: Usar el nuevo método
        }
        NotificarCanviVida();
    }

    public bool EsViu() => vidaActual > 0;

    public void IncrementarVida(int quantitat)
    {
        if (quantitat <= 0) return;
        vidaActual = Mathf.Min(vidaActual + quantitat, vidaMaxima);
        NotificarCanviVida();
    }

    public void DecrementarVida(int quantitat)
    {
        if (quantitat <= 0) { Debug.Log("Daño ≤0, no aplicado"); return; }
        if (InvencibilitatJugador.Instance?.EsInvencible == true)
        {
            Debug.Log("Jugador invencible, no aplica daño");
            return;
        }
        if (!EsViu()) { Debug.Log("Jugador ya muerto"); return; }

        Debug.Log($"Vida antes: {vidaActual}");
        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        Debug.Log($"Vida después: {vidaActual} (-{quantitat})");
        NotificarCanviVida();

        if (InvencibilitatJugador.Instance != null && SistemaPerks.Instance.EstaDesbloquejada(1))
        {
            InvencibilitatJugador.Instance.ActivarInvencibilitat();
        }

        if (vidaActual <= 0)
            StartCoroutine(Morir());
    }

    public override IEnumerator Morir()
    {
        animator?.SetBool("senseVida", true);
        InvocarMuerte();

        if (cortinilla == null) cortinilla = FindObjectOfType<Cortinilla>();
        if (cortinilla != null)
        {
            cortinilla.ResetearCortinilla();
            cortinilla.MostrarCortinilla();
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogWarning("No se pudo mostrar cortinilla al morir");
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(tempsReviure);
        TeleportarAlHub(true);
        ReviureJugador();
    }

    public void ReviureJugador()
    {
        vidaActual = vidaMaxima;
        animator?.SetBool("senseVida", false);
        InvocarRevivir();
        NotificarCanviVida();
    }

    protected override void NotificarCanviVida()
    {
        base.NotificarCanviVida();
        ActualitzarUI();
        OnVidaCanviada?.Invoke();
    }

    private void ActualitzarUI()
    {
        if (vidaUI != null)
            vidaUI.UpdateHealth(vidaActual);
    }

    public override void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }

    public void TeleportarAlHub(bool usarCortinilla = true)
    {
        var posicionador = GetComponent<PosicionadorJugador>() 
                         ?? gameObject.AddComponent<PosicionadorJugador>();

        if (SistemaPerks.Instance != null)
        {
            SistemaPerks.Instance.GuardarValor("UsarCortinilla", usarCortinilla ? "1" : "0");
            SistemaPerks.Instance.GuardarValor("LastSpawnPoint", "Hub");
            SistemaPerks.Instance.GuardarPosicioTeleport(TPConstants.HUB_SPAWN_POINT);
        }
        else
        {
            PlayerPrefs.SetString("LastSpawnPoint", "Hub");
            PlayerPrefs.SetString("UsarCortinilla", usarCortinilla ? "1" : "0");
            PlayerPrefs.Save();
        }

        posicionador.IniciarTeleport(TPConstants.HUB_SPAWN_POINT, TPConstants.HUB_SCENE);
    }
}