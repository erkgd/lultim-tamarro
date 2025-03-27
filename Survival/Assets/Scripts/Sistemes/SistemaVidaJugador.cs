using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SistemaVidaJugador : SistemaVida
{
    // Associem un jugador al sistema de vida
    private Jugador jugador;
    private VidaUI vidaUI;
    private Cortinilla cortinilla;

    protected override void Awake()
    {
        // Inicialitzem el component Jugador
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        // Referència al component Jugador associat
        jugador = GetComponent<Jugador>();
        vidaUI = FindObjectOfType<VidaUI>();
        if (vidaUI != null)
            vidaUI.ActualitzarVida(jugador.VidaActual);
        cortinilla = FindObjectOfType<Cortinilla>();
    }

    public override bool EsViu()
    {
        return jugador != null && jugador.VidaActual > 0;
    }

    // Carreguem la dependencia per morir
    protected override IEnumerator Morir()
    {
        if (!EsViu() && jugador != null)
        {
            return jugador.Morir();
        }
        yield break;
    }

    public override void IncrementarVida(int quantitat)
    {
        if (quantitat <= 0) return;
        
        vidaActual = Mathf.Min(vidaActual + quantitat, vidaMaxima);
        NotificarCanviVida();
    }

    public override void DecrementarVida(int quantitat)
    {
        if (quantitat <= 0 || invencibilitatJugador.EsInvencible) return;

        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        NotificarCanviVida();

        Jugador.invencibilitatJugador.ActivarInvencibilitat();

        if (vidaActual <= 0 && cortinilla != null)
        {
            StartCoroutine(Morir());
            cortinilla.MostrarCortinilla();
        }
    }

    protected override void NotificarCanviVida()
    {
        // Actualizar la UI si existe un componente VidaUI
        if (vidaUI != null)
        {
            vidaUI.ActualitzarVida(jugador.VidaActual);
        }
    }
}