using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Enemic))]
public class MovimentEnemic : MonoBehaviour
{
    private Enemic enemic;
    private NavMeshAgent agent;
    private Animator animator;
    
    [Header("Configuració Moviment")]
    [SerializeField] private float velocitatNormal = 3.5f;
    [SerializeField] private float velocitatPersecucio = 5.5f;
    [SerializeField] private float distanciaArribada = 0.5f;
    [SerializeField] private float distanciaArribadaMinima = 0.1f;
    
    [Header("Patrulla")]
    [SerializeField] private float tempsEsperaEntrePunts = 1f;
    [SerializeField] private bool activarPatrulla = true;
    [SerializeField] private bool dibuixarGizmos = true;
    [SerializeField] private string nomCarpetaPunts = "Moviment";
    
    private Transform carpetaPunts;
    private Transform[] puntsPatrulla;
    private int puntActual = 0;
    private bool esperantEnPunt = false;
    private bool enPatrulla = false;
    private bool patrollaConfigurada = false;
    
    public float VelocitatNormal => velocitatNormal;
    public float VelocitatPersecucio => velocitatPersecucio;
    
    private Vector3 destinoActual;
    private bool destinoAsignado = false;
    
    private void Awake()
    {
        enemic = GetComponent<Enemic>();
    }
    
    private void Start()
    {
        agent = enemic.Agent;
        animator = enemic.AnimatorEnemic;
        
        // Configurar el NavMeshAgent para que funcione mejor
        ConfigurarNavMeshAgent();
        
        // Configurar la patrulla si está activada
        if (activarPatrulla)
        {
            ConfigurarPuntsPatrulla();
            IniciarPatrulla();
        }
    }
    
    private void ConfigurarNavMeshAgent()
    {
        if (agent == null) return;
        
        agent.speed = velocitatNormal;
        agent.angularSpeed = 180f;
        agent.acceleration = 12f;
        agent.stoppingDistance = distanciaArribada * 0.8f;
        agent.autoBraking = false;
        agent.updateRotation = true;
        agent.updatePosition = true;
    }
    
    private void ConfigurarPuntsPatrulla()
    {
        // Buscar la carpeta de puntos en el mismo nivel (como hermana del GameObject)
        if (transform.parent != null)
        {
            carpetaPunts = transform.parent.Find(nomCarpetaPunts);
            
            if (carpetaPunts == null)
            {
                // Intentar buscar una carpeta que contenga el texto "movimen" en el mismo nivel
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    Transform hermano = transform.parent.GetChild(i);
                    if (hermano != transform && hermano.name.ToLower().Contains("movimen"))
                    {
                        carpetaPunts = hermano;
                        break;
                    }
                }
            }
        }
        
        if (carpetaPunts != null && carpetaPunts.childCount > 0)
        {
            // Obtener los puntos de patrulla desde la carpeta encontrada
            puntsPatrulla = new Transform[carpetaPunts.childCount];
            for (int i = 0; i < carpetaPunts.childCount; i++)
            {
                puntsPatrulla[i] = carpetaPunts.GetChild(i);
            }
            
            patrollaConfigurada = true;
            Debug.Log($"Enemic {gameObject.name}: Patrulla configurada amb {puntsPatrulla.Length} punts de la carpeta '{carpetaPunts.name}'");
        }
        else
        {
            Debug.LogWarning($"Enemic {gameObject.name}: No s'ha trobat la carpeta '{nomCarpetaPunts}' al mateix nivell o no té punts");
            patrollaConfigurada = false;
        }
    }
    
    public void IniciarPatrulla()
    {
        if (!patrollaConfigurada || puntsPatrulla == null || puntsPatrulla.Length == 0)
            return;
            
        puntActual = 0;
        enPatrulla = true;
        AnarAPuntPatrulla(puntActual);
        
        Debug.Log($"Enemic {gameObject.name}: Iniciant patrulla");
    }
    
    private void AnarAPuntPatrulla(int indexPunt)
    {
        if (puntsPatrulla == null || indexPunt < 0 || indexPunt >= puntsPatrulla.Length)
            return;
            
        AnarA(puntsPatrulla[indexPunt].position);
    }
    
    private void Update()
    {
        // Actualizar animaciones basadas en la velocidad
        if (animator != null && agent != null)
        {
            // Comprovar si els paràmetres existeixen abans d'intentar usar-los
            if (HasParameter("Velocitat", animator) && HasParameter("EnMoviment", animator))
            {
                // Velocidad normalizada: 0 cuando está parado, 1 cuando va a velocidad máxima
                float velocidadNormalizada = agent.velocity.magnitude / velocitatPersecucio;
                
                // Establecer parámetro de velocidad en el animator
                animator.SetFloat("Velocitat", velocidadNormalizada);
                
                // Establecer si está en movimiento o no
                animator.SetBool("EnMoviment", velocidadNormalizada > 0.1f);
            }
            else
            {
                // Utilitzar paràmetres alternatius que són comuns en molts animators
                if (HasParameter("Speed", animator))
                {
                    animator.SetFloat("Speed", agent.velocity.magnitude / velocitatPersecucio);
                }
                
                if (HasParameter("IsMoving", animator))
                {
                    animator.SetBool("IsMoving", agent.velocity.magnitude > 0.1f);
                }
                else if (HasParameter("Moving", animator))
                {
                    animator.SetBool("Moving", agent.velocity.magnitude > 0.1f);
                }
                else if (HasParameter("Walk", animator))
                {
                    animator.SetBool("Walk", agent.velocity.magnitude > 0.1f);
                }
            }
        }
        
        // Actualizar la patrulla si está activa
        if (enPatrulla && !esperantEnPunt)
        {
            ActualitzarPatrulla();
        }
    }
    
    private void ActualitzarPatrulla()
    {
        if (HaArribat())
        {
            StartCoroutine(EsperarEnPuntPatrulla());
        }
    }
    
    private IEnumerator EsperarEnPuntPatrulla()
    {
        esperantEnPunt = true;
        
        // Detener al enemigo
        Aturar();
        
        // Esperar un tiempo
        yield return new WaitForSeconds(tempsEsperaEntrePunts);
        
        // Avanzar al siguiente punto (en modo circular)
        puntActual = (puntActual + 1) % puntsPatrulla.Length;
        
        // Ir al siguiente punto
        AnarAPuntPatrulla(puntActual);
        
        esperantEnPunt = false;
    }
    
    // Método para interrumpir la patrulla (p.ej. cuando detecta al jugador)
    public void AturarPatrulla()
    {
        enPatrulla = false;
    }
    
    // Método para reanudar la patrulla
    public void ReprendrePatrulla()
    {
        if (!patrollaConfigurada)
            return;
            
        enPatrulla = true;
        
        // Si estamos en medio de una espera, no hacemos nada
        if (esperantEnPunt)
            return;
            
        // Si no, vamos al punto actual
        AnarAPuntPatrulla(puntActual);
    }
    
    public void AnarA(Vector3 destino)
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        
        // Guardar el destino actual
        destinoActual = destino;
        destinoAsignado = true;
        
        // Configurar la velocidad normal
        agent.speed = velocitatNormal;
        
        // Asegurar que el agente está activado
        agent.isStopped = false;
        
        // Establecer el destino
        agent.SetDestination(destino);
    }
    
    public void Perseguir(Transform objectiu, float velocitat)
    {
        if (agent == null || !agent.isActiveAndEnabled || objectiu == null) return;
        
        // Interrumpir la patrulla mientras persigue
        AturarPatrulla();
        
        // Configurar velocidad de persecución
        agent.speed = velocitat;
        
        // Asegurar que el agente está activado
        agent.isStopped = false;
        
        // Establecer el destino
        agent.SetDestination(objectiu.position);
        
        // Guardar destino actual
        destinoActual = objectiu.position;
        destinoAsignado = true;
    }
    
    public void Aturar()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        // Actualitzar animacions
        if (animator != null)
        {
            // Comprovar si els paràmetres existeixen abans d'actualitzar-los
            if (HasParameter("Velocitat", animator))
            {
                animator.SetFloat("Velocitat", 0);
            }
            else if (HasParameter("Speed", animator))
            {
                animator.SetFloat("Speed", 0);
            }
            
            if (HasParameter("EnMoviment", animator))
            {
                animator.SetBool("EnMoviment", false);
            }
            else if (HasParameter("IsMoving", animator))
            {
                animator.SetBool("IsMoving", false);
            }
            else if (HasParameter("Moving", animator))
            {
                animator.SetBool("Moving", false);
            }
            else if (HasParameter("Walk", animator))
            {
                animator.SetBool("Walk", false);
            }
        }
    }
    
    public void Continuar()
    {
        if (agent == null || !agent.isActiveAndEnabled) return;
        
        // Si estamos en patrulla, continuamos con ella
        if (enPatrulla)
        {
            AnarAPuntPatrulla(puntActual);
            return;
        }
        
        // Si no, si tenemos un destino guardado, volver a él
        if (destinoAsignado)
        {
            agent.isStopped = false;
            agent.SetDestination(destinoActual);
        }
    }
    
    public bool HaArribat()
    {
        if (agent == null || !agent.isActiveAndEnabled) return true;
        
        // Comprobar si hemos llegado al destino
        return !agent.pathPending && 
               agent.remainingDistance <= agent.stoppingDistance + distanciaArribadaMinima &&
               (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f);
    }
    
    // Método para debug visual
    private void OnDrawGizmosSelected()
    {
        if (!dibuixarGizmos) return;
        
        // Dibuixar el destí actual i el radi d'arribada si n'hi ha un assignat
        if (Application.isPlaying && destinoAsignado)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(destinoActual, 0.3f);
            Gizmos.DrawWireSphere(destinoActual, distanciaArribada);
        }
        
        // Buscar la carpeta de punts per dibuixar els gizmos fins i tot en mode editor
        Transform carpetaGizmos = null;
        
        if (transform.parent != null)
        {
            carpetaGizmos = transform.parent.Find(nomCarpetaPunts);
            
            if (carpetaGizmos == null)
            {
                // Intentar buscar una carpeta que contingui el text "movimen" al mateix nivell
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    Transform hermano = transform.parent.GetChild(i);
                    if (hermano != transform && hermano.name.ToLower().Contains("movimen"))
                    {
                        carpetaGizmos = hermano;
                        break;
                    }
                }
            }
        }
        
        // Dibuixar els punts de patrulla i les seves connexions
        if (carpetaGizmos != null && carpetaGizmos.childCount > 0)
        {
            Gizmos.color = Color.cyan;
            
            // Dibuixar cada punt
            for (int i = 0; i < carpetaGizmos.childCount; i++)
            {
                Transform punt = carpetaGizmos.GetChild(i);
                Gizmos.DrawSphere(punt.position, 0.3f);
                
                // Dibuixar línies entre punts consecutius
                if (i < carpetaGizmos.childCount - 1)
                {
                    Gizmos.DrawLine(punt.position, carpetaGizmos.GetChild(i + 1).position);
                }
                
                // Conectar el último punto con el primero (bucle)
                if (i == carpetaGizmos.childCount - 1 && carpetaGizmos.childCount > 1)
                {
                    Gizmos.DrawLine(punt.position, carpetaGizmos.GetChild(0).position);
                }
            }
            
            // En mode joc, ressaltar el punt actual
            if (Application.isPlaying && puntsPatrulla != null && puntActual < puntsPatrulla.Length)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(puntsPatrulla[puntActual].position, 0.4f);
            }
        }
    }
    
    // Mètode auxiliar per comprovar si un paràmetre existeix a l'animator
    private bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}
