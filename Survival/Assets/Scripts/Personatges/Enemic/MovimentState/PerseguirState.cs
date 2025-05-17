using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Estado de persecución del enemigo
/// </summary>
public class PerseguirState : IEnemicState
{
    private float comptadorAtacs = 0f;
    
    public void EnterState(MovimentEnemic moviment)
    {
        NavMeshAgent agent = moviment.GetAgent();
        
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = moviment.VelocitatPersecucio;
        }
        
        Debug.Log($"Enemic {moviment.gameObject.name}: Entrant en estat PERSEGUIR");
    }
    
    public void UpdateState(MovimentEnemic moviment)
    {
        Transform jugador = moviment.GetJugador();
        NavMeshAgent agent = moviment.GetAgent();
        
        if (jugador == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;
            
        // Actualizamos el contador de ataques
        if (comptadorAtacs > 0)
        {
            comptadorAtacs -= Time.deltaTime;
        }
        
        // Perseguimos al jugador
        agent.SetDestination(jugador.position);
        
        float distanciaJugador = moviment.GetDistanciaAlJugador();
        
        // Si estamos lo suficientemente cerca, atacamos
        if (distanciaJugador <= moviment.RangAtacar && comptadorAtacs <= 0)
        {
            IntentarAtacar(moviment);
        }
        
        // Si el jugador se aleja demasiado, pasamos a sospecha
        if (distanciaJugador > moviment.RangPerseguir)
        {
            moviment.CanviarEstat(AIState.SOSPITA);
        }
    }
    
    public void ExitState(MovimentEnemic moviment)
    {
        // No hay acciones específicas al salir de estado de persecución
    }
    
    public AIState GetStateType()
    {
        return AIState.PERSEGUIR;
    }
    
    private void IntentarAtacar(MovimentEnemic moviment)
    {
        NavMeshAgent agent = moviment.GetAgent();
        Enemic enemic = moviment.GetEnemic();
        
        // Detenemos temporalmente
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        
        // Activamos el ataque
        if (enemic != null)
        {
            enemic.Atacar();
        }
        
        // Reiniciamos el contador
        comptadorAtacs = moviment.TempsEntreAtacs;
        
        // Reanudamos el movimiento después de un momento
        moviment.StartCoroutine(ReanudarMoviment(moviment, 0.5f));
    }
    
    private IEnumerator ReanudarMoviment(MovimentEnemic moviment, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        NavMeshAgent agent = moviment.GetAgent();
        // Verificar que el agente esté activo y en un NavMesh
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
}
