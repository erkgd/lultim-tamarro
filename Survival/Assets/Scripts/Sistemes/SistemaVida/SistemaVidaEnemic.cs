using System;
using System.Collections;
using UnityEngine;

public class SistemaVidaEnemic : SistemaVida
{
    [SerializeField] private float vidaMaxima = 2f;
    [SerializeField] private float vidaActual = 2f;
    
    // Referencias
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    
    // Eventos para comunicación
    public event Action OnIniciarAtac;
    public event Action QuanMoriEnemic;
    // Acceso a propiedades
    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;
    
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
    
    public void DecrementarVida(float quantitat, string font = "")
    {
        if (quantitat <= 0 || !EsViu() || animator.GetBool("senseVida")) 
            return;
        
        vidaActual = Mathf.Max(vidaActual - quantitat, 0f);
        
        if (!string.IsNullOrEmpty(font))
            Debug.Log($"Enemic {name} rep {quantitat} de dany de {font}. Vida restant: {vidaActual}");
        
        if (vidaActual > 0 && animator != null)
            animator.SetTrigger("RepMal");
        
        NotificarCanviVida();
        
        if (vidaActual <= 0)
        {
            vidaActual = 0f;
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
        // Notificar a los suscriptores de la muerte del enemigo
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
        
        // Notificar muerte al sistema de contadores si existe
        if (SistemaCounter.Instance != null)
        {
            // Obtener el tipo de enemigo - se determina en SistemaCounter
            SistemaCounter.Instance.RegistrarEnemigoEliminado(0); // El tipo se detecta automáticamente en SistemaCounter
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

