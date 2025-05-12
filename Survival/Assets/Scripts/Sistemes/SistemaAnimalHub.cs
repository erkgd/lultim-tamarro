using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Este sistema gestiona la visibilidad de los animales en el hub según el estado de desbloqueo de las perks.
/// Cada animal se asocia con una perk específica y será visible solo si esa perk está desbloqueada.//
/// </summary>
public class SistemaAnimalHub : MonoBehaviour
{
    [Serializable]
    public class AnimalPerkConfig
    {
        public string etiqueta;          // Tag del animal
        [Tooltip("Índice de la perk asociada: 0=Velocidad, 1=Resistencia, 2=Ataque, 3=Vida")]
        public int indicePerk;           // Índice de la perk asociada
        [Tooltip("Si es verdadero, el animal aparece cuando la perk está desbloqueada")]
        public bool mostrarSiDesbloqueada = true; // Si es true, el animal aparece cuando la perk está desbloqueada
        [Tooltip("GameObject que contiene el modelo del animal (opcional)")]
        public GameObject objetoAnimal;  // Referencia directa al GameObject del animal (opcional)
    }

    [Header("Configuración General")]
    [SerializeField] private bool mostrarMensajes = true;
    [SerializeField] private bool buscarAnimalesAutomaticamente = true;

    [Header("Configuración de Animales")]
    [Tooltip("Configuración para cada animal en el mapa")]
    [SerializeField] private List<AnimalPerkConfig> animalesConfig = new List<AnimalPerkConfig>
    {
        new AnimalPerkConfig { etiqueta = "Escurço", indicePerk = 1, mostrarSiDesbloqueada = true },
        new AnimalPerkConfig { etiqueta = "Marmota", indicePerk = 3, mostrarSiDesbloqueada = true },
        new AnimalPerkConfig { etiqueta = "Ocell", indicePerk = 0, mostrarSiDesbloqueada = true },
        new AnimalPerkConfig { etiqueta = "Isard", indicePerk = 2, mostrarSiDesbloqueada = true }
    };

    // Diccionario para almacenar los GameObjects de los animales
    private Dictionary<string, List<GameObject>> animalesEnMapa = new Dictionary<string, List<GameObject>>();

    private void Start()
    {
        // Inicialización del sistema
        EncontrarAnimales();
        ConfigurarVisibilidadAnimales();
    }

    /// <summary>
    /// Busca todos los animales en la escena según las etiquetas configuradas.
    /// </summary>
    private void EncontrarAnimales()
    {
        if (mostrarMensajes)
            Debug.Log("SistemaAnimalHub: Buscando animales en el mapa...");

        foreach (var config in animalesConfig)
        {
            // Inicializamos la lista para esta etiqueta
            if (!animalesEnMapa.ContainsKey(config.etiqueta))
                animalesEnMapa[config.etiqueta] = new List<GameObject>();

            // Si se ha asignado un objeto directamente, usamos ese
            if (config.objetoAnimal != null)
            {
                animalesEnMapa[config.etiqueta].Add(config.objetoAnimal);
                if (mostrarMensajes)
                    Debug.Log($"SistemaAnimalHub: Animal '{config.objetoAnimal.name}' añadido directamente para etiqueta '{config.etiqueta}'");
            }
            // Si no se ha asignado un objeto y está activada la búsqueda automática
            else if (buscarAnimalesAutomaticamente)
            {
                GameObject[] animalesConTag = GameObject.FindGameObjectsWithTag(config.etiqueta);
                
                if (animalesConTag.Length > 0)
                {
                    animalesEnMapa[config.etiqueta].AddRange(animalesConTag);
                    if (mostrarMensajes)
                        Debug.Log($"SistemaAnimalHub: Encontrados {animalesConTag.Length} objetos con etiqueta '{config.etiqueta}'");
                }
                else if (mostrarMensajes)
                {
                    Debug.LogWarning($"SistemaAnimalHub: No se encontraron objetos con etiqueta '{config.etiqueta}'");
                }
            }
        }
    }

    /// <summary>
    /// Configura la visibilidad de los animales según el estado de las perks.
    /// </summary>
    private void ConfigurarVisibilidadAnimales()
    {
        if (SistemaPerks.Instance == null)
        {
            Debug.LogError("SistemaAnimalHub: No se encontró SistemaPerks.Instance. No se puede configurar la visibilidad de los animales.");
            return;
        }

        if (mostrarMensajes)
            Debug.Log("SistemaAnimalHub: Configurando visibilidad de animales según perks...");

        foreach (var config in animalesConfig)
        {
            // Verificar si la perk está desbloqueada
            bool perkDesbloqueada = SistemaPerks.Instance.EstaDesbloquejada(config.indicePerk);
            
            // Determinar si el animal debe ser visible según la condición configurada
            bool debeSerVisible = (perkDesbloqueada == config.mostrarSiDesbloqueada);
            
            // Si tenemos animales para esta etiqueta en el diccionario
            if (animalesEnMapa.TryGetValue(config.etiqueta, out List<GameObject> animales) && animales.Count > 0)
            {
                foreach (GameObject animal in animales)
                {
                    if (animal != null)
                    {
                        animal.SetActive(debeSerVisible);
                        
                        if (mostrarMensajes)
                        {
                            string nombrePerk = SistemaPerks.Instance.NomPerk(config.indicePerk);
                            Debug.Log($"SistemaAnimalHub: Animal '{animal.name}' con etiqueta '{config.etiqueta}' " +
                                     $"configurado a {(debeSerVisible ? "visible" : "oculto")}. " +
                                     $"La perk '{nombrePerk}' está {(perkDesbloqueada ? "desbloqueada" : "bloqueada")}.");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Actualiza manualmente la visibilidad de los animales (útil para llamar desde otros scripts).
    /// </summary>
    public void ActualizarVisibilidad()
    {
        ConfigurarVisibilidadAnimales();
    }
    
    /// <summary>
    /// Agrega manualmente un animal al sistema.
    /// </summary>
    public void AgregarAnimal(GameObject animal, string etiqueta, int indicePerk, bool mostrarSiDesbloqueada = true)
    {
        // Verificar si el animal ya existe en la configuración
        bool configuracionExistente = false;
        foreach (var config in animalesConfig)
        {
            if (config.etiqueta == etiqueta && config.indicePerk == indicePerk)
            {
                configuracionExistente = true;
                if (config.objetoAnimal == null)
                    config.objetoAnimal = animal;
                break;
            }
        }
        
        // Si no existe la configuración, agregarla
        if (!configuracionExistente)
        {
            AnimalPerkConfig nuevaConfig = new AnimalPerkConfig 
            { 
                etiqueta = etiqueta, 
                indicePerk = indicePerk, 
                mostrarSiDesbloqueada = mostrarSiDesbloqueada,
                objetoAnimal = animal
            };
            
            animalesConfig.Add(nuevaConfig);
        }
        
        // Asegurarse de que el animal esté en el diccionario
        if (!animalesEnMapa.ContainsKey(etiqueta))
            animalesEnMapa[etiqueta] = new List<GameObject>();
        
        if (!animalesEnMapa[etiqueta].Contains(animal))
            animalesEnMapa[etiqueta].Add(animal);
        
        // Actualizar la visibilidad según el estado actual
        ConfigurarVisibilidadAnimales();
    }
}