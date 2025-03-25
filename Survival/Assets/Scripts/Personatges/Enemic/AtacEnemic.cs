using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemic))]
public class AtacEnemic : MonoBehaviour
{
    private Enemic enemic;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform jugador;
    
    [Header("Configuració Atac")]
    //[SerializeField] private float tempsEsperaPostAtac = 1.5f;
    [SerializeField] private float duracioAnimacioAtac = 0.5f;
    [SerializeField] private float tempsPerDesapareixer = 2f;
    
    private void Awake()
    {
        enemic = GetComponent<Enemic>();
    }
    
    private void Start()
    {
        animator = enemic.AnimatorEnemic;
        agent = enemic.Agent;
        jugador = enemic.Jugador;
    }
    
    public void IniciarAtac()
    {
        StartCoroutine(enemic.ExecutarAtacPublic());
    }
    
    public IEnumerator ExecutarAtac()
    {
        enemic.Atacant = true;
        agent.isStopped = true;
        animator.SetTrigger("Atacar");

        if (jugador != null)
        {
            IVida vidaJugador = jugador.GetComponent<IVida>();
            if (vidaJugador != null)
            {
                vidaJugador.DecrementarVida(enemic.DanyAtac, gameObject.name);

                Jugador jugadorScript = jugador.GetComponent<Jugador>();
                if (jugadorScript != null)
                    jugadorScript.RebreKnockback((jugador.position - transform.position).normalized, enemic.ForcaKnockback);
            }
        }

        yield return new WaitForSeconds(duracioAnimacioAtac);
        agent.isStopped = false;
        enemic.Atacant = false;
    }
    
    public IEnumerator ExecutarMort()
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
        Debug.Log($"Desactivando enemigo {gameObject.name} después de morir");
        gameObject.SetActive(false);
    }
}
