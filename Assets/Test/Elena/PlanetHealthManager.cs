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
    public float animationSpeed = 0.5f;

    [Header("Pantalla de Game Over")]
    public GameObject gameOverScreen;

    // --- Variables Privadas para la Animación ---
    // (AQUÍ ESTABA EL ERROR: líneas duplicadas)
    private float targetFillAmount = 1f;   // El valor al que QUEREMOS ir
    private float currentFillAmount = 1f;  // El valor VISUAL actual de la barra
    private bool isDead = false;           // Para controlar que la muerte solo ocurra una vez

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
        currentFillAmount = targetFillAmount;
        UpdateVisuals(currentFillAmount);
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
    private void UpdatePlanetHealthLogic()
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
        targetFillAmount = totalHealthValue;

       // 4. Comprobar Game Over (¡Lógica actualizada!)
        //    Solo comprobamos si la vida objetivo es 0 Y no estamos ya muertos
        if (targetFillAmount < 0.01f && !isDead)
        {
            isDead = true; // <-- Marcamos que estamos muertos
            // La pantalla aún no se muestra, esperamos a la animación
        }
    }

    // --- ¡LA MAGIA OCURRE AQUÍ! ---

// La función Update() se llama en cada fotograma
    void Update()
    {
        // 1. Animamos la barra si es necesario
        if (currentFillAmount != targetFillAmount)
        {
            currentFillAmount = Mathf.MoveTowards(
                currentFillAmount,        // Desde
                targetFillAmount,         // Hacia
                animationSpeed * Time.deltaTime // A esta velocidad
            );

            // Actualizamos la UI (barra y texto) con este valor "en movimiento"
            UpdateVisuals(currentFillAmount);
        }

        // 2. Comprobamos la condición de muerte (¡MOVIDO FUERA!)
        //    Si el planeta está marcado como "muerto"
        //    Y la animación de la barra HA TERMINADO (ha llegado a 0)
        //    Usamos Mathf.Approximately para comparar floats de forma segura
        if (isDead && Mathf.Approximately(currentFillAmount, targetFillAmount))
        {
            ShowDeathScreen();
        }
    }
    // Esta función se encarga de mostrar la pantalla y pausar el juego
    void ShowDeathScreen()
    {
        // Activar la pantalla de Game Over
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            
            // Forzar que la imagen sea visible si tiene alpha bajo
            UnityEngine.UI.Image image = gameOverScreen.GetComponent<UnityEngine.UI.Image>();
            if (image != null && image.color.a < 0.1f)
            {
                Color newColor = image.color;
                newColor.a = 1f;
                image.color = newColor;
            }
            
            // Forzar CanvasGroup si existe
            CanvasGroup canvasGroup = gameOverScreen.GetComponent<CanvasGroup>();
            if (canvasGroup != null && canvasGroup.alpha < 0.1f)
            {
                canvasGroup.alpha = 1f;
            }
        }

        // Pausar el juego
        Time.timeScale = 0f;
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