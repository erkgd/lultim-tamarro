using System;
using System.Collections;
using UnityEngine;

public abstract class Personatge : MonoBehaviour, IVida, IAtacant
{
    [Header("Referències")]
    protected Animator animator;

    [Header("Vida")]
    [SerializeField] protected int vidaActual;
    [SerializeField] protected int vidaMaxima = 5;

    [Header("Atac")]
    [SerializeField] protected int dany = 1;
    [SerializeField] protected float forcaKnockback = 5f;
    protected bool atacant = false;

    // Event vida
    public event Action QuanCanviVida;

    // Propietats interfície IVida
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;

    // Propietats interfície IAtacant
    public int Dany => dany;
    public bool EstaAtacant() => atacant;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        vidaActual = vidaMaxima;
    }

    #region IVida
    public virtual bool EsViu()
    {
        return vidaActual > 0;
    }

    public virtual void IncrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual += quantitat;
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Notifiquem el canvi de vida
        NotificarCanviVida();
    }

    public virtual void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual -= quantitat;

        // Activem l'animació de rebre mal
        if (animator != null)
            animator.SetTrigger("TrRepMal");

        // Si la vida arriba a 0 o menys, iniciem el procés de mort
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Notifiquem el canvi de vida
        NotificarCanviVida();
    }

    // Mètode protegit per notificar canvis de vida (per a classes derivades)
    protected virtual void NotificarCanviVida()
    {
        QuanCanviVida?.Invoke();
    }

    protected void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }

    protected void InvokeQuanCanviVida()
    {
        QuanCanviVida?.Invoke();
    }

    protected abstract IEnumerator Morir();
    #endregion

    #region IAtacant
    public virtual void Atacar()
    {
        if (atacant) return;
        StartCoroutine(ExecutarAtac());
    }

    protected abstract IEnumerator ExecutarAtac();
    #endregion
}