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
    
    public abstract void DecrementarVida(int quantitat, string font = "");

    public abstract IEnumerator Morir();

    public abstract void Atacar();

    public abstract IEnumerator ExecutarAtac();
    
    // Método protegido para que las clases derivadas puedan invocar el evento
    protected void InvocarQuanCanviVida()
    {
        QuanCanviVida?.Invoke();
    }
}