#if !DEFINED_AISTATE
#define DEFINED_AISTATE
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

#if DEFINED_AISTATE
public enum AIState
{
    PATRULLA,
    PERSEGUIR,
    SOSPITA
}
#endif

[RequireComponent(typeof(Enemic))]
public class MovimentEnemic : MonoBehaviour
{
    private Enemic enemic;
    private NavMeshAgent agent;
    private Animator animator;
    private Transform jugador;
    
    [Header("Estat IA")]
    public AIState estatActual = AIState.PATRULLA;
    
    // Estados del patrón State
    private IEnemicState currentState;
    private IEnemicState patrullaState;
    private IEnemicState perseguirState;
    private IEnemicState sospitaState;
    
    private string nomCarpetaPunts;
    private float velocitatNormal;
    private float velocitatPersecucio;
    private float tempsEsperaPatrulla;
    private float rangPerseguir;
    private float tempsSospita;
    private float rangAtacar;    private float tempsEntreAtacs;
    
    private Transform[] puntsPatrulla;
    
    // Propietats públiques requerides per IAEnemic y los estados
    public float VelocitatNormal => velocitatNormal;
    public float VelocitatPersecucio => velocitatPersecucio;
    public float RangPerseguir => rangPerseguir;
    public float RangAtacar => rangAtacar;
    public float TempsEsperaPatrulla => tempsEsperaPatrulla;
    public float TempsSospita => tempsSospita;
    public float TempsEntreAtacs => tempsEntreAtacs;
      private void Awake()
    {
        enemic = GetComponent<Enemic>();
        
        // Inicializar los estados
        patrullaState = new PatrullaState();
        perseguirState = new PerseguirState();
        sospitaState = new SospitaState();
    }
    
    private void Start()
    {
        // Obtenim les referències necessàries
        agent = enemic.Agent;
        animator = enemic.AnimatorEnemic;
        jugador = enemic.Jugador;
        
        // Configurem l'agent
        agent.speed = velocitatNormal;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        
        // Busquem els punts de patrulla a la carpeta
        BuscarPuntsPatrulla();
        
        // Iniciar en estado de patrulla
        CanviarEstat(AIState.PATRULLA);
    }    public void ConfigurarMoviment(
        string nomCarpetaPunts,
        float velocitatNormal,
        float velocitatPersecucio,
        float tempsEsperaPatrulla,
        float rangPerseguir,
        float tempsSospita,
        float rangAtacar,
        float tempsEntreAtacs)
    {
        this.nomCarpetaPunts = nomCarpetaPunts;
        this.velocitatNormal = velocitatNormal;
        this.velocitatPersecucio = velocitatPersecucio;
        this.tempsEsperaPatrulla = tempsEsperaPatrulla;
        this.rangPerseguir = rangPerseguir;
        this.tempsSospita = tempsSospita;
        this.rangAtacar = rangAtacar;
        this.tempsEntreAtacs = tempsEntreAtacs;
    }
    
    private void BuscarPuntsPatrulla()
    {
        // Buscar la carpeta de punts en el mateix nivell
        Transform carpetaPunts = null;
        
        if (transform.parent != null)
        {
            carpetaPunts = transform.parent.Find(nomCarpetaPunts);
        }
        
        // Si s'ha trobat la carpeta, obtenim els punts
        if (carpetaPunts != null && carpetaPunts.childCount > 0)
        {
            puntsPatrulla = new Transform[carpetaPunts.childCount];
            
            for (int i = 0; i < carpetaPunts.childCount; i++)
            {
                puntsPatrulla[i] = carpetaPunts.GetChild(i);
            }
            
            Debug.Log($"Enemic {gameObject.name}: Patrulla configurada amb {puntsPatrulla.Length} punts");
        }
        else
        {
            Debug.LogWarning($"Enemic {gameObject.name}: No s'ha trobat la carpeta '{nomCarpetaPunts}' o no té punts");
        }
    }
    
    // Mètodes que IAEnemic utilitza per controlar l'enemic
    public void IniciarPatrulla()
    {
        CanviarEstat(AIState.PATRULLA);
    }
    
    public void ReprendrePatrulla()
    {
        CanviarEstat(AIState.PATRULLA);
    }
    
    public void AnarA(Vector3 destino)
    {
        if (agent == null) return;
        
        agent.isStopped = false;
        agent.speed = velocitatNormal;
        agent.SetDestination(destino);
    }
    
    public void Perseguir(Transform objectiu, float velocitat)
    {
        if (agent == null || objectiu == null) return;
        
        // Actualizar el jugador si es diferente
        if (objectiu != jugador)
        {
            jugador = objectiu;
        }
        
        CanviarEstat(AIState.PERSEGUIR);
    }
      // Métodos de acceso para los estados
    public Transform GetJugador() => jugador;
    public NavMeshAgent GetAgent() => agent;
    public Enemic GetEnemic() => enemic;
    public Transform[] GetPuntsPatrulla() => puntsPatrulla;
    
    /// <summary>
    /// Obtiene la distancia al jugador
    /// </summary>
    public float GetDistanciaAlJugador()
    {
        if (jugador == null) return float.MaxValue;
        return Vector3.Distance(transform.position, jugador.position);
    }
    
    /// <summary>
    /// Cambia el estado actual del enemigo
    /// </summary>
    public void CanviarEstat(AIState nouEstat)
    {
        // Salir del estado actual si existe
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        
        // Actualizar el estado actual
        estatActual = nouEstat;
        
        // Asignar el nuevo estado
        switch (nouEstat)
        {
            case AIState.PATRULLA:
                currentState = patrullaState;
                break;
            case AIState.PERSEGUIR:
                currentState = perseguirState;
                break;
            case AIState.SOSPITA:
                currentState = sospitaState;
                break;
        }
        
        // Entrar en el nuevo estado
        if (currentState != null)
        {
            currentState.EnterState(this);
        }
    }
    
    private void Update()
    {
        // Si l'enemic no està viu, aturem el moviment
        if (!enemic.EsViu())
        {
            // Verificar que el agente esté activo y en un NavMesh abans de detenirlo
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            return;
        }
        
        // Si no hi ha jugador i no estamos en patrulla, cambiamos a patrulla
        if (jugador == null && estatActual != AIState.PATRULLA)
        {
            CanviarEstat(AIState.PATRULLA);
            return;
        }
        
        // Actualizar el estado actual
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }
      // Ya no necesitamos estos métodos porque se han movido a las clases de estado
}