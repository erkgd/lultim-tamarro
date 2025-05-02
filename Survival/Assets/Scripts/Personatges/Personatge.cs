using System;
using System.Collections;
using UnityEngine;

public abstract class Personatge : MonoBehaviour
{
    // Event vida
    public event Action QuanCanviVida;

    // Propietats
    public abstract float VidaActual { get; }
    public abstract float VidaMaxima { get; }
    public abstract float Dany { get; }
    public abstract float ForcaKnockback { get; }
    public abstract bool EstaAtacant();

    protected abstract void Awake();

    protected abstract void Start();
    
    public abstract void DecrementarVida(float quantitat, string font = "");

    public abstract IEnumerator Morir();

    public abstract void Atacar();

    public abstract IEnumerator ExecutarAtac();
    
    // Método protegido para que las clases derivadas puedan invocar el evento
    protected void InvocarQuanCanviVida()
    {
        QuanCanviVida?.Invoke();
    }
}