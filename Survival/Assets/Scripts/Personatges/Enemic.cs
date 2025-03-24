using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemic : MonoBehaviour, IVida, IAtacant
{
    [Header("Referències")]
    private Animator animator;
    private NavMeshAgent agent;
    private Transform jugador;
    
    [Header("Vida")]
    [SerializeField] public int vidaActual;
    [SerializeField] private int vidaMaxima = 3;

    [Header("Atac")]
    [SerializeField] private int dany = 1;
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;
    [SerializeField] private float forcaKnockback = 7f;
    [SerializeField] private float tempsEsperaPostAtac = 1.5f;
    private float comptadorAtacs = 0f;
    private bool atacant = false;

    [Header("Patrulla")]
    [SerializeField] private Transform puntsMoviment; // Si s'assigna, usa aquest. Si no, utilitza els fills directes
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

    // Estat actual de l'IA
    public enum AIState
    {
        PATRULLA,
        PERSEGUIR,
        SOSPITA
    }
    public AIState estatActual;

    // Event vida
    public event Action QuanCanviVida;

    // Propietats interfície IVida
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;

    // Propietats interfície IAtacant
    public int Dany => dany;
    public bool EstaAtacant() => atacant;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Inicialitza la vida
        vidaActual = vidaMaxima;
        
        // Cerca el jugador
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
        // Si l'enemic està mort, no fer res
        if (!EsViu())
        {
            agent.isStopped = true;
            return;
        }

        // Actualitza el comptador d'atacs
        if (comptadorAtacs > 0)
            comptadorAtacs -= Time.deltaTime;

        // Obté la distància al jugador
        float distanciaJugador = 0f;
        if (jugador != null)
            distanciaJugador = Vector3.Distance(jugador.position, transform.position);

        // Màquina d'estats de l'IA
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

                // Intent d'atac
                if (distanciaJugador <= rangAtacar && comptadorAtacs <= 0)
                {
                    Atacar();
                    comptadorAtacs = tempsEntreAtacs;
                }

                // Canvi a estat de sospita si el jugador s'allunya
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

    #region IVida
    public bool EsViu()
    {
        return vidaActual > 0;
    }

    public void IncrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual += quantitat;
        if (vidaActual > vidaMaxima)
            vidaActual = vidaMaxima;

        // Notifiquem el canvi de vida
        QuanCanviVida?.Invoke();
    }

    public void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0) return;

        vidaActual -= quantitat;

        // Activem l'animació de rebre mal
        if (animator != null)
            animator.SetTrigger("TrRepMal");

        // Si la vida arriba a 0 o menys, iniciem el procés de mort
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }

        // Notifiquem el canvi de vida
        QuanCanviVida?.Invoke();
    }

    private IEnumerator Morir()
    {
        // Activem l'animació de mort
        if (animator != null)
            animator.SetBool("senseVida", true);

        // Desactivem l'agent
        agent.isStopped = true;
        enabled = false;

        // Esperem a que l'animació acabi
        yield return new WaitForSeconds(2f);

        // Desactivem el GameObject
        gameObject.SetActive(false);
    }
    #endregion

    #region IAtacant
    public void Atacar()
    {
        StartCoroutine(ExecutarAtac());
    }

    private IEnumerator ExecutarAtac()
    {
        atacant = true;
        
        // Aturem l'agent temporalment
        agent.isStopped = true;
        
        // Activem l'animació d'atac
        animator.SetTrigger("Atacar");
        
        // Apliquem dany al jugador si està a rang
        if (jugador != null)
        {
            // Calculamos la dirección del knockback (desde el enemigo hacia el jugador)
            Vector3 direccioKnockback = (jugador.position - transform.position).normalized;
            direccioKnockback.y = 0; // Mantenemos el knockback horizontal
            
            IVida vidaJugador = jugador.GetComponent<IVida>();
            if (vidaJugador != null)
            {
                vidaJugador.DecrementarVida(dany, gameObject.name);
                
                // Aplicamos el knockback al jugador
                Jugador jugadorScript = jugador.GetComponent<Jugador>();
                if (jugadorScript != null)
                {
                    jugadorScript.RecibirKnockback(direccioKnockback, forcaKnockback);
                }
            }
        }

        // Esperem un moment per a l'animació d'atac
        yield return new WaitForSeconds(0.5f);
        
        // Pausa adicional después del ataque - el enemigo se queda quieto
        animator.SetFloat("VelocitatMoviment", 0f); // Si tienes este parámetro en tu animator
        
        // Esperamos el tiempo configurado
        yield return new WaitForSeconds(tempsEsperaPostAtac);
        
        // Reactivem el moviment de l'agent
        agent.isStopped = false;
        atacant = false;
        
        // Opcional: cambiar velocidad según estado actual
        if (estatActual == AIState.PERSEGUIR)
        {
            agent.speed = velocitatPersecucio;
        }
        else
        {
            agent.speed = velocitatNormal;
        }
    }
    #endregion

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