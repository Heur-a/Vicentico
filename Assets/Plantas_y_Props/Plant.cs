using UnityEngine;
using System.Collections; // Necesario para eventos y corrutinas

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

    [Header("Vida automática (tu parte)")]
    [Tooltip("¿Usar el sistema automático de vida de 30s?")]
    public bool useAutoLife = true;

    [Tooltip("Tiempo total de vida en segundos.")]
    public float totalLifeTime = 30f; // 30s

    [Tooltip("Tiempo en verde (sana).")]
    public float timeGreen = 15f;     // 15s verde

    [Tooltip("Tiempo en rojo antes de morir.")]
    public float timeRed = 7f;        // 7s roja

    [Tooltip("Duración de la animación de encogerse antes de desaparecer.")]
    public float shrinkDuration = 1.5f;

    // --- Eventos ---
    // Un evento es como una señal de radio.
    // Cuando la planta cambie de estado, avisará al PlanetManager.
    public static event System.Action<Plant> OnPlantStateChanged;

    // --- Estado Privado ---
    private int _previousHealthLevel; // Para detectar cambios
    private bool isDying = false;     // Para no repetir la animación de muerte

    void Start()
    {
        // Al empezar, guardamos el estado inicial y actualizamos el visual
        _previousHealthLevel = healthLevel;
        UpdateVisuals();

        // TU PARTE: iniciar la vida automática si está activada
        if (useAutoLife)
        {
            StartCoroutine(LifeCycleRoutine());
        }
    }

    // ---------- TU LÓGICA: ciclo de vida automático ----------
    IEnumerator LifeCycleRoutine()
    {
        // Seguridad por si se ponen tiempos raros
        float orangeTime = Mathf.Max(0f, totalLifeTime - timeGreen - timeRed);

        // 🟢 Fase verde: healthLevel = 3
        // (ya empieza en 3, solo esperamos)
        yield return new WaitForSeconds(timeGreen);

        // Pasar a naranja (healthLevel = 2)
        TakeDamage(); // 3 -> 2

        // 🟠 Fase naranja
        if (orangeTime > 0f)
            yield return new WaitForSeconds(orangeTime);

        // Pasar a rojo (healthLevel = 1)
        TakeDamage(); // 2 -> 1

        // 🔴 Fase roja
        if (timeRed > 0f)
            yield return new WaitForSeconds(timeRed);

        // 💀 Animación de encogerse y desaparecer
        StartCoroutine(ShrinkAndDie());
    }

    IEnumerator ShrinkAndDie()
    {
        if (isDying) yield break;
        isDying = true;

        // Escalamos el objeto visual (el renderer) o, si no, este objeto
        Transform target = plantRenderer != null ? plantRenderer.transform : transform;
        Vector3 startScale = target.localScale;
        Vector3 endScale = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // Por si acaso no estaba ya en nivel 1, lo marcamos como muerto
        if (healthLevel > 1)
        {
            healthLevel = 1;
            NotifyStateChange();
        }

        Destroy(gameObject);
    }
    // ---------------------------------------------------------

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

    // --- Herramientas para testear con el ratón (de vuestra práctica) ---

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
