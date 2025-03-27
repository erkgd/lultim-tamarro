using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Jugador))]
public class InvencibilitatJugador : MonoBehaviour
{
    private Jugador jugador;
    private Animator animator;
    private ParticleSystem efecteInvencibilitat;
    
    private float tempsInvencibilitat;
    private Color colorEfecteInvencibilitat;
    private float midaParticules;
    private float velocitatParticules;
    private float taxaEmissioParticules;
    private float radiEfecte;
    
    public bool EsInvencible { get; private set; } = false;
    
    private void Awake()
    {
        jugador = GetComponent<Jugador>();
        animator = jugador.AnimatorJugador;
    }
    
    public void ConfigurarInvencibilitat(
        float tempsInvencibilitat,
        Color colorEfecteInvencibilitat,
        float midaParticules,
        float velocitatParticules,
        float taxaEmissioParticules,
        float radiEfecte)
    {
        this.tempsInvencibilitat = tempsInvencibilitat;
        this.colorEfecteInvencibilitat = colorEfecteInvencibilitat;
        this.midaParticules = midaParticules;
        this.velocitatParticules = velocitatParticules;
        this.taxaEmissioParticules = taxaEmissioParticules;
        this.radiEfecte = radiEfecte;
    }
    
    public void ConfigurarEfecteInvencibilitat(ParticleSystem efecte)
    {
        efecteInvencibilitat = efecte;
        
        // Si no tenim un efecte assignat, en creem un de nou
        if (efecteInvencibilitat == null)
        {
            CrearEfecteInvencibilitat();
        }
    }
    
    private void CrearEfecteInvencibilitat()
    {
        // Buscar si ja existeix
        efecteInvencibilitat = GetComponentInChildren<ParticleSystem>();
        
        // Si no existeix, crear-ne un
        if (efecteInvencibilitat == null)
        {
            GameObject efectoObj = new GameObject("EfecteInvencibilitat");
            efectoObj.transform.SetParent(transform);
            efectoObj.transform.localPosition = Vector3.up * 0.5f; // A mitja alçada del personatge
            
            efecteInvencibilitat = efectoObj.AddComponent<ParticleSystem>();
            
            // Asegurar que el sistema está detenido antes de configurarlo
            efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // Sistema de partícules
            var main = efecteInvencibilitat.main;
            main.loop = false; // Configurar com a no loop
            main.startLifetime = Mathf.Min(1.0f, tempsInvencibilitat);
            main.startSpeed = velocitatParticules;
            main.startSize = midaParticules;
            main.startColor = colorEfecteInvencibilitat;
            main.duration = tempsInvencibilitat; // Configurar durada igual al temps d'invencibilitat
            
            // Emissor de partícules
            var emission = efecteInvencibilitat.emission;
            emission.rateOverTime = taxaEmissioParticules;
            
            // Forma (esfera al voltant del personatge)
            var shape = efecteInvencibilitat.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radiEfecte;
            shape.radiusThickness = 0.0f; // Emetre des de la superfície
            
            // Moviment de les partícules
            var velocity = efecteInvencibilitat.velocityOverLifetime;
            velocity.orbitalY = 1.0f;
            
            // Color groc i transparència
            var colorOverLifetime = efecteInvencibilitat.colorOverLifetime;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(colorEfecteInvencibilitat, 0.0f),
                    new GradientColorKey(new Color(1f, 0.7f, 0.0f), 1.0f) // Taronja daurat
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.9f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorOverLifetime.color = gradient;
            
            // Renderer per assegurar el color correcte
            var renderer = efecteInvencibilitat.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = colorEfecteInvencibilitat;
            
            // Afegim una transició suau al final
            var sizeOverLifetime = efecteInvencibilitat.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0, 0.7f);
            sizeCurve.AddKey(0.5f, 1f);
            sizeCurve.AddKey(1, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }
        else
        {
            // Si ya existe, asegurarnos de que esté detenido antes de configurarlo
            efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            
            // Actualizar configuración
            var main = efecteInvencibilitat.main;
            main.loop = false;
            main.duration = tempsInvencibilitat;
            main.startColor = colorEfecteInvencibilitat;
            main.startLifetime = Mathf.Max(1.0f, tempsInvencibilitat * 0.5f); // Temps de vida més llarg
        }
        
        // Desactivem inicialment, però sense netejar
        efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    
    public void ActivarInvencibilitat()
    {
        StartCoroutine(PeriodeInvencibilitat());
    }
    
    private IEnumerator PeriodeInvencibilitat()
    {
        EsInvencible = true;
        
        // Configurar i activar les partícules
        if (efecteInvencibilitat != null)
        {
            // Aturar les partícules anteriors però permetent que acabin suaument
            efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            
            // Esperem un instant per assegurar-nos que el sistema està preparat
            yield return null;
            
            // Configurar propietats per obtenir una transició suau
            var main = efecteInvencibilitat.main;
            main.startColor = colorEfecteInvencibilitat;
            main.duration = tempsInvencibilitat;
            main.loop = false;
            main.startLifetime = Mathf.Max(1.0f, tempsInvencibilitat * 0.75f); // Vida més llarga que la durada
            
            // Activar el sistema
            efecteInvencibilitat.Play();
        }
        
        // Activem l'animació d'invencibilitat si existeix al controlador d'animació
        if (animator != null)
        {
            // Comprovar si el paràmetre existeix abans de configurar-lo
            if (HasParameter("Invencibilitat", animator))
            {
                animator.SetTrigger("Invencibilitat");
            }
            else if (HasParameter("Invincible", animator))
            {
                animator.SetTrigger("Invincible");
            }
            else if (HasParameter("Hit", animator))
            {
                animator.SetTrigger("Hit"); // Paràmetre comú per quan el personatge rep un cop
            }
        }
        
        // Esperem el temps d'invencibilitat exacte
        yield return new WaitForSeconds(tempsInvencibilitat);
        
        // Finalització gradual de les partícules
        if (efecteInvencibilitat != null)
        {
            // Detenim només l'emissió permetent que les partícules existents completin el seu cicle
            efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            
            // Càlcul de temps de desaparició basada en la vida de les partícules
            float tempsDesaparicio = efecteInvencibilitat.main.startLifetime.constant;
            
            // Aplicar una transició de desaparició
            float valorInicial = 1.0f;
            float temps = 0f;
            
            while (temps < tempsDesaparicio)
            {
                temps += Time.deltaTime;
                float t = temps / tempsDesaparicio;
                
                // Reduïm gradualment l'escala per aconseguir un efecte de desaparició més suau
                float escalaActual = Mathf.Lerp(valorInicial, 0f, t);
                efecteInvencibilitat.transform.localScale = Vector3.one * escalaActual;
                
                yield return null;
            }
            
            // Assegurem que les partícules s'aturen completament al final
            efecteInvencibilitat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            efecteInvencibilitat.transform.localScale = Vector3.one; // Restaurar escala
        }
        
        EsInvencible = false;
    }
    
    // Mètode per comprovar si un paràmetre existeix a l'Animator
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
