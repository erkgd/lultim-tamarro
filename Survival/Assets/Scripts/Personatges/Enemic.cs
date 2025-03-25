using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemic : Personatge
{
    [Header("Referències")]
    private NavMeshAgent agent;
    private Transform jugador;

    [Header("Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;
    [SerializeField] private float tempsEsperaPostAtac = 1.5f;
    private float comptadorAtacs = 0f;

    [Header("Patrulla")]
    [SerializeField] private Transform puntsMoviment;
    [SerializeField] private float tempsEsperaPatrulla = 1f;
    private int puntActual;
    private Transform[] puntsPropies;
    private bool esperantEnPunt = false;

    [Header("Persecució")]
    [SerializeField] private float rangPerseguir = 10f;
    [SerializeField] private float velocitatNormal = 3.5f;
    [SerializeField] private float velocitatPersecucio = 20.0f;

    [Header("Sospita")]
    [SerializeField] private float tempsSospita = 3f;
    private float tempsUltimaVegadaVist;
    private Vector3 ultimaPosicioVista;

    public enum AIState
    {
        PATRULLA,
        PERSEGUIR,
        SOSPITA
    }
    public AIState estatActual;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Start()
    {
        base.Start();
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;

        // Inicialitza els punts de patrulla
        // Si no s'ha assignat un contenidor de punts, utilitzem els fills directes
        if (puntsMoviment == null)
        {
            // Comptem quants fills té aquest GameObject
            int nombreFills = transform.childCount;
            
            // Si té fills, els utilitzarem com a punts de patrulla
            if (nombreFills > 0)
            {
                // Creem l'array de punts
                puntsPropies = new Transform[nombreFills];
                
                // Omplim l'array amb els fills
                for (int i = 0; i < nombreFills; i++)
                {
                    puntsPropies[i] = transform.GetChild(i);
                }
                
                // Si tenim punts, establim el primer destí
                if (puntsPropies.Length > 0)
                {
                    agent.SetDestination(puntsPropies[0].position);
                    Debug.Log($"Iniciant patrulla amb {puntsPropies.Length} punts propis");
                }
            }
        }
        else if (puntsMoviment.childCount > 0)
        {
            // Si s'ha assignat un contenidor, utilitzem els seus fills
            agent.SetDestination(puntsMoviment.GetChild(0).position);
            Debug.Log($"Iniciant patrulla amb {puntsMoviment.childCount} punts del contenidor");
        }

        // Inicialitza la IA
        estatActual = AIState.PATRULLA;
        agent.speed = velocitatNormal;
        tempsUltimaVegadaVist = tempsSospita;
    }

    void Update()
    {
        if (!EsViu())
        {
            agent.isStopped = true;
            return;
        }

        if (comptadorAtacs > 0)
            comptadorAtacs -= Time.deltaTime;

        float distanciaJugador = jugador != null ? Vector3.Distance(jugador.position, transform.position) : 0f;

        switch (estatActual)
        {
            case AIState.PATRULLA:
                MovimentPatrulla();
                if (jugador != null && distanciaJugador <= rangPerseguir)
                {
                    estatActual = AIState.PERSEGUIR;
                    agent.speed = velocitatPersecucio;
                }
                break;

            case AIState.PERSEGUIR:
                if (jugador != null)
                    agent.SetDestination(jugador.position);

                if (distanciaJugador <= rangAtacar && comptadorAtacs <= 0)
                {
                    Atacar();
                    comptadorAtacs = tempsEntreAtacs;
                }

                if (distanciaJugador > rangPerseguir)
                {
                    ultimaPosicioVista = jugador.position;
                    estatActual = AIState.SOSPITA;
                    tempsUltimaVegadaVist = tempsSospita;
                    agent.speed = velocitatNormal;
                }
                break;

            case AIState.SOSPITA:
                if (agent.remainingDistance <= 0.5f || agent.destination == Vector3.zero)
                {
                    agent.SetDestination(ultimaPosicioVista);
                }

                tempsUltimaVegadaVist -= Time.deltaTime;

                if (jugador != null && distanciaJugador <= rangPerseguir)
                {
                    estatActual = AIState.PERSEGUIR;
                    agent.speed = velocitatPersecucio;
                }
                else if (tempsUltimaVegadaVist <= 0)
                {
                    estatActual = AIState.PATRULLA;
                }
                break;
        }
    }

    protected override IEnumerator Morir()
    {
        animator.SetBool("senseVida", true);
        agent.isStopped = true;
        enabled = false;

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    protected override IEnumerator ExecutarAtac()
    {
        atacant = true;
        agent.isStopped = true;
        animator.SetTrigger("Atacar");

        if (jugador != null)
        {
            IVida vidaJugador = jugador.GetComponent<IVida>();
            if (vidaJugador != null)
            {
                vidaJugador.DecrementarVida(dany, gameObject.name);

                Jugador jugadorScript = jugador.GetComponent<Jugador>();
                if (jugadorScript != null)
                    jugadorScript.RecibirKnockback((jugador.position - transform.position).normalized, forcaKnockback);
            }
        }

        yield return new WaitForSeconds(0.5f);
        agent.isStopped = false;
        atacant = false;
    }

    private void MovimentPatrulla()
    {
        // Si ja estem esperant en un punt, no fem res més
        if (esperantEnPunt)
            return;
        
        // Si estem utilitzant punts propis (fills directes)
        if (puntsPropies != null && puntsPropies.Length > 0)
        {
            // Si hem arribat al destí actual
            if (agent.remainingDistance <= 0.5f)
            {
                StartCoroutine(EsperarEnPunt());
            }
        }
        // Si estem utilitzant un contenidor de punts
        else if (puntsMoviment != null && puntsMoviment.childCount > 0)
        {
            // Si hem arribat al destí actual
            if (agent.remainingDistance <= 0.5f)
            {
                StartCoroutine(EsperarEnPunt());
            }
        }
    }

    private IEnumerator EsperarEnPunt()
    {
        esperantEnPunt = true;
        
        // Aturem l'agent temporalment
        agent.isStopped = true;
        
        // Esperem el temps configurat
        yield return new WaitForSeconds(tempsEsperaPatrulla);
        
        // Avancem al següent punt
        puntActual++;
        
        // Si estem utilitzant punts propis (fills directes)
        if (puntsPropies != null && puntsPropies.Length > 0)
        {
            // Tornem al primer punt si hem arribat al final
            if (puntActual >= puntsPropies.Length)
                puntActual = 0;
                
            // Establim el nou destí
            agent.SetDestination(puntsPropies[puntActual].position);
            Debug.Log($"Avançant al punt propi {puntActual}");
        }
        // Si estem utilitzant un contenidor de punts
        else if (puntsMoviment != null && puntsMoviment.childCount > 0)
        {
            // Tornem al primer punt si hem arribat al final
            if (puntActual >= puntsMoviment.childCount)
                puntActual = 0;
                
            // Establim el nou destí
            agent.SetDestination(puntsMoviment.GetChild(puntActual).position);
            Debug.Log($"Avançant al punt del contenidor {puntActual}");
        }
        
        // Reactivem l'agent
        agent.isStopped = false;
        esperantEnPunt = false;
    }
}