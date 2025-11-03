using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asegúrate de tener esto si usas TextMeshPro

public class PlanetHealthManager : MonoBehaviour
{
    [Header("Barra de Vida (UI)")]
    public Image planetHealthBar;
    public TMP_Text healthPercentageText; // O "public Text ..." si usas el antiguo

    [Header("Configuración de Animación")]
    [Tooltip("Velocidad de la animación. 0.5 = la barra tarda 2 segundos en vaciarse (1 / 0.5). 1 = tarda 1 segundo.")]
    public float animationSpeed = 0.5f; // <-- NUEVA VARIABLE

    // --- Variables Privadas para la Animación ---
    private float targetFillAmount = 1f;   // <-- El valor al que QUEREMOS ir
    private float currentFillAmount = 1f;  // <-- El valor VISUAL actual de la barra
    
    // --- Conexión con las Plantas ---

    void OnEnable()
    {
        Plant.OnPlantStateChanged += HandlePlantStateChanged;
    }

    void OnDisable()
    {
        Plant.OnPlantStateChanged -= HandlePlantStateChanged;
    }

    void Start()
    {
        // Al empezar, calculamos la vida inicial
        UpdatePlanetHealthLogic();
        
        // Sincronizamos los valores iniciales sin animar
        currentFillAmount = targetFillAmount; // <-- MODIFICADO
        UpdateVisuals(currentFillAmount);     // <-- MODIFICADO
    }

    // Esta función se ejecuta CADA VEZ que una planta cambia de estado
    private void HandlePlantStateChanged(Plant plantQueCambio)
    {
        // 1. Calculamos cuál DEBERÍA ser la nueva vida
        UpdatePlanetHealthLogic();
        
        // 2. ¡YA NO actualizamos la UI aquí!
        //    Solo le hemos dado un nuevo "targetFillAmount".
        //    La función Update() se encargará de la animación.
    }

    // Esta función AHORA solo calcula la lógica, no actualiza la UI
    private void UpdatePlanetHealthLogic() // <-- CAMBIADO de "UpdatePlanetHealth"
    {
        // 1. Encontrar todas las plantas
        Plant[] allPlants = FindObjectsOfType<Plant>();
        if (allPlants.Length == 0) return;

        // 2. Calcular la contribución de vida total
        float totalHealthValue = 0f;
        int totalPlants = allPlants.Length;
        float maxContributionPerPlant = 1f / totalPlants;

        foreach (Plant plant in allPlants)
        {
            switch (plant.healthLevel)
            {
                case 3: totalHealthValue += maxContributionPerPlant; break;
                case 2: totalHealthValue += maxContributionPerPlant * 0.5f; break;
                case 1: totalHealthValue += 0f; break;
            }
        }

        // 3. ¡LA PARTE MÁS IMPORTANTE!
        //    En lugar de tocar la barra, solo guardamos el objetivo.
        targetFillAmount = totalHealthValue; // <-- MODIFICADO

        // 4. Comprobar Game Over (la lógica sí es instantánea)
        if (targetFillAmount < 0.01f)
        {
            Debug.Log("¡EL PLANETA HA MUERTO! (Game Over)");
            // La barra seguirá animándose hasta el 0%
        }
    }


    // --- ¡LA MAGIA OCURRE AQUÍ! ---

    // La función Update() se llama en cada fotograma
    void Update()
    {
        // Si el valor visual actual (currentFillAmount)
        // no es igual al valor objetivo (targetFillAmount)...
        if (currentFillAmount != targetFillAmount)
        {
            // ...movemos SUAVEMENTE el valor actual hacia el objetivo.
            currentFillAmount = Mathf.MoveTowards(
                currentFillAmount,        // Desde
                targetFillAmount,         // Hacia
                animationSpeed * Time.deltaTime // A esta velocidad
            );

            // Ahora, actualizamos la UI (barra y texto) con
            // este valor "en movimiento".
            UpdateVisuals(currentFillAmount);
        }
    }

    // Función separada para actualizar la barra y el texto
    void UpdateVisuals(float value)
    {
        // 1. Actualizar la barra de vida (Image)
        if (planetHealthBar != null)
        {
            planetHealthBar.fillAmount = value;
        }

        // 2. Actualizar el texto del porcentaje (Text)
        if (healthPercentageText != null)
        {
            healthPercentageText.text = (value * 100f).ToString("F0") + "%";
        }
    }
}