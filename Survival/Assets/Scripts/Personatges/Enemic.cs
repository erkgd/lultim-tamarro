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
    private Animator animator;
    private SistemaVidaEnemic sistemaVidaEnemic;
    
    // Variables para implementar propiedades abstractas
    [SerializeField] protected int vidaMaxima = 30;
    [SerializeField] protected int vidaActual = 30;
    [SerializeField] protected int dany = 10;
    [SerializeField] protected float forcaKnockback = 3f;
    protected bool atacant = false;
    
    // Componentes modularizados
    private AtacEnemic atacEnemic;
    private MovimentEnemic movimentEnemic;
    private IAEnemic iaEnemic;

    [Header("Configuració Atac")]
    [SerializeField] private float duracioAnimacioAtac = 0.5f;
    [SerializeField] private float tempsPerDesapareixer = 2f;

    [Header("Configuració Moviment")]
    [SerializeField] private string nomCarpetaPunts = "Moviment";
    [SerializeField] private float velocitatNormal = 3.5f;
    [SerializeField] private float velocitatPersecucio = 5.5f;
    [SerializeField] private float tempsEsperaPatrulla = 1f;
    [SerializeField] private float rangPerseguir = 10f;
    [SerializeField] private float tempsSospita = 3f;
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 2.0f;

    [Header("Configuració IA")]
    [SerializeField] private float rangDeteccio = 10f;
    [SerializeField] private float tempsMaximPersecucio = 5f;

    // Implementación de propiedades abstractas
    public override int VidaActual => vidaActual;
    public override int VidaMaxima => vidaMaxima;
    public override int Dany => dany;
    public override float ForcaKnockback => forcaKnockback;

    public NavMeshAgent Agent => agent;
    public Transform Jugador => jugador;
    public Animator AnimatorEnemic => animator;

    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Configurar NavMeshAgent para mejor desempeño
        agent.acceleration = 12f; // Acelera más rápido
        agent.angularSpeed = 180f; // Gira más rápido
        agent.autoBraking = false; // No frena al llegar al destino
        
        // Inicializar componentes modulares
        atacEnemic = gameObject.AddComponent<AtacEnemic>();
        movimentEnemic = gameObject.AddComponent<MovimentEnemic>();
        iaEnemic = gameObject.AddComponent<IAEnemic>();
        sistemaVidaEnemic = gameObject.AddComponent<SistemaVidaEnemic>();
        
        // Configurar los componentes con los valores serializados
        ConfigurarComponents();
    }

    private void ConfigurarComponents()
    {
        // Configurar AtacEnemic
        atacEnemic.ConfigurarAtac(duracioAnimacioAtac, tempsPerDesapareixer);

        // Configurar MovimentEnemic
        movimentEnemic.ConfigurarMoviment(
            nomCarpetaPunts,
            velocitatNormal,
            velocitatPersecucio,
            tempsEsperaPatrulla,
            rangPerseguir,
            tempsSospita,
            rangAtacar,
            tempsEntreAtacs
        );

        // Configurar IAEnemic
        iaEnemic.ConfigurarIA(rangDeteccio, rangAtacar, tempsEntreAtacs, tempsMaximPersecucio);
    }

    protected override void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
            
        // Iniciar la IA
        iaEnemic.Inicialitzar();
    }

    void Update()
    {
        /*if (!EsViu())
        {
            agent.isStopped = true;
            return;
        }*/

        // Actualizar la IA
        iaEnemic.ActualitzarIA();
    }

    public override bool EstaAtacant()
    {
        return atacant;
    }


    public override void DecrementarVida(int quantitat)
    {
        // Evitamos modificar la vida del enemigo si ya está muriendo
        if (animator.GetBool("senseVida"))
            return;
    }

    // Implementación de las funciones abstractas de evento
    protected override void NotificarCanviVida()
    {
        InvokeQuanCanviVida();
    }

    protected override void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }

    protected override void InvokeQuanCanviVida()
    {
        QuanCanviVida?.Invoke();
    }

    // Propiedades públicas para acceder a miembros protegidos
    public bool Atacant { get => atacant; set => atacant = value; }
    
    // Método para llamar al método protegido
    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }

    protected override IEnumerator ExecutarAtac()
    {
        return atacEnemic.ExecutarAtac();
    }

    protected override IEnumerator Morir() 
    {
        // Evitamos múltiples llamadas a esta corrutina
        if (animator.GetBool("senseVida"))
            yield break;
            
        Debug.Log($"Ejecutando muerte de {gameObject.name}");
        
        // Activamos la animación de muerte
        animator.SetBool("senseVida", true);
        
        // Detenemos al enemigo y desactivamos su script principal
        agent.isStopped = true;
        enemic.enabled = false;
        
        // Esperamos a que termine la animación
        yield return new WaitForSeconds(tempsPerDesapareixer);
        
        // Nos aseguramos de desactivar el objeto (solución a que no desaparezca)
        gameObject.SetActive(false);
    }

    public override void Atacar()
    {
        sistemaVida.IniciarAtac();
    }
    
    
}