using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SistemaVida : MonoBehaviour
{
    protected abstract void Awake();

    protected abstract void Start();

    public abstract bool EsViu();

    // Método abstracto para gestionar la muerte
    protected abstract IEnumerator Morir();

    // Método abstracto para decrementar vida
    public abstract void DecrementarVida(int quantitat);
    
    // Método abstracto para incrementar vida
    public abstract void IncrementarVida(int quantitat);
    
    // Método abstracto para notificar cambios de vida
    protected abstract void NotificarCanviVida();
}