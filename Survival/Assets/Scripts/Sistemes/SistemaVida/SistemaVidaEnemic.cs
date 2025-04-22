using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaEnemic : SistemaVida
{
    [SerializeField] private int vidaMaxima = 2;
    [SerializeField] private int vidaActual = 2;
    
    // Referencias
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    
    // Eventos para comunicación
    public event Action OnIniciarAtac;
    public event Action QuanMoriEnemic;
    // Acceso a propiedades
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    
    public override void Awake()
    {
        base.Awake();
        
        // Obtener referencias
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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
            animator.SetTrigger("RepMal");
        
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
        // En lugar de llamar directamente a Enemic, notificamos a través del evento
        OnIniciarAtac?.Invoke();
    }
    
    public override IEnumerator Morir()
    {
        QuanMoriEnemic?.Invoke();

        // Configurar animación y estado
        if (animator != null)
        {
            animator.SetBool("senseVida", true);
        }
        
        // Desactivar NavMeshAgent con comprobación de seguridad
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        // Notificar muerte
        InvocarMuerte();
        
        // Reproducir animación de muerte y esperar
        yield return new WaitForSeconds(1f);
        
        // Aplicar efecto de reducción de tamaño
        yield return StartCoroutine(animacioDesapareixer());
        
        // Destruir el objeto
        Destroy(gameObject);
    }
    
    // Función dedicada al efecto de reducción de tamaño
    private IEnumerator animacioDesapareixer()
    {
        float duracionReduccion = 1.0f;
        float tiempoTranscurrido = 0f;
        Vector3 escalaOriginal = transform.localScale;
        Vector3 escalaFinal = Vector3.zero;
        
        while (tiempoTranscurrido < duracionReduccion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float factor = Mathf.Clamp01(tiempoTranscurrido / duracionReduccion);
            
            // Interpolar la escala entre la original y cero
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaFinal, factor);
            
            yield return null;
        }
    }
}

