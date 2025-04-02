using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemic))]
public class AtacEnemic : MonoBehaviour
{
    private Enemic enemic;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform jugador;
    
    private float duracioAnimacioAtac;
    private float tempsPerDesapareixer;
    
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

    public void ConfigurarAtac(float duracioAnimacioAtac, float tempsPerDesapareixer)
    {
        this.duracioAnimacioAtac = duracioAnimacioAtac;
        this.tempsPerDesapareixer = tempsPerDesapareixer;
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
            Personatge personatgeJugador = jugador.GetComponent<Personatge>();
            if (personatgeJugador != null)
            {
                personatgeJugador.DecrementarVida(enemic.DanyAtac);

                Jugador jugadorScript = jugador.GetComponent<Jugador>();
                if (jugadorScript != null)
                    jugadorScript.RebreKnockback((jugador.position - transform.position).normalized, enemic.ForcaKnockback);
            }
        }

        yield return new WaitForSeconds(duracioAnimacioAtac);
        agent.isStopped = false;
        enemic.Atacant = false;
    }
    
}