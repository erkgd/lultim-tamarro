using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    PATRULLA,
    PERSEGUIR,
    SOSPITA
}

public class MovimentEnemics : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private Transform puntsMoviment;
    private int puntActual;

    [SerializeField] private float tempsEsperaPatrulla = 1f;
    private float contadorEspera;

    [Header("Components")]
    NavMeshAgent enemic;
    private SistemaAtac sistemaAtac;
    private SistemaVida sistemaVida; // Referència al sistema de vida

    [Header("Estat IA")]
    public AIState currentState;

    [Header("Perseguir")]
    [SerializeField] private float rangPerseguir = 10f;
    //[SerializeField] private float rangPerdrePista = 15f;       // ? No està en us
    [SerializeField] private float velocitatNormal = 3.5f;
    [SerializeField] private float velocitatPersecucio = 20.0f;

    [Header("Sospita")]
    [SerializeField] private float tempsSospita = 3f;
    private float tempsUltimaVegadaVist;
    private Vector3 ultimaPosicioVista;

    [Header("Atacar")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f; // Cooldown de l'atac, temps de espera entre atacs
    private float comptadorAtacs = 0f;  // Cooldown de l'atac, itinerari de temps entre atacs
    private Animator animador;

    private GameObject player;

    void Start()
    {
        enemic = GetComponent<NavMeshAgent>();
        sistemaAtac = GetComponent<SistemaAtac>();
        sistemaVida = GetComponent<SistemaVida>(); // Inicialitza el sistema de vida
        player = GameObject.FindGameObjectWithTag("Player");
        animador = GetComponent<Animator>();
        comptadorAtacs = 0f;

        // Patrulla inicial
        if (puntsMoviment != null && puntsMoviment.childCount > 0)
        {
            enemic.SetDestination(puntsMoviment.GetChild(puntActual).position);
        }

        contadorEspera = tempsEsperaPatrulla;
        tempsUltimaVegadaVist = tempsSospita;
        currentState = AIState.PATRULLA;
        Debug.Log($"{gameObject.name} is patrullando the city");

        enemic.speed = velocitatNormal;
    }

    void Update()
    {
        // Verifica si el personatge està viu
        if (sistemaVida == null || !sistemaVida.EsViu())
        {
            enemic.isStopped = true;
            return;
        } else
            enemic.isStopped = false;

        //Anem detectant la distància del jugador
        float distanciaJugador = Vector3.Distance(player.transform.position, transform.position);

        // Update attack cooldown timer
        if (comptadorAtacs > 0)
        {
            comptadorAtacs -= Time.deltaTime;
        }

        switch (currentState)
        {
            case AIState.PATRULLA:
                MovimentPatrulla();

                if (distanciaJugador <= rangPerseguir)
                {
                    currentState = AIState.PERSEGUIR;
                    Debug.Log($"{gameObject.name} says '' why are you running! ''");
                    enemic.speed = velocitatPersecucio;
                }
                break;

            case AIState.PERSEGUIR:
                enemic.SetDestination(player.transform.position);

                // Mirem si pot atacar
                if ((distanciaJugador <= rangAtacar) && (comptadorAtacs <= 0))
                {
                    // Parem un moment el moviment del enemic
                    enemic.isStopped = true;

                    // Fem el atac amb un trigger.
                    if (animador != null)
                    {
                        // animador.SetTrigger("Atacar");
                        // Aquí es cridaria a la funció de atacar del jugador
                        sistemaAtac.AplicarDany(1, gameObject.name);
                    }

                    // Aqui resetejem el comptador de atacs
                    comptadorAtacs = tempsEntreAtacs;

                    // Finalment reanudem el moviment del enemic
                    StartCoroutine(ReanudarMoviment(0.1f));
                }

                // Si el jugador està fora del rang de perseguir, tornem a la sospita
                if (distanciaJugador > rangPerseguir)
                {
                    ultimaPosicioVista = player.transform.position;
                    currentState = AIState.SOSPITA;
                    Debug.Log("Sospita");
                    tempsUltimaVegadaVist = tempsSospita;
                    enemic.speed = velocitatNormal;
                }
                break;

            case AIState.SOSPITA:

                Debug.Log("Sospita");

                if (enemic.remainingDistance <= 0.5f || enemic.destination == Vector3.zero)
                {
                    enemic.SetDestination(ultimaPosicioVista);
                }

                tempsUltimaVegadaVist -= Time.deltaTime;

                if (distanciaJugador <= rangPerseguir)
                {
                    currentState = AIState.PERSEGUIR;
                    Debug.Log($"{gameObject.name} says '' why are you running! ''");
                    enemic.speed = velocitatPersecucio;
                }
                else if (tempsUltimaVegadaVist <= 0)
                {
                    currentState = AIState.PATRULLA;
                    Debug.Log($"{gameObject.name} is patrullando the city");
                }
                break;
        }
    }

    private IEnumerator ReanudarMoviment(float delay)
    {
        yield return new WaitForSeconds(delay);
        enemic.isStopped = false;
    }

    private void MovimentPatrulla()
    {
        if (puntsMoviment == null || puntsMoviment.childCount == 0)
            return;

        if (enemic.remainingDistance <= 0.2f)
        {
            puntActual++;
            if (puntActual >= puntsMoviment.childCount)
            {
                puntActual = 0;
            }

            enemic.SetDestination(puntsMoviment.GetChild(puntActual).position);
        }
    }
}