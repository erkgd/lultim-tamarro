using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Jugador))]
public class AtacJugador : MonoBehaviour
{
    private Jugador jugador;
    private Animator animator;
    private BoxCollider boxColliderAtac;
    
    [Header("Configuració Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 0.6f;
    [SerializeField] private float tempsAtac = 0.05f;
    [SerializeField] private float angleVisioAtac = 60f;
    [SerializeField] private int danyAtac = 1;
    
    private float comptadorAtacs = 0f;
    
    private void Awake()
    {
        jugador = GetComponent<Jugador>();
        animator = jugador.AnimatorJugador;
        boxColliderAtac = jugador.BoxColliderAtac;
    }
    
    public void ActualitzarAtac()
    {
        // Actualitzem el temporitzador d'atacs
        if (comptadorAtacs > 0f)
        {
            comptadorAtacs -= Time.deltaTime;
        }

        // Control de l'atac
        if (Input.GetButtonDown("Atacar") && comptadorAtacs <= 0f)
        {
            IniciarAtac();
        }
    }
    
    public void IniciarAtac()
    {
        StartCoroutine(jugador.ExecutarAtacPublic());
        comptadorAtacs = tempsEntreAtacs;
    }
    
    public IEnumerator ExecutarAtac()
    {
        // Activar immediatament les coses del atac
        jugador.Atacant = true;  // Usamos la propiedad pública
        if (boxColliderAtac != null)
            boxColliderAtac.enabled = true;

        animator.SetTrigger("TrAtac");
        
        // Verificar si estem mirant cap a un enemic en paral·lel
        bool mirantEnemic = EstaMirantEnemic();
        
        // Només fem dany si estem mirant a l'enemic
        if (mirantEnemic)
        {
            Debug.Log("Atacant a un enemic visible!");
            // Apliquem el dany immediatament
            AplicarDanyAEnemicsVisibles();
        }
        else
        {
            Debug.Log("Atacant a l'aire, no hi ha cap enemic a la vista.");
        }

        // Reduïm el temps d'espera entre l'activació i desactivació del collider
        yield return new WaitForSeconds(tempsAtac);

        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;

        jugador.Atacant = false;  // Usamos la propiedad pública
    }
    
    // Optimització del mètode per verificar si estem mirant cap a un enemic
    private bool EstaMirantEnemic()
    {
        // Buscar enemics en un con davant del jugador
        Collider[] enemics = Physics.OverlapSphere(transform.position, rangAtacar);
        
        // Optimitzar la detecció
        Vector3 forward = transform.forward;
        
        foreach (Collider enemic in enemics)
        {
            // Comprovar si és un enemic
            if (enemic.CompareTag("Enemy"))
            {
                // Calcular la direcció cap a l'enemic
                Vector3 direccioEnemic = (enemic.transform.position - transform.position).normalized;
                
                // Calcular l'angle entre la direcció del jugador i la direcció cap a l'enemic
                float angle = Vector3.Angle(forward, direccioEnemic);
                
                // Si l'enemic està dins de l'angle de visió, estem mirant cap a ell
                if (angle < angleVisioAtac)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Mètode per aplicar dany als enemics visibles
    private void AplicarDanyAEnemicsVisibles()
    {
        // Buscar enemics en un con davant del jugador
        Collider[] enemics = Physics.OverlapSphere(transform.position, rangAtacar);
        
        foreach (Collider enemic in enemics)
        {
            // Comprovar si és un enemic
            if (enemic.CompareTag("Enemy"))
            {
                // Calcular la direcció cap a l'enemic
                Vector3 direccioEnemic = (enemic.transform.position - transform.position).normalized;
                
                // Calcular l'angle entre la direcció del jugador i la direcció cap a l'enemic
                float angle = Vector3.Angle(transform.forward, direccioEnemic);
                
                // Si l'enemic està dins de l'angle de visió, li apliquem dany
                if (angle < angleVisioAtac)
                {
                    Enemic scriptEnemic = enemic.GetComponent<Enemic>();
                    if (scriptEnemic != null)
                    {
                        scriptEnemic.DecrementarVida(danyAtac, gameObject.name);
                        
                        // Comprovar si l'enemic ha mort i activar l'animació de mort
                        if (scriptEnemic.VidaActual <= 0)
                        {
                            Animator animatorEnemic = scriptEnemic.GetComponent<Animator>();
                            if (animatorEnemic != null)
                            {
                                animatorEnemic.SetBool("EnemicMort", true);
                            }
                        }
                    }
                }
            }
        }
    }
    
    // Mètode per visualitzar el con d'atac a l'editor
    private void OnDrawGizmosSelected()
    {
        // Dibuixar el radi d'atac
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangAtacar);
        
        // Dibuixar el con de visió
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * rangAtacar;
        float halfAngle = angleVisioAtac * 0.5f * Mathf.Deg2Rad;
        Vector3 right = transform.right * Mathf.Sin(halfAngle) * rangAtacar;
        Vector3 up = transform.up * Mathf.Sin(halfAngle) * rangAtacar;
        
        // Línies del con
        Gizmos.DrawRay(transform.position, forward + right);
        Gizmos.DrawRay(transform.position, forward - right);
        Gizmos.DrawRay(transform.position, forward + up);
        Gizmos.DrawRay(transform.position, forward - up);
    }
}
