using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Este script se encarga de asegurar que siempre exista un EventSystem en la escena,
/// necesario para que el nuevo Input System funcione correctamente tanto en el editor como en builds.
/// </summary>
[DefaultExecutionOrder(-1000)] // Ejecución muy temprana para garantizar que existe antes que cualquier otro script
public class InputSystemManager : MonoBehaviour
{
    private static InputSystemManager instance;

    [Header("Configuración")]
    [SerializeField] private bool mostrarMensajes = true;
    [SerializeField] private string nombreEventSystem = "EventSystem";

    [Header("Referencias opcionales")]
    [SerializeField] private InputActionAsset acciones;

    private void Awake()
{
    // Implementación del patrón Singleton
    if (instance == null)
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Verificar si existe un EventSystem y crearlo si no existe
        CrearEventSystemSiNoExiste();
    }
    else if (instance != this)
    {
        Destroy(this); 
        if (mostrarMensajes)
            Debug.LogWarning($"InputSystemManager: Instancia duplicada del componente InputSystemManager destruida en {gameObject.name}. La instancia original está en {instance.gameObject.name}.");
        return;
    }
}

    /// <summary>
    /// Verifica si existe un EventSystem en la escena y crea uno si no existe
    /// </summary>
    private void CrearEventSystemSiNoExiste()
    {
        // Buscar si ya existe un EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        
        if (eventSystem == null)
        {
            // No existe, creamos uno nuevo
            GameObject eventoSistema = new GameObject(nombreEventSystem);
            
            // Añadir los componentes necesarios
            eventSystem = eventoSistema.AddComponent<EventSystem>();
            
            // Usar InputSystemUIInputModule en lugar de StandaloneInputModule para el nuevo Input System
            InputSystemUIInputModule inputModule = eventoSistema.AddComponent<InputSystemUIInputModule>();
            
            // Asignar las acciones si están disponibles
            if (acciones != null)
            {
                inputModule.actionsAsset = acciones;
            }
            
            // Asegurarse de que persista entre escenas
            DontDestroyOnLoad(eventoSistema);
            
            if (mostrarMensajes)
                Debug.Log("InputSystemManager: EventSystem creado automáticamente");
        }
        else if (mostrarMensajes)
        {
            Debug.Log($"InputSystemManager: Se encontró un EventSystem existente: {eventSystem.name}");
            
            // Verificar si tiene el módulo correcto para el nuevo Input System
            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                Debug.LogWarning("El EventSystem no tiene un InputSystemUIInputModule. Esto puede causar problemas con el nuevo Input System.");
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("Se ha agregado un InputSystemUIInputModule al EventSystem existente.");
            }
        }
    }
}