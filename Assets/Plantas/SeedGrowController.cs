using UnityEngine;
using UnityEngine.Events;
using Vuforia;

public class SeedGrowController : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour observer;               // ImageTarget (se autollenará si lo dejas vacío)

    [Header("Referencias")]
    public GameObject seedObject;                    // Semillas (activar/ocultar)
    public Crecimiento_Plantas growth;               // Componente de crecimiento de la planta (mismo ImageTarget)

    [Header("Opciones")]
    public bool showSeedsOnFound = true;             // Mostrar semillas al detectar el target
    public bool hideAllOnLost = true;                // Ocultar TODO al perder el target
    public bool requireTargetVisibleToWater = true;  // Bloquear "regar" si el target no está visible

    [Header("Feedback opcional")]
    public ParticleSystem waterFX;                   // Partículas de agua (opcional)
    public AudioSource waterSFX;                     // Sonido de riego (opcional)

    [Header("Eventos")]
    public UnityEvent onTargetFound;                 // Se dispara al detectar el target
    public UnityEvent onTargetLost;                  // Se dispara al perder el target
    public UnityEvent onWaterStart;                  // Justo cuando se riega
    public UnityEvent onGrowthBegin;                 // Cuando empieza a crecer la planta
    public UnityEvent onResetAll;                    // Cuando hacemos reset total

    // --- Estado interno ---
    bool watered = false;        // ya regado
    bool targetVisible = false;  // estado de tracking

    void Awake()
    {
        // Autoconfigura observer si está vacío
        if (observer == null) observer = GetComponent<ObserverBehaviour>();
        if (observer != null) observer.OnTargetStatusChanged += OnTargetStatusChanged;

        // Semillas visibles al inicio (si así lo quieres)
        if (seedObject != null) seedObject.SetActive(true);

        // Evita competencia con el auto del crecimiento
        if (growth != null)
        {
            growth.autoOnTracking = false;
            growth.ResetHidden(); // planta oculta y preparada
        }
    }

    void OnDestroy()
    {
        if (observer != null) observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    // ------------------- Callbacks de Vuforia -------------------
    void OnTargetStatusChanged(ObserverBehaviour ob, TargetStatus status)
    {
        targetVisible = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (targetVisible)
        {
            onTargetFound?.Invoke();
            watered = false;

            if (showSeedsOnFound && seedObject != null)
                seedObject.SetActive(true);

            if (growth != null)
                growth.ResetHidden(); // planta oculta, escala mínima, volume 0, etc.
        }
        else
        {
            onTargetLost?.Invoke();

            if (hideAllOnLost)
            {
                if (seedObject != null) seedObject.SetActive(false);
                if (growth != null) growth.ResetHidden();
                watered = false;
            }
        }
    }
    // ------------------------------------------------------------

    /// <summary>
    /// Llama esto desde un botón UI o evento AR para "regar".
    /// </summary>
    public void WaterSeeds()
    {
        Debug.Log($"[SeedGrow] Water pressed | growth={(growth ? "OK" : "NULL")} | targetVisible={targetVisible}");
        // Bloqueos de seguridad
        if (growth == null) return;
        if (watered) return;
        if (requireTargetVisibleToWater && !targetVisible) return;

        watered = true;

        // Feedback inmediato
        onWaterStart?.Invoke();
        if (waterFX != null) { waterFX.Clear(true); waterFX.Play(); }
        if (waterSFX != null) waterSFX.Play();

        // Oculta semillas
        if (seedObject != null)
            seedObject.SetActive(false);

        // Inicia el crecimiento real (partículas/bloom lo gestiona Crecimiento_Plantas)
        onGrowthBegin?.Invoke();
        growth.BeginGrowthNow();
    }

    /// <summary>
    /// Resetea todo a estado inicial: semillas visibles, planta oculta.
    /// Útil para botones de reinicio o cambios de escena.
    /// </summary>
    public void ResetAll()
    {
        watered = false;

        if (seedObject != null)
            seedObject.SetActive(true);

        if (growth != null)
            growth.ResetHidden();

        // Apaga feedback si estaba activo
        if (waterFX != null) waterFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (waterSFX != null) waterSFX.Stop();

        onResetAll?.Invoke();
    }

    /// <summary>
    /// Muestra u oculta manualmente las semillas (por si lo necesitas desde otros scripts).
    /// </summary>
    public void SetSeedsVisible(bool visible)
    {
        if (seedObject != null) seedObject.SetActive(visible);
    }
}
