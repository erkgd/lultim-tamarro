using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaEnemic : SistemaVida
{
    [SerializeField] private int vidaMaxima = 30;
    [SerializeField] private int vidaActual = 30;
    
    // Referencias
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    private Enemic enemic;
    
    // Acceso a propiedades
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    
    public override void Awake()
    {
        base.Awake();
        
        // Obtener referencias
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        enemic = GetComponent<Enemic>();
    }
    
    public bool EsViu()
    {
        return vidaActual > 0;
    }
    
    public void DecrementarVida(int quantitat, string font = "")
    {
        // Evitamos modificar la vida si ya está muriendo
        if (quantitat <= 0 || !EsViu() || animator.GetBool("senseVida")) 
            return;
        
        vidaActual = Mathf.Max(vidaActual - quantitat, 0);
        
        // Registrar fuente de daño
        if (!string.IsNullOrEmpty(font))
            Debug.Log($"Enemic {name} rep {quantitat} de dany de {font}. Vida restant: {vidaActual}");
        
        // Activamos la animación de recibir daño
        if (vidaActual > 0 && animator != null)
            animator.SetTrigger("TrRepMal");
        
        // Notificamos el cambio de vida
        NotificarCanviVida();
        
        // Si la vida llega a 0, iniciamos la muerte
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            StartCoroutine(Morir());
        }
    }
    
    public override void SubscribeToQuanCanviVida(Action handler)
    {
        QuanCanviVida += handler;
    }
    
    public void IniciarAtac()
    {
        if (enemic != null)
        {
            StartCoroutine(enemic.ExecutarAtacPublic());
        }
    }
}