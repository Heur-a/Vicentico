using UnityEngine;
using System.Collections; // Necesario para eventos

// Este script debe ir en cada uno de tus 3 objetos de planta
public class Plant : MonoBehaviour
{
    // --- Variables Públicas (visibles en el Inspector) ---
    [Header("Configuración de Vida")]
    [Tooltip("Nivel de vida actual: 3=Sana, 2=Muriendo, 1=Muerta")]
    [Range(1, 3)]
    public int healthLevel = 3; // Empezamos en 3 por defecto

    [Header("Configuración Visual")]
    public Renderer plantRenderer; // Arrastra aquí el MeshRenderer o SpriteRenderer de tu planta
    public Color healthyColor = Color.green;
    public Color dyingColor = new Color(1f, 0.6f, 0f); // Naranja
    public Color deadColor = Color.red;

    // --- Eventos ---
    // Un evento es como una señal de radio.
    // Cuando la planta muera, gritará "¡He muerto!".
    // El PlanetManager estará escuchando esa señal.
    public static event System.Action<Plant> OnPlantStateChanged;

    // --- Estado Privado ---
    private int _previousHealthLevel; // Para detectar cambios

    void Start()
    {
        // Al empezar, guardamos el estado inicial y actualizamos el visual
        _previousHealthLevel = healthLevel;
        UpdateVisuals();
    }

    // Esta función la llamarás desde otro script cuando la planta reciba daño
    public void TakeDamage()
    {
        // Solo restamos vida si no está ya muerta
        if (healthLevel > 1)
        {
            healthLevel--; // Baja el nivel (ej. de 3 a 2)
            Debug.Log(gameObject.name + " ahora tiene vida: " + healthLevel);

            // Avisamos al sistema de que hemos cambiado de estado
            NotifyStateChange();
        }
    }

    // Esta función la llamarás para curar la planta (si quieres)
    public void Heal()
    {
        if (healthLevel < 3)
        {
            healthLevel++;
            Debug.Log(gameObject.name + " ahora tiene vida: " + healthLevel);
            
            // Avisamos al sistema de que hemos cambiado de estado
            NotifyStateChange();
        }
    }

    // Función privada para notificar al manager y actualizar el color
    private void NotifyStateChange()
    {
        // Actualizamos el color
        UpdateVisuals();

        // Lanzamos el evento "OnPlantStateChanged"
        // Le pasamos 'this' (esta misma planta) para que el manager sepa quién cambió
        OnPlantStateChanged?.Invoke(this);
    }

    // Actualiza el color de la planta basado en su healthLevel
    public void UpdateVisuals()
    {
        if (plantRenderer == null)
        {
            Debug.LogWarning("No hay Renderer asignado en " + gameObject.name);
            return;
        }

        switch (healthLevel)
        {
            case 3: // Sana
                plantRenderer.material.color = healthyColor;
                break;
            case 2: // Muriendo
                plantRenderer.material.color = dyingColor;
                break;
            case 1: // Muerta
                plantRenderer.material.color = deadColor;
                break;
            default: // Caso por defecto (nunca debería pasar)
                plantRenderer.material.color = Color.white;
                break;
        }
    }

  // --- AÑADE ESTAS FUNCIONES EN SU LUGAR ---
    
    // Esta función se llama automáticamente cuando haces clic
    // izquierdo sobre un objeto que tiene un Collider.
    private void OnMouseDown()
    {
        Debug.Log("Has hecho clic en: " + gameObject.name);
        TakeDamage();
    }

    // Opcional: Dejemos una forma de curar para probar.
    // Si el ratón está ENCIMA de la planta y pulsas "H", se cura.
    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Curando a: " + gameObject.name);
            Heal();
        }
    }
}