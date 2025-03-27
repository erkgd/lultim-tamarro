using System;
using System.Collections;
using UnityEngine;

public abstract class Personatge : MonoBehaviour
{
    // Event vida
    public event Action QuanCanviVida;

    // Propietats
    public abstract int VidaActual { get; }
    public abstract int VidaMaxima { get; }
    public abstract int Dany { get; }
    public abstract float ForcaKnockback { get; }
    public abstract bool EstaAtacant();

    protected abstract void Awake();

    protected abstract void Start();
    
    public abstract void DecrementarVida(int quantitat);

    protected abstract void NotificarCanviVida();

    protected abstract void SubscribeToQuanCanviVida(Action handler);

    protected abstract void InvokeQuanCanviVida();

    protected abstract IEnumerator Morir();

    public abstract void Atacar();

    protected abstract IEnumerator ExecutarAtac();
}