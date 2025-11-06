using UnityEngine;
using UnityEngine.UI; // Necesario para el Slider
using System.Collections; // Necesario para Corutinas

// Este script debe ir en el objeto "Bowl" (el que tiene el Box Collider)
public class WaterBowl : MonoBehaviour
{
    [Header("Configuración de la Barra")]
    [Tooltip("Arrastra aquí el Slider de UI que creaste en el Canvas World Space")]
    public Slider waterBar;
    
    [Tooltip("Velocidad a la que se vacía la barra (unidades por segundo)")]
    public float drainSpeed = 0.1f; // Ej: 0.1 = 10% por segundo, tarda 10s en vaciarse

    [Tooltip("Velocidad a la que se rellena la barra (unidades por segundo)")]
    public float refillSpeed = 0.05f; // Ej: 0.05 = 5% por segundo, tarda 20s en rellenarse

    [Header("Configuración del Sonido")]
    [Tooltip("Componente AudioSource que contiene el clip de 'beber agua'")]
    public AudioSource drinkingSound;
    
    [Header("Mensajes")]
    [Tooltip("¿Mostrar mensajes cuando el bowl está vacío?")]
    public bool showMessages = true;

    private bool isDrinking = false;
    private Coroutine refillCoroutine; // Para guardar la referencia a la rutina de rellenado
    private bool hasShownEmptyMessage = false; // Para no repetir el mensaje

    void Start()
    {
        // Asegurarnos de que el sonido esté configurado para loop
        if (drinkingSound != null)
        {
            drinkingSound.loop = true;
            drinkingSound.playOnAwake = false;
        }
        
        // Empezar con la barra llena
        if (waterBar != null)
        {
            waterBar.value = 1f; // 1f = 100%
        }
    }

    void Update()
    {
        // Si el personaje está bebiendo...
        if (isDrinking)
        {
            // ...y la barra aún tiene agua...
            if (waterBar.value > 0)
            {
                // ...vaciar la barra poco a poco.
                waterBar.value -= drainSpeed * Time.deltaTime;
                hasShownEmptyMessage = false; // Resetear el flag mientras hay agua
            }
            else
            {
                // ...si la barra llega a 0...
                waterBar.value = 0;
                isDrinking = false; // Ya no puede beber
                drinkingSound.Stop(); // Parar el sonido
                
                // Mostrar mensaje de que el bowl está vacío
                if (!hasShownEmptyMessage)
                {
                    ShowMessage("¡El bowl está vacío! Espera a que se rellene...", new Color(1f, 0.5f, 0f)); // Naranja
                    hasShownEmptyMessage = true;
                }
            }
        }
        
        // Resetear el flag si el bowl se ha rellenado
        if (waterBar.value >= 1f)
        {
            hasShownEmptyMessage = false;
        }
    }

    // Se ejecuta cuando un Collider CON Rigidbody entra en este Trigger
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("¡ALGO HA ENTRADO! Se llama: " + other.name); // <-- AÑADE ESTA LÍNEA
        // Comprobar si es el personaje correcto
        if (other.CompareTag("PersonajeBebedor"))
        {
            Debug.Log(other.name + " ha entrado en el bowl.");

            // Si se estaba rellenando, parar esa rutina
            if (refillCoroutine != null)
            {
                StopCoroutine(refillCoroutine);
                refillCoroutine = null;
            }
            
            // Solo empezar a beber si hay agua
            if (waterBar.value > 0)
            {
                isDrinking = true;
                drinkingSound.Play();
                
                // Notificar al personaje que está bebiendo
                CharacterWatering character = other.GetComponent<CharacterWatering>();
                if (character != null)
                {
                    character.StartDrinking();
                }
            }
            else
            {
                // Si el bowl está vacío, avisar al personaje
                ShowMessage("El bowl está vacío. Espera a que se rellene...", new Color(1f, 0.5f, 0f));
            }
        }
    }

    // Se ejecuta cuando el Collider sale del Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PersonajeBebedor"))
        {
            Debug.Log(other.name + " ha salido del bowl.");

            // Parar de beber y parar el sonido
            isDrinking = false;
            if (drinkingSound != null)
            {
                drinkingSound.Stop();
            }
            
            // Notificar al personaje que dejó de beber
            CharacterWatering character = other.GetComponent<CharacterWatering>();
            if (character != null)
            {
                character.StopDrinking();
            }

            // Empezar la rutina de rellenado
            refillCoroutine = StartCoroutine(RefillWater());
        }
    }

    // Una Corutina para rellenar la barra poco a poco
    IEnumerator RefillWater()
    {
        // Mientras la barra no esté llena Y el personaje no esté bebiendo...
        while (waterBar.value < 1f && !isDrinking)
        {
            // ...rellenar la barra.
            waterBar.value += refillSpeed * Time.deltaTime;
            yield return null; // Esperar al siguiente fotograma
        }

        // Asegurarse de que queda en 1 exacto
        waterBar.value = 1f;
        refillCoroutine = null; // Limpiar la referencia
        
        // Mostrar mensaje cuando el bowl se ha rellenado completamente
        ShowMessage("¡Bowl rellenado! Ya puedes beber agua.", Color.cyan);
    }
    
    // Método auxiliar para mostrar mensajes
    void ShowMessage(string message, Color color)
    {
        if (!showMessages) return;
        
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowMessage(message, -1f, color);
        }
    }
}