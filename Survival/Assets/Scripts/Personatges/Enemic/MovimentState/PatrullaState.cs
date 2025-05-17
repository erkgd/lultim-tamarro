using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de patrulla del enemigo
/// </summary>
public class PatrullaState : IEnemicState
{
    private int puntActual = 0;
    private bool esperantEnPunt = false;
    
    public void EnterState(MovimentEnemic moviment)
    {
        Transform[] puntsPatrulla = moviment.GetPuntsPatrulla();
        
        if (puntsPatrulla == null || puntsPatrulla.Length == 0)
            return;
            
        NavMeshAgent agent = moviment.GetAgent();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = moviment.VelocitatNormal;
            puntActual = 0;
            agent.SetDestination(puntsPatrulla[0].position);
        }
        
        Debug.Log($"Enemic {moviment.gameObject.name}: Entrant en estat PATRULLA");
    }
    
    public void UpdateState(MovimentEnemic moviment)
    {
        // Si detectamos al jugador, cambiamos al estado de persecución
        float distanciaJugador = moviment.GetDistanciaAlJugador();
        if (distanciaJugador <= moviment.RangPerseguir)
        {
            moviment.CanviarEstat(AIState.PERSEGUIR);
            return;
        }
        
        // Si no, seguimos con la patrulla
        ActualitzarPatrulla(moviment);
    }
    
    public void ExitState(MovimentEnemic moviment)
    {
        // Nada específico al salir de estado de patrulla
    }
    
    public AIState GetStateType()
    {
        return AIState.PATRULLA;
    }
    
    private void ActualitzarPatrulla(MovimentEnemic moviment)
    {
        Transform[] puntsPatrulla = moviment.GetPuntsPatrulla();
        
        // Si no hay puntos o estamos esperando, no hacemos nada
        if (puntsPatrulla == null || puntsPatrulla.Length == 0 || esperantEnPunt)
            return;
        
        NavMeshAgent agent = moviment.GetAgent();
        // Verificar que el agente esté activo y en un NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            // Si hemos llegado al punto actual, avanzamos al siguiente
            if (agent.remainingDistance <= 0.5f)
            {
                moviment.StartCoroutine(EsperarEnPunt(moviment));
            }
        }
    }
    
    private IEnumerator EsperarEnPunt(MovimentEnemic moviment)
    {
        esperantEnPunt = true;
        
        NavMeshAgent agent = moviment.GetAgent();
        Transform[] puntsPatrulla = moviment.GetPuntsPatrulla();
        
        // Verificar que el agente esté activo y en un NavMesh antes de detenerlo
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            // Detenemos el agente
            agent.isStopped = true;
        }
        
        // Esperamos un momento
        yield return new WaitForSeconds(moviment.TempsEsperaPatrulla);
        
        // Avanzamos al siguiente punto
        puntActual = (puntActual + 1) % puntsPatrulla.Length;
        
        // Verificar que el agente esté activo y en un NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(puntsPatrulla[puntActual].position);
            
            // Reactivamos el agente
            agent.isStopped = false;
        }
        
        esperantEnPunt = false;
    }
}
