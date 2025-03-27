using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SistemaVidaEnemic : SistemaVida
{
    // Associem un enemic al sistema de vida
    private Enemic enemic;

    protected override void Awake()
    {
        // Inicialitzem el component Enemic
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        // Referència al component Enemic associat
        enemic = GetComponent<Enemic>();
    }

    public override bool EsViu()
    {
        return enemic != null && enemic.VidaActual > 0;
    }

    // Carreguem la dependencia per morir
    protected override IEnumerator Morir()
    {
        if (enemic != null)
        {
            return enemic.Morir();
        }
        yield break;
    }

    public override void IncrementarVida(int quantitat)
    {
        if (enemic != null && quantitat > 0)
        {
            enemic.IncrementarVida(quantitat);
            NotificarCanviVida();
        }
    }

    public override void DecrementarVida(int quantitat)
    {
        if (quantitat <= 0 || !EsViu()) return;
        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        // Activamos la animación de recibir daño si no estamos muriendo
        if (vidaActual > 0 && animator != null)
            animator.SetTrigger("TrRepMal");

        // Notificamos el cambio de vida
        NotificarCanviVida();

        // Si la vida llega a 0, iniciamos la muerte
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            //StartCoroutine(Morir());
        }
    }

    protected override void NotificarCanviVida()
    {
        //notificaxcion de cambio de vida aun por implementar, igual necesitamos un event handler.
    }
}