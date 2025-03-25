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
    
    [Header("Configuració")]
    [SerializeField] private string nomCarpetaPunts = "Moviment";
    [SerializeField] private float velocitatNormal = 3.5f;
    [SerializeField] private float velocitatPersecucio = 5.5f;
    
    [Header("Patrulla")]
    [SerializeField] private float tempsEsperaPatrulla = 1f;
    private Transform[] puntsPatrulla;
    private int puntActual = 0;
    private bool esperantEnPunt = false;
    
    [Header("Persecució")]
    [SerializeField] private float rangPerseguir = 10f;
    
    [Header("Sospita")]
    [SerializeField] private float tempsSospita = 3f;
    private float tempsUltimaVegadaVist;
    private Vector3 ultimaPosicioVista;
    
    [Header("Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;
    private float comptadorAtacs = 0f;

    // Propietats públiques requerides per IAEnemic
    public float VelocitatNormal => velocitatNormal;
    public float VelocitatPersecucio => velocitatPersecucio;
    public float RangPerseguir => rangPerseguir;
    public float RangAtacar => rangAtacar;
    
    private void Awake()
    {
        enemic = GetComponent<Enemic>();
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
        
        // Inicialitzem variables
        estatActual = AIState.PATRULLA;
        tempsUltimaVegadaVist = tempsSospita;
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
    
    // Mètode que IAEnemic utilitza per iniciar la patrulla
    public void IniciarPatrulla()
    {
        if (puntsPatrulla == null || puntsPatrulla.Length == 0)
            return;
            
        estatActual = AIState.PATRULLA;
        agent.speed = velocitatNormal;
        puntActual = 0;
        agent.SetDestination(puntsPatrulla[0].position);
    }
    
    // Mètode que IAEnemic utilitza per reprendre la patrulla
    public void ReprendrePatrulla()
    {
        if (puntsPatrulla == null || puntsPatrulla.Length == 0)
            return;
            
        estatActual = AIState.PATRULLA;
        agent.speed = velocitatNormal;
        
        if (!esperantEnPunt)
        {
            agent.SetDestination(puntsPatrulla[puntActual].position);
        }
    }
    
    // Mètode que IAEnemic utilitza per anar a una posició específica
    public void AnarA(Vector3 destino)
    {
        if (agent == null) return;
        
        agent.isStopped = false;
        agent.speed = velocitatNormal;
        agent.SetDestination(destino);
    }
    
    // Mètode que IAEnemic utilitza per perseguir al jugador
    public void Perseguir(Transform objectiu, float velocitat)
    {
        if (agent == null || objectiu == null) return;
        
        estatActual = AIState.PERSEGUIR;
        agent.isStopped = false;
        agent.speed = velocitat;
        agent.SetDestination(objectiu.position);
    }
    
    private void Update()
    {
        // Si l'enemic no està viu, aturem el moviment
        if (!enemic.EsViu())
        {
            agent.isStopped = true;
            return;
        }
        
        // Actualitzem el comptador d'atacs
        if (comptadorAtacs > 0)
        {
            comptadorAtacs -= Time.deltaTime;
        }
        
        // Si no hi ha jugador, només patrullem
        if (jugador == null)
        {
            if (estatActual == AIState.PATRULLA)
            {
                ActualitzarPatrulla();
            }
            return;
        }
        
        // Calculem la distància al jugador
        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        
        // Màquina d'estats
        switch (estatActual)
        {
            case AIState.PATRULLA:
                ActualitzarPatrulla();
                
                // Si detectem el jugador, començem a perseguir-lo
                if (distanciaJugador <= rangPerseguir)
                {
                    estatActual = AIState.PERSEGUIR;
                    agent.speed = velocitatPersecucio;
                }
                break;
                
            case AIState.PERSEGUIR:
                // Perseguim al jugador
                agent.SetDestination(jugador.position);
                
                // Si estem prou a prop, ataquem
                if (distanciaJugador <= rangAtacar && comptadorAtacs <= 0)
                {
                    // Aturem temporalment
                    agent.isStopped = true;
                    
                    // Activem l'atac
                    enemic.Atacar();
                    
                    // Reiniciem el comptador
                    comptadorAtacs = tempsEntreAtacs;
                    
                    // Reprenem el moviment després d'un moment
                    StartCoroutine(ReanudarMoviment(0.5f));
                }
                
                // Si el jugador s'allunya massa, passem a sospita
                if (distanciaJugador > rangPerseguir)
                {
                    estatActual = AIState.SOSPITA;
                    agent.speed = velocitatNormal;
                    ultimaPosicioVista = jugador.position;
                    tempsUltimaVegadaVist = tempsSospita;
                }
                break;
                
            case AIState.SOSPITA:
                // Anem a l'última posició coneguda
                if (agent.remainingDistance <= 0.5f || agent.destination == Vector3.zero)
                {
                    agent.SetDestination(ultimaPosicioVista);
                }
                
                // Reduïm el temps de sospita
                tempsUltimaVegadaVist -= Time.deltaTime;
                
                // Si veiem el jugador de nou, tornem a perseguir
                if (distanciaJugador <= rangPerseguir)
                {
                    estatActual = AIState.PERSEGUIR;
                    agent.speed = velocitatPersecucio;
                }
                // Si s'esgota el temps, tornem a patrullar
                else if (tempsUltimaVegadaVist <= 0)
                {
                    estatActual = AIState.PATRULLA;
                }
                break;
        }
    }
    
    private void ActualitzarPatrulla()
    {
        // Si no hi ha punts o estem esperant, no fem res
        if (puntsPatrulla == null || puntsPatrulla.Length == 0 || esperantEnPunt)
            return;
        
        // Si hem arribat al punt actual, avancem al següent
        if (agent.remainingDistance <= 0.5f)
        {
            StartCoroutine(EsperarEnPunt());
        }
    }
    
    private IEnumerator EsperarEnPunt()
    {
        esperantEnPunt = true;
        
        // Aturem l'agent
        agent.isStopped = true;
        
        // Esperem un moment
        yield return new WaitForSeconds(tempsEsperaPatrulla);
        
        // Avancem al següent punt
        puntActual = (puntActual + 1) % puntsPatrulla.Length;
        agent.SetDestination(puntsPatrulla[puntActual].position);
        
        // Reactivem l'agent
        agent.isStopped = false;
        
        esperantEnPunt = false;
    }
    
    private IEnumerator ReanudarMoviment(float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.isStopped = false;
    }
}