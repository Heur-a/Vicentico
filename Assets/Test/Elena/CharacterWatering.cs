using UnityEngine;
using UnityEngine.UI; // Para el slider de agua del personaje
using System.Collections;

// Este script debe ir en el personaje que tiene el tag "PersonajeBebedor"
public class CharacterWatering : MonoBehaviour
{
    [Header("Configuración de Agua del Personaje")]
    [Tooltip("Cantidad máxima de agua que puede cargar el personaje")]
    public float maxWaterCapacity = 100f;
    
    [Tooltip("Cantidad actual de agua que tiene el personaje")]
    [Range(0f, 100f)]
    public float currentWater = 0f;
    
    [Tooltip("Cantidad de agua que se gana por segundo al beber")]
    public float waterGainRate = 20f; // 20 unidades por segundo
    
    [Tooltip("Cantidad de agua que se usa para regar una planta")]
    public float waterPerPlant = 30f;
    
    [Header("UI del Personaje (Opcional)")]
    [Tooltip("Slider para mostrar cuánta agua tiene el personaje")]
    public Slider waterSlider;
    
    [Header("Configuración de Riego")]
    [Tooltip("Riego automático al acercarse a la planta")]
    public bool automaticWatering = true;
    
    [Tooltip("Cooldown entre riegos en segundos (para cada planta)")]
    public float wateringCooldown = 2f;
    
    [Header("Configuración del Sonido")]
    [Tooltip("Sonido al regar una planta")]
    public AudioSource wateringSound;
    
    [Header("Efectos Visuales (Opcional)")]
    [Tooltip("Partículas de agua al regar")]
    public ParticleSystem wateringParticles;
    
    [Header("Mensajes en Pantalla")]
    [Tooltip("¿Mostrar mensajes de las acciones?")]
    public bool showMessages = true;
    
    [Tooltip("Colores de los mensajes")]
    public Color messageColorInfo = Color.white;
    public Color messageColorSuccess = Color.green;
    public Color messageColorWarning = Color.yellow;
    public Color messageColorError = Color.red;
    
    // Variables privadas
    private bool isDrinking = false;
    private float lastWaterAmount = 0f;
    // Diccionario para guardar el último tiempo de riego de cada planta
    private System.Collections.Generic.Dictionary<Plant, float> plantWateringTimes = new System.Collections.Generic.Dictionary<Plant, float>();
    
    void Start()
    {
        // Inicializar el slider si existe
        UpdateWaterUI();
    }
    
    void Update()
    {
        // Si está bebiendo, aumentar el agua
        if (isDrinking && currentWater < maxWaterCapacity)
        {
            lastWaterAmount = currentWater;
            currentWater += waterGainRate * Time.deltaTime;
            currentWater = Mathf.Min(currentWater, maxWaterCapacity);
            UpdateWaterUI();
            
            // Mensaje cuando llega a capacidad máxima
            if (currentWater >= maxWaterCapacity && lastWaterAmount < maxWaterCapacity)
            {
                ShowMessage("¡Agua llena! " + currentWater.ToString("F0") + "/" + maxWaterCapacity, messageColorSuccess);
            }
        }
    }
    
    // Esta función se llama desde WaterBowl cuando el personaje empieza a beber
    public void StartDrinking()
    {
        isDrinking = true;
        Debug.Log(gameObject.name + " está bebiendo agua.");
        ShowMessage("Bebiendo agua...", messageColorInfo);
    }
    
    // Esta función se llama desde WaterBowl cuando el personaje deja de beber
    public void StopDrinking()
    {
        isDrinking = false;
        Debug.Log(gameObject.name + " dejó de beber. Agua actual: " + currentWater);
        ShowMessage("Agua recogida: " + currentWater.ToString("F0") + "/" + maxWaterCapacity, messageColorInfo);
    }
    
