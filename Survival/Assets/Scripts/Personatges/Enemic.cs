using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SistemaVidaEnemic))]
public class Enemic : Personatge
{
    [Header("Referències")]
    private NavMeshAgent agent;
    private Transform jugador;
    private Animator animator;
    private SistemaVidaEnemic sistemaVida;
    
    // Variables para implementar propiedades abstractas
    [SerializeField] private int dany = 10;
    [SerializeField] private float forcaKnockback = 3f;
    private bool atacant = false;
    
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

    // Implementación de propiedades abstractas a través del sistema de vida
    public override int VidaActual => sistemaVida.VidaActual;
    public override int VidaMaxima => sistemaVida.VidaMaxima;
    public override int Dany => dany;
    public override float ForcaKnockback => forcaKnockback;

    public NavMeshAgent Agent => agent;
    public Transform Jugador => jugador;
    public Animator AnimatorEnemic => animator;
    public bool Atacant { get => atacant; set => atacant = value; }

    // Propiedades adicionales para otras clases
    public int DanyAtac => dany;
    
    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        sistemaVida = GetComponent<SistemaVidaEnemic>();
        
        // Configurar NavMeshAgent para mejor desempeño
        agent.acceleration = 12f;
        agent.angularSpeed = 180f;
        agent.autoBraking = false;
        
        // Inicializar componentes modulares
        atacEnemic = gameObject.AddComponent<AtacEnemic>();
        movimentEnemic = gameObject.AddComponent<MovimentEnemic>();
        iaEnemic = gameObject.AddComponent<IAEnemic>();
        
        // Configurar los componentes con los valores serializados
        ConfigurarComponents();
    }

    private void ConfigurarComponents()
    {
        // Configuraciones...
        atacEnemic.ConfigurarAtac(duracioAnimacioAtac, tempsPerDesapareixer);
        movimentEnemic.ConfigurarMoviment(nomCarpetaPunts, velocitatNormal, velocitatPersecucio, 
            tempsEsperaPatrulla, rangPerseguir, tempsSospita, rangAtacar, tempsEntreAtacs);
        iaEnemic.ConfigurarIA(rangDeteccio, rangAtacar, tempsEntreAtacs, tempsMaximPersecucio);
    }

    protected override void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
            
        // Iniciar la IA
        iaEnemic.Inicialitzar();
        
        // Suscripción al evento de cambio de vida
        sistemaVida.SubscribeToQuanCanviVida(() => {
            // No invocar el evento directamente
            // QuanCanviVida?.Invoke(); <- Error, los eventos solo pueden usarse con += o -=
            NotificarCambiVida();
        });
    }

    private void Update()
    {
        if (!sistemaVida.EsViu())
        {
            agent.isStopped = true;
            return;
        }

        // Actualizar la IA
        iaEnemic.ActualitzarIA();
    }

    // Métodos que delegan al sistema de vida
    public override void DecrementarVida(int quantitat, string font = "")
    {
        sistemaVida.DecrementarVida(quantitat, font);
    }
    
    // Método público para comprobar si está vivo (necesario para MovimentEnemic)
    public bool EsViu()
    {
        return sistemaVida.EsViu();
    }
    
    // Método para llamar al método protegido
    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }
    
    public override IEnumerator ExecutarAtac()
    {
        return atacEnemic.ExecutarAtac();
    }

    public override IEnumerator Morir() 
    {
        // Delegamos al sistema de vida
        return sistemaVida.Morir();
    }

    public override void Atacar()
    {
        sistemaVida.IniciarAtac();
    }

    public override bool EstaAtacant()
    {
        return atacant;
    }

    // Método para notificar a los suscriptores
    protected void NotificarCambiVida()
    {
        // Usar el método protegido de la clase base en lugar de reflexión
        InvocarQuanCanviVida();
    }
}