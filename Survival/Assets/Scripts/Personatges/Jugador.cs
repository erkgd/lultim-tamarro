using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SistemaVidaJugador))]
public class Jugador : Personatge
{
    [Header("Referències")]
    [SerializeField] private ParticleSystem efecteInvencibilitat;
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    [SerializeField] private Color colorPerkAtac = new Color(1f, 0.88f, 0f, 1f);
    private Color colorOriginal;
    private CharacterController characterController;
    private Animator animator;
    private BoxCollider boxColliderAtac;
    private SistemaVidaJugador sistemaVida;
    
    // Variables para implementar las propiedades abstractas
    [SerializeField] private float danyAtac = 1f;
    [SerializeField] private float forcaKnockback = 5f;
    private bool atacant = false;
    private bool perkAtacAplicat = false;

    // Componentes modularizados
    private MovimentJugador movimentJugador;
    private AtacJugador atacJugador;

    [Header("Configuració Moviment")]
    [SerializeField] private float velocitat = 5f;
    [SerializeField] private float velocitatRotacio = 120f;
    [SerializeField] private float velocitatCorrer = 10f;
    [SerializeField] private float forcaGravetat = 1f;
    [SerializeField] private float duracioKnockback = 0.25f;

    [Header("Configuració Atac")]
    [SerializeField] private float rangAtacar = 2.0f;
    [SerializeField] private float tempsEntreAtacs = 0.6f;
    [SerializeField] private float tempsAtac = 0.05f;
    [SerializeField] private float angleVisioAtac = 60f;

    [Header("Configuració Invencibilitat")]
    [SerializeField] private float tempsInvencibilitat = 1.7f;
    [SerializeField] private Color colorEfecteInvencibilitat = Color.yellow;
    [SerializeField] private float midaParticules = 0.2f;
    [SerializeField] private float velocitatParticules = 0.5f;
    [SerializeField] private float taxaEmissioParticules = 40f;
    [SerializeField] private float radiEfecte = 1.0f;

    // Implementación de propiedades abstractas
    public override float VidaActual => sistemaVida.VidaActual;
    public override float VidaMaxima => sistemaVida.VidaMaxima;
    public override float Dany => danyAtac;
    public override float ForcaKnockback => forcaKnockback;

    public CharacterController CharacterController => characterController;
    public Animator AnimatorJugador => animator;
    public BoxCollider BoxColliderAtac => boxColliderAtac;

    // Propiedad para acceder al estado de ataque
    public bool Atacant { get => atacant; set => atacant = value; }

    [Header("Àudio d'Atac")]
    [SerializeField] private AudioClip soAtac;
    [SerializeField, Range(0f, 3f)] private float volumAtac = 1f;

    [Header("Àudio de Dany")]
    [SerializeField] private AudioClip soDany;
    [SerializeField, Range(0f, 3f)] private float volumDany = 1f;
    
    // Variable per mantenir referència a l'àudio actual de dany
    private GameObject audioDanyActual;

    protected override void Awake()
    {
        // Inicializar components
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        boxColliderAtac = GetComponent<BoxCollider>();
        sistemaVida = GetComponent<SistemaVidaJugador>();
        
        // Buscar todos los SkinnedMeshRenderer
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshRenderers.Length == 0)
            {
                Debug.LogWarning("No s'han trobat SkinnedMeshRenderer en el personatge");
            }
            else
            {
                Debug.Log($"Trobats {meshRenderers.Length} SkinnedMeshRenderer en el personatge");
            }
        }

        // Guardar el color original del primer mesh (asumimos que todos tienen el mismo color)
        if (meshRenderers.Length > 0)
        {
            colorOriginal = meshRenderers[0].material.color;
        }

        if (boxColliderAtac != null)
            boxColliderAtac.enabled = false;

        // Inicializar components modulares
        movimentJugador = gameObject.AddComponent<MovimentJugador>();
        atacJugador = gameObject.AddComponent<AtacJugador>();
        
        // Configurar components
        ConfigurarComponents();
    }

    private void ConfigurarComponents()
    {
        // Configurar MovimentJugador
        movimentJugador.ConfigurarMoviment(velocitat, velocitatRotacio, velocitatCorrer, forcaGravetat, duracioKnockback);

        // Configurar AtacJugador
        atacJugador.ConfigurarAtac(rangAtacar, tempsEntreAtacs, tempsAtac, angleVisioAtac, danyAtac);
        atacJugador.SetAudioAtac(soAtac, volumAtac);

        // Asegurarse de que exista el componente InvencibilitatJugador para el Singleton
        InvencibilitatJugador invencibilitat = GetComponent<InvencibilitatJugador>();
        if (invencibilitat == null)
        {
            invencibilitat = gameObject.AddComponent<InvencibilitatJugador>();
            Debug.Log("Se ha añadido el componente InvencibilitatJugador al jugador");
        }

        // Configurar InvencibilitatJugador mediante el Singleton
        InvencibilitatJugador.Instance.ConfigurarInvencibilitat(
            tempsInvencibilitat,
            colorEfecteInvencibilitat,
            midaParticules,
            velocitatParticules,
            taxaEmissioParticules,
            radiEfecte
        );
        
        // Verificar si necesitamos crear un sistema de partículas nuevo
        if (efecteInvencibilitat == null)
        {
            Debug.Log("No hay sistema de partículas asignado para invencibilidad, se creará automáticamente");
        }
        
        // Configurar el efecto de invencibilidad (si es null, se creará uno nuevo)
        InvencibilitatJugador.Instance.ConfigurarEfecteInvencibilitat(efecteInvencibilitat);
        Debug.Log("Sistema de invencibilidad configurado correctamente");
    }

    protected override void Start()
    {
        // Suscribirse a eventos del sistema de vida
        sistemaVida.OnVidaCanviada += OnVidaCanviada;
        sistemaVida.OnMuerte += DesactivarControl;
        sistemaVida.OnRevivir += ActivarControl;

        if (SistemaPerks.Instance != null)
        {
            if (SistemaPerks.Instance.EstaDesbloquejada(2) && !perkAtacAplicat)
            {
                AplicarPerkAtac();
            }
            SistemaPerks.Instance.OnPerkChanged += ComprovarPerkAtac;
        }
    }

    private void ComprovarPerkAtac(int indexPerk)
    {
        if (indexPerk == 2 && !perkAtacAplicat)
        {
            AplicarPerkAtac();
        }
    }

    private void AplicarPerkAtac()
    {
        danyAtac = Mathf.RoundToInt(danyAtac * 1.5f);
        perkAtacAplicat = true;

        // Aplicar el color a todos los meshes
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = colorPerkAtac;
                Debug.Log($"Color actualitzat per {meshRenderer.name}");
            }
        }
    }

    private void OnVidaCanviada()
    {
        NotificarCambiVida();
    }
    
    // Método adicional para notificar a los suscriptores
    protected void NotificarCambiVida()
    {
        // Usar el método protegido de la clase base en lugar de reflexión
        InvocarQuanCanviVida();
    }
    
    private void DesactivarControl()
    {
        enabled = false;
    }
    
    private void ActivarControl()
    {
        enabled = true;
    }

    void Update()
    {
        if (!sistemaVida.EsViu()) return;

        movimentJugador.ActualitzarMoviment();
        atacJugador.ActualitzarAtac();
    }

    // Métodos que delegan al sistema de vida
    public override void DecrementarVida(float quantitat, string font = "")
    {
        sistemaVida.DecrementarVida(quantitat, font);
        
        // Reproduir l'àudio de dany
        CrearAudioDany();
    }
    
    public void IncrementarVida(float quantitat)
    {
        sistemaVida.IncrementarVida(quantitat);
    }

    public override IEnumerator Morir()
    {
        // Delegar al sistema de vida
        return sistemaVida.Morir();
    }

    public override bool EstaAtacant()
    {
        return atacant;
    }

    public override void Atacar()
    {
        atacJugador.IniciarAtac();
    }

    public override IEnumerator ExecutarAtac()
    {
        return atacJugador.ExecutarAtac();
    }
    
    // Método para llamar al método protegido desde otros componentes
    public IEnumerator ExecutarAtacPublic()
    {
        return ExecutarAtac();
    }
    
    public void RebreKnockback(Vector3 direccio, float forca)
    {
        movimentJugador.AplicarKnockback(direccio, forca);
    }
    
    // Mètode per crear l'àudio de dany
    private void CrearAudioDany()
    {
        if (soDany != null)
        {
            // Destruir l'àudio anterior si existeix
            if (audioDanyActual != null)
            {
                Destroy(audioDanyActual);
            }

            // 1) Creem un GameObject temporal
            audioDanyActual = new GameObject("AudioDanyTemp");
            audioDanyActual.transform.position = transform.position;

            // 2) Fiquem el AudioSource
            AudioSource aSource = audioDanyActual.AddComponent<AudioSource>();
            aSource.clip = soDany;
            aSource.volume = volumDany;
            aSource.spatialBlend = 0f; // 0 = 2D (sense roll-off)
            aSource.Play();

            // 3) Destruim el AudioSource quan termini
            Destroy(audioDanyActual, soDany.length);
        }
    }
    
    private void OnDestroy()
    {
        if (sistemaVida != null)
        {
            sistemaVida.OnVidaCanviada -= OnVidaCanviada;
            sistemaVida.OnMuerte -= DesactivarControl;
            sistemaVida.OnRevivir -= ActivarControl;
        }

        if (SistemaPerks.Instance != null)
        {
            SistemaPerks.Instance.OnPerkChanged -= ComprovarPerkAtac;
        }

        // Netejar l'àudio de dany si existeix
        if (audioDanyActual != null)
        {
            Destroy(audioDanyActual);
            audioDanyActual = null;
        }

        // Restaurar el color original en todos los meshes
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = colorOriginal;
            }
        }
    }
}