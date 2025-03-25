using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemic))]
public class IAEnemic : MonoBehaviour
{
    private Enemic enemic;
    private MovimentEnemic moviment;
    private AtacEnemic atac;
    private Transform jugador;
    
    [Header("Configuració IA")]
    [SerializeField] private float rangDeteccio = 10f;
    [SerializeField] private float rangAtac = 2f;
    [SerializeField] private float tempsEntreAtacs = 2f;
    [SerializeField] private float tempsMaximPersecucio = 5f;
    private float comptadorAtacs = 0f;
    private float comptadorPersecucio = 0f;
    
    private bool perseguintJugador = false;
    
    private void Awake()
    {
        enemic = GetComponent<Enemic>();
    }
    
    public void Inicialitzar()
    {
        moviment = GetComponent<MovimentEnemic>();
        atac = GetComponent<AtacEnemic>();
        jugador = enemic.Jugador;
        
        // Iniciar patrulla automáticamente
        if (moviment != null)
        {
            moviment.IniciarPatrulla();
        }
    }
    
    public void ActualitzarIA()
    {
        if (comptadorAtacs > 0)
            comptadorAtacs -= Time.deltaTime;
            
        // Si no hay jugador, solo patrullar
        if (jugador == null)
            return;
            
        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);
        
        // Comprobar si el jugador está dentro del rango de detección
        if (distanciaJugador <= rangDeteccio)
        {
            // Si no estábamos persiguiendo al jugador, empezar persecución
            if (!perseguintJugador)
            {
                perseguintJugador = true;
                comptadorPersecucio = tempsMaximPersecucio;
            }
            
            // Perseguir al jugador
            moviment.Perseguir(jugador, moviment.VelocitatPersecucio);
            
            // Si está en rango de ataque, atacar
            if (distanciaJugador <= rangAtac && comptadorAtacs <= 0)
            {
                enemic.Atacar();
                comptadorAtacs = tempsEntreAtacs;
            }
        }
        // Si estábamos persiguiendo pero el jugador se alejó
        else if (perseguintJugador)
        {
            // Decrementar contador de persecución
            comptadorPersecucio -= Time.deltaTime;
            
            // Si el tiempo de persecución se agotó, volver a patrullar
            if (comptadorPersecucio <= 0)
            {
                perseguintJugador = false;
                moviment.ReprendrePatrulla();
            }
            // Si no, seguir persiguiendo hasta la última posición conocida
            else
            {
                moviment.AnarA(jugador.position);
            }
        }
    }
    
    // Método para visualizar los rangos de detección y ataque
    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangDeteccio);
        
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangAtac);
    }
}
