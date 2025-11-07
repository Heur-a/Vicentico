using System.Collections;
using UnityEngine;
using Vuforia;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Crecimiento_Plantas : MonoBehaviour
{[Header("Vuforia")]
    public ObserverBehaviour observer;
    [Tooltip("Si está activo, la planta crece/encoge con el tracking. Desactívalo si la disparas desde SeedGrowController.")]
    public bool autoOnTracking = true;

    [Header("Persistencia")]
    [Tooltip("Guarda si la planta ya creció. Si no pones clave, usará el nombre del target o el nombre del GameObject.")]
    public bool persistGrowth = true;
    public string saveKeyOverride = ""; // opcional

    [Header("Plant parts")]
    public Transform plantRoot;
    public Transform plantMesh;     // Debe ser el objeto que tiene el MeshRenderer (p.ej. SP_Plant06)

    [Header("Growth")]
    public float growDuration = 2f;
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Si está activo, solo escala el eje Y (X y Z permanecen en 1).")]
    public bool scaleOnlyY = true;

    [Header("Bloom / Floración (opcional)")]
    public Animator bloomAnimator;
    public string bloomTrigger = "Bloom";
    public SkinnedMeshRenderer bloomSMR;
    public string bloomBlendshapeName = "Bloom";
    public float bloomDuration = 0.8f;

    [Header("Postprocesado (URP)")]
    public Volume growthVolume;
    [Range(0f, 1f)] public float volumeTargetWeight = 0.85f;
    public float volumeFadeOutTime = 0.35f;

    [Header("Partículas")]
    public ParticleSystem growDust;
    public ParticleSystem bloomBurst;
    public float maxDustRate = 35f;

    // ---- Interno ----
    Coroutine routine;
    int bloomBlendshapeIndex = -1;
    bool hasGrown = false;      // estado persistente
    string saveKey;             // clave para PlayerPrefs

    void Awake()
    {
        if (observer == null) observer = GetComponent<ObserverBehaviour>();
        if (plantMesh == null) plantMesh = transform;

        // ---- Clave de guardado ----
        saveKey = BuildSaveKey();

        // ---- Cargar estado ----
        if (persistGrowth)
            hasGrown = PlayerPrefs.GetInt(saveKey, 0) == 1;

        // ---- Estado inicial según lo guardado ----
        if (hasGrown)
        {
            // Si ya creció alguna vez, mostrarla crecida sin animación
            ShowInstantGrown();
        }
        else
        {
            // Si aún no ha crecido, oculta y pon escala inicial
            plantMesh.localScale = StartScale();
            plantMesh.gameObject.SetActive(false);
            SetRenderersEnabled(false);
            if (growthVolume) growthVolume.weight = 0f;
        }

        if (bloomSMR && !string.IsNullOrEmpty(bloomBlendshapeName) && bloomSMR.sharedMesh != null)
            bloomBlendshapeIndex = bloomSMR.sharedMesh.GetBlendShapeIndex(bloomBlendshapeName);

        if (growDust) { growDust.Clear(true); growDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        if (bloomBurst) bloomBurst.Clear(true);

        if (observer != null && autoOnTracking)
            observer.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDestroy()
    {
        if (observer != null && autoOnTracking)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    // ---------- API pública ----------
    /// Llama esto si quieres forzar que “quede guardado” como crecida (por ejemplo desde fuera).
    public void MarkAsGrownAndSave()
    {
        hasGrown = true;
        SaveState();
    }

    public void ResetHidden()
    {
        if (routine != null) StopCoroutine(routine);

        if (hasGrown)
        {
            // Si ya creció, muéstrala crecida (no la resetees)
            ShowInstantGrown();
            return;
        }

        // Si NO ha crecido, ocúltala
        plantMesh.localScale = StartScale();
        plantMesh.gameObject.SetActive(false);
        SetRenderersEnabled(false);

        if (growthVolume) growthVolume.weight = 0f;

        if (growDust)
        {
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (bloomSMR && bloomBlendshapeIndex >= 0)
            bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, 0f);
    }

    public void BeginGrowthNow()
    {
        if (hasGrown)
        {
            // Ya creció: asegúrate de que se vea crecida
            ShowInstantGrown();
            return;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(GrowAndBloom());
    }
    // ----------------------------------

    // Automático (solo si autoOnTracking = true)
    void OnTargetStatusChanged(ObserverBehaviour ob, TargetStatus status)
    {
        bool visible = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (visible)
        {
            if (hasGrown)
            {
                // Si ya creció, aparece crecida al instante
                ShowInstantGrown();
                return;
            }

            if (autoOnTracking)
            {
                if (routine != null) StopCoroutine(routine);
                routine = StartCoroutine(GrowAndBloom());
            }
        }
        else
        {
            if (hasGrown)
            {
                // IMPORTANTE: si ya creció, NO hagas shrink; solo oculta visualmente.
                HideVisualsOnly();
            }
            else
            {
                // Si no ha crecido, sí puedes shrinkear/ocultar normal
                if (autoOnTracking)
                {
                    if (routine != null) StopCoroutine(routine);
                    routine = StartCoroutine(Shrink());
                }
                else
                {
                    HideVisualsOnly();
                }
            }
        }
    }

    IEnumerator GrowAndBloom()
    {
        // Mostrar y habilitar render
        if (!plantMesh.gameObject.activeSelf) plantMesh.gameObject.SetActive(true);
        SetRenderersEnabled(true);
        yield return null; // deja procesar el activado un frame

        // Polvo ON
        if (growDust)
        {
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Play();
        }

        float t = 0f;
        Vector3 start = StartScale();
        Vector3 end = EndScale();

        while (t < growDuration)
        {
            t += Time.deltaTime;
            float k = growCurve.Evaluate(Mathf.Clamp01(t / growDuration));
            plantMesh.localScale = Vector3.Lerp(start, end, k);

            if (growthVolume)
                growthVolume.weight = Mathf.Lerp(0f, volumeTargetWeight, k);

            if (growDust)
            {
                var em = growDust.emission;
                em.rateOverTime = Mathf.Lerp(0f, maxDustRate, k);
            }

            yield return null;
        }

        plantMesh.localScale = end;
        if (growthVolume) growthVolume.weight = volumeTargetWeight;

        // Floración opcional
        if (bloomAnimator && !string.IsNullOrEmpty(bloomTrigger))
            bloomAnimator.SetTrigger(bloomTrigger);

        if (bloomSMR && bloomBlendshapeIndex >= 0)
        {
            float b = 0f;
            while (b < bloomDuration)
            {
                b += Time.deltaTime;
                float kk = Mathf.Clamp01(b / bloomDuration);
                bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, Mathf.Lerp(0f, 100f, kk));
                yield return null;
            }
            bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, 100f);
        }

        if (growDust)
        {
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (bloomBurst) bloomBurst.Play();

        // ✅ Marca y guarda el estado como “ya creció”
        hasGrown = true;
        SaveState();
    }

    IEnumerator Shrink()
    {
        float shrinkDuration = 0.3f;
        float t = 0f;
        Vector3 from = plantMesh.localScale;
        Vector3 to = StartScale();
        float startWeight = growthVolume ? growthVolume.weight : 0f;

        if (growDust)
        {
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / shrinkDuration);
            plantMesh.localScale = Vector3.Lerp(from, to, k);

            if (growthVolume)
            {
                float f = Mathf.Clamp01(t / Mathf.Max(0.0001f, volumeFadeOutTime));
                growthVolume.weight = Mathf.Lerp(startWeight, 0f, f);
            }
            yield return null;
        }

        plantMesh.localScale = to;

        if (growthVolume) growthVolume.weight = 0f;
        if (bloomSMR && bloomBlendshapeIndex >= 0) bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, 0f);

        plantMesh.gameObject.SetActive(false);
        SetRenderersEnabled(false);
    }

    // ------- Helpers / utilidades -------
    Vector3 StartScale() => scaleOnlyY ? new Vector3(1f, 0f, 1f) : Vector3.zero;
    Vector3 EndScale()   => Vector3.one;

    void SetRenderersEnabled(bool enabled)
    {
        if (!plantMesh) return;
        foreach (var r in plantMesh.GetComponentsInChildren<Renderer>(true))
            r.enabled = enabled;
    }

    void ShowInstantGrown()
    {
        plantMesh.localScale = EndScale();
        plantMesh.gameObject.SetActive(true);
        SetRenderersEnabled(true);
        if (growthVolume) growthVolume.weight = volumeTargetWeight;
        if (bloomSMR && bloomBlendshapeIndex >= 0) bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, 100f);
    }

    void HideVisualsOnly()
    {
        // Oculta render y objeto, sin cambiar escala objetivo ni estado
        plantMesh.gameObject.SetActive(false);
        SetRenderersEnabled(false);
        // (opcional) puedes bajar el volume aquí
        if (growthVolume) growthVolume.weight = 0f;
    }

    string BuildSaveKey()
    {
        if (!persistGrowth) return "";

        if (!string.IsNullOrEmpty(saveKeyOverride))
            return saveKeyOverride;

        // Preferimos el nombre del target si existe
        if (observer != null && observer.TargetName != null && observer.TargetName.Length > 0)
            return "PlantGrown_" + observer.TargetName;

        // Fallback al nombre del GameObject
        return "PlantGrown_" + gameObject.scene.name + "_" + gameObject.name;
    }

    void SaveState()
    {
        if (!persistGrowth || string.IsNullOrEmpty(saveKey)) return;
        PlayerPrefs.SetInt(saveKey, hasGrown ? 1 : 0);
        PlayerPrefs.Save();
    }
}