    // Verificar si se puede regar una planta específica
    bool CanWaterPlant(Plant plant)
    {
        // La planta no debe ser null
        if (plant == null)
        {
            return false;
        }
        
        // Solo regar plantas que no estén en salud máxima
        if (plant.healthLevel >= 3)
        {
            ShowMessage(plant.gameObject.name + " ya está completamente sana.", messageColorInfo);
            return false;
        }
        
        // Debe tener agua suficiente
        if (currentWater < waterPerPlant)
        {
            ShowMessage("No tienes suficiente agua. Tienes: " + currentWater.ToString("F0") + "/" + waterPerPlant.ToString("F0"), messageColorWarning);
            return false;
        }
        
        // Verificar el cooldown específico de esta planta
        if (plantWateringTimes.ContainsKey(plant))
        {
            float timeSinceLastWatering = Time.time - plantWateringTimes[plant];
            if (timeSinceLastWatering < wateringCooldown)
            {
                float remainingTime = wateringCooldown - timeSinceLastWatering;
                ShowMessage("Espera " + remainingTime.ToString("F1") + "s antes de regar " + plant.gameObject.name + " de nuevo", messageColorWarning);
                return false;
            }
        }
        
        return true;
    }
    
    // Regar una planta específica
    void WaterPlant(Plant plant)
    {
        if (plant == null) return;
        
        // Verificar si se puede regar
        if (!CanWaterPlant(plant)) return;
        
        // Guardar el nombre de la planta antes de regarla
        string plantName = plant.gameObject.name;
        int previousHealth = plant.healthLevel;
        
        // Consumir agua
        currentWater -= waterPerPlant;
        UpdateWaterUI();
        
        // Curar la planta
        plant.Heal();
        
        // Actualizar el tiempo del último riego de esta planta específica
        if (plantWateringTimes.ContainsKey(plant))
        {
            plantWateringTimes[plant] = Time.time;
        }
        else
        {
            plantWateringTimes.Add(plant, Time.time);
        }
        
        // Efectos de sonido
        if (wateringSound != null && !wateringSound.isPlaying)
        {
            wateringSound.Play();
        }
        
        // Efectos visuales
        if (wateringParticles != null)
        {
            wateringParticles.transform.position = plant.transform.position;
            wateringParticles.Play();
        }
        
        Debug.Log("¡Regaste " + plantName + "! Agua restante: " + currentWater);
        
        // Mensaje de éxito
        string healthStatus = plant.healthLevel == 3 ? "¡Totalmente recuperada!" : "Salud mejorada (" + previousHealth + " → " + plant.healthLevel + ")";
        ShowMessage("¡" + plantName + " regada! " + healthStatus + " | Agua: " + currentWater.ToString("F0"), messageColorSuccess);
    }
    
    // Actualizar la UI del agua
    void UpdateWaterUI()
    {
        if (waterSlider != null)
        {
            waterSlider.value = currentWater / maxWaterCapacity;
        }
    }
    
    // Regar automáticamente cuando el personaje entra en el collider de una planta
    void OnTriggerEnter(Collider other)
    {
        if (!automaticWatering) return;
        
        // Intentar obtener el componente Plant del objeto con el que colisionamos
        Plant plant = other.GetComponent<Plant>();
        
        if (plant != null)
        {
            Debug.Log("Detectada planta: " + plant.gameObject.name + " - Nivel: " + plant.healthLevel);
            WaterPlant(plant);
        }
    }
    
    // Método auxiliar para mostrar mensajes
    void ShowMessage(string message, Color color)
    {
        if (!showMessages) return;
        
        if (MessageDisplay.Instance != null)
        {
            MessageDisplay.Instance.ShowMessage(message, -1f, color);
        }
        else
        {
            Debug.LogWarning("No hay MessageDisplay en la escena. Añade un objeto con el script MessageDisplay.");
        }
    }
    
    // Visualización en el editor (debug)
    void OnDrawGizmosSelected()
    {
        // Dibujar líneas hacia las plantas que se regaron recientemente
        Gizmos.color = Color.cyan;
        foreach (var kvp in plantWateringTimes)
        {
            if (kvp.Key != null)
            {
                float timeSinceWatering = Time.time - kvp.Value;
                
                // Solo dibujar si fue regada en los últimos 3 segundos
                if (timeSinceWatering < 3f)
                {
                    Gizmos.DrawLine(transform.position, kvp.Key.transform.position);
                }
            }
        }
    }
}

