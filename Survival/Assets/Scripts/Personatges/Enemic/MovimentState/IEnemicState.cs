using UnityEngine;

/// <summary>
/// Interfaz para los estados del enemigo
/// </summary>
public interface IEnemicState 
{
    /// <summary>
    /// Se ejecuta al entrar en el estado
    /// </summary>
    void EnterState(MovimentEnemic moviment);
    
    /// <summary>
    /// Se ejecuta cada frame mientras el estado está activo
    /// </summary>
    void UpdateState(MovimentEnemic moviment);
    
    /// <summary>
    /// Se ejecuta al salir del estado
    /// </summary>
    void ExitState(MovimentEnemic moviment);
    
    /// <summary>
    /// Devuelve el tipo de estado
    /// </summary>
    AIState GetStateType();
}
