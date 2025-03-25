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
    
    // Componentes modularizados
    private AtacEnemic atacEnemic;
    private MovimentEnemic movimentEnemic;
    private IAEnemic iaEnemic;

    public NavMeshAgent Agent => agent;
    public Transform Jugador => jugador;
    public Animator AnimatorEnemic => animator;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        
        // Configurar NavMeshAgent para mejor desempeño
        agent.acceleration = 12f; // Acelera más rápido
        agent.angularSpeed = 180f; // Gira más rápido
        agent.autoBraking = false; // No frena al llegar al destino
        
        // Inicializar componentes modulares
        atacEnemic = gameObject.AddComponent<AtacEnemic>();
        movimentEnemic = gameObject.AddComponent<MovimentEnemic>();
        iaEnemic = gameObject.AddComponent<IAEnemic>();
    }

    protected override void Start()
    {
        base.Start();
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
            
        // Iniciar la IA
        iaEnemic.Inicialitzar();
    }

    void Update()
    {
        if (!EsViu())
        {
            agent.isStopped = true;
            return;
        }

        // Actualizar la IA
        iaEnemic.ActualitzarIA();
    }

    // Corregir la firma del método Morir para que devuelva IEnumerator
    protected override IEnumerator Morir()
    {
        // Implementación directa como corrutina
        return atacEnemic.ExecutarMort();
    }

    // Propiedades públicas para acceder a miembros protegidos
    public bool Atacant { get => atacant; set => atacant = value; }
    public int DanyAtac => dany;  // Propiedad de solo lectura
    public float ForcaKnockback => forcaKnockback;  // Propiedad de solo lectura
    
    // Método público para llamar al método protegido
    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }

    protected override IEnumerator ExecutarAtac()
    {
        return atacEnemic.ExecutarAtac();
    }

    public override void Atacar()
    {
        atacEnemic.IniciarAtac();
    }

    public override void DecrementarVida(int quantitat, string font)
    {
        if (quantitat <= 0 || !EsViu()) return;

        // Evitamos modificar la vida del enemigo si ya está muriendo
        if (animator.GetBool("senseVida"))
            return;

        vidaActual -= quantitat;
        Debug.Log($"Enemic {name} rep {quantitat} de dany de {font}. Vida restant: {vidaActual}");

        // Activamos la animación de recibir daño si no estamos muriendo
        if (vidaActual > 0 && animator != null)
            animator.SetTrigger("TrRepMal");

        // Notificamos el cambio de vida
        NotificarCanviVida();

        // Si la vida llega a 0, iniciamos la muerte (ya no llamamos a base.DecrementarVida)
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }
    }
}