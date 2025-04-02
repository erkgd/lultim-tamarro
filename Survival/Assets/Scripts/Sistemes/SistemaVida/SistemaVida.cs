using System;
using System.Collections;
using UnityEngine;

public abstract class SistemaVida : MonoBehaviour
{
    // Evento para notificar cambios de vida
    public event Action QuanCanviVida;
    
    // Eventos adicionales para la gestión de estados
    public event Action OnMuerte;
    public event Action OnRevivir;
    
    // Métodos para suscripción a eventos
    public abstract void SubscribeToQuanCanviVida(Action handler);
    
    // Métodos para notificación de cambios
    protected virtual void NotificarCanviVida()
    {
        QuanCanviVida?.Invoke();
    }
    
    // Método para invocar evento de muerte
    protected virtual void InvocarMuerte()
    {
        OnMuerte?.Invoke();
    }
    
    // Método para invocar evento de revival
    protected virtual void InvocarRevivir()
    {
        OnRevivir?.Invoke();
    }
    
    // Métodos virtuales para ser sobrescritos
    public virtual void Awake() { }
    
    // Método para gestionar la muerte
    public virtual IEnumerator Morir()
    {
        yield break;
    }
}