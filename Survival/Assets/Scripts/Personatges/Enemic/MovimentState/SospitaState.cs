using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de sospecha del enemigo
/// </summary>
public class SospitaState : IEnemicState
{
    private Vector3 ultimaPosicioVista;
    private float tempsUltimaVegadaVist;
    
    public void EnterState(MovimentEnemic moviment)
    {
        Transform jugador = moviment.GetJugador();
        NavMeshAgent agent = moviment.GetAgent();
        
        ultimaPosicioVista = jugador != null ? jugador.position : moviment.transform.position;
        tempsUltimaVegadaVist = moviment.TempsSospita;
        
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = moviment.VelocitatNormal;
            agent.SetDestination(ultimaPosicioVista);
        }
        
        Debug.Log($"Enemic {moviment.gameObject.name}: Entrant en estat SOSPITA");
    }
    
    public void UpdateState(MovimentEnemic moviment)
    {
        NavMeshAgent agent = moviment.GetAgent();
        
        // Verificar que el agente esté activo y en un NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            // Si hemos llegado al destino y no tenemos uno nuevo
            if (agent.remainingDistance <= 0.5f || agent.destination == Vector3.zero)
            {
                agent.SetDestination(ultimaPosicioVista);
            }
            
            // Reducimos el tiempo de sospecha
            tempsUltimaVegadaVist -= Time.deltaTime;
            
            // Comprobamos si vemos al jugador
            ComprovarVista(moviment);
            
            // Si se agota el tiempo, volvemos a patrullar
            if (tempsUltimaVegadaVist <= 0)
            {
                moviment.CanviarEstat(AIState.PATRULLA);
            }
        }
    }
    
    public void ExitState(MovimentEnemic moviment)
    {
        // No hay acciones específicas al salir del estado de sospecha
    }
    
    public AIState GetStateType()
    {
        return AIState.SOSPITA;
    }
    
    private void ComprovarVista(MovimentEnemic moviment)
    {
        float distanciaJugador = moviment.GetDistanciaAlJugador();
        
        // Si vemos al jugador de nuevo, volvemos a perseguir
        if (distanciaJugador <= moviment.RangPerseguir)
        {
            moviment.CanviarEstat(AIState.PERSEGUIR);
        }
    }
}
