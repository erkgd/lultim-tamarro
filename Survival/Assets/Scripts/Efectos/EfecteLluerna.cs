using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EfecteLluerna : MonoBehaviour
{
    private ParticleSystem sistemaParticules;
    private ParticleSystem.MainModule mainModule;
    
    [Header("Configuració Lluerna")]
    [SerializeField] private Color colorLluerna = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private float intensitatMinima = 0.2f;
    [SerializeField] private float intensitatMaxima = 1f;
    [SerializeField] private float velocitatParpadeig = 2f;
    
    private void Awake()
    {
        sistemaParticules = GetComponent<ParticleSystem>();
        mainModule = sistemaParticules.main;
        
        ConfigurarEfecteLluerna();
    }
    
    private void ConfigurarEfecteLluerna()
    {
        // Configuració principal
        mainModule.startLifetime = new ParticleSystem.MinMaxCurve(5f, 7f);
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.3f);
        mainModule.startColor = new ParticleSystem.MinMaxGradient(colorLluerna);
        
        // Configurar emisió
        var emission = sistemaParticules.emission;
        emission.rateOverTime = 10f;
        
        // Configurar forma
        var shape = sistemaParticules.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 5f;
        
        // Configurar mida al llarg del temps per parpadeig
        var sizeOverLifetime = sistemaParticules.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        // Crear corba de parpadeig
        AnimationCurve parpadeigCurve = new AnimationCurve();
        for (float t = 0; t <= 1f; t += 0.1f)
        {
            float valor = Mathf.Lerp(intensitatMinima, intensitatMaxima, 
                (Mathf.Sin(t * velocitatParpadeig * Mathf.PI * 2) + 1f) / 2f);
            parpadeigCurve.AddKey(t, valor);
        }
        
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, parpadeigCurve);
        
        // Configurar moviment
        var velocityOverLifetime = sistemaParticules.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.speedModifier = 0.1f;
    }
    
    public void AjustarIntensitat(float minima, float maxima)
    {
        intensitatMinima = minima;
        intensitatMaxima = maxima;
        ConfigurarEfecteLluerna();
    }
    
    public void AjustarVelocitatParpadeig(float velocitat)
    {
        velocitatParpadeig = velocitat;
        ConfigurarEfecteLluerna();
    }
    
    public void AjustarColor(Color nouColor)
    {
        colorLluerna = nouColor;
        ConfigurarEfecteLluerna();
    }
} 