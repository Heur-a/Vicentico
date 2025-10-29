using UnityEngine;
using Vuforia;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Crecimiento_Plantas : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour observer;

    [Header("Plant parts")]
    public Transform plantRoot;
    public Transform plantMesh;

    [Header("Growth")]
    public float growDuration = 2f;
    public AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
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
    public ParticleSystem growDust;    // FX_GrowDust
    public ParticleSystem bloomBurst;  // FX_BloomBurst
    public float maxDustRate = 35f;    // tope del “polvo” durante el crecimiento

    Vector3 originalScale;
    Coroutine routine;
    int bloomBlendshapeIndex = -1;

    void Awake()
    {
        if (observer == null) observer = GetComponent<ObserverBehaviour>();
        if (plantMesh == null) plantMesh = transform;

        originalScale = plantMesh.localScale;

        plantMesh.localScale = scaleOnlyY
            ? new Vector3(originalScale.x, 0.001f, originalScale.z)
            : Vector3.one * 0.001f;

        if (observer != null)
            observer.OnTargetStatusChanged += OnTargetStatusChanged;

        if (growthVolume != null) growthVolume.weight = 0f;

        if (bloomSMR != null && !string.IsNullOrEmpty(bloomBlendshapeName) && bloomSMR.sharedMesh != null)
            bloomBlendshapeIndex = bloomSMR.sharedMesh.GetBlendShapeIndex(bloomBlendshapeName);

        // Asegurar partículas apagadas al inicio
        if (growDust != null) { growDust.Clear(true); growDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        if (bloomBurst != null) { bloomBurst.Clear(true); }
    }

    void OnDestroy()
    {
        if (observer != null)
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    void OnTargetStatusChanged(ObserverBehaviour ob, TargetStatus status)
    {
        bool visible = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(visible ? GrowAndBloom() : Shrink());
    }

    IEnumerator GrowAndBloom()
    {
        // Arrancar polvo
        if (growDust != null)
        {
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Play();
        }

        float t = 0f;
        while (t < growDuration)
        {
            t += Time.deltaTime;
            float k = growCurve.Evaluate(Mathf.Clamp01(t / growDuration));

            if (scaleOnlyY)
                plantMesh.localScale = new Vector3(originalScale.x, Mathf.Lerp(0.001f, originalScale.y, k), originalScale.z);
            else
                plantMesh.localScale = Vector3.Lerp(Vector3.one * 0.001f, originalScale, k);

            if (growthVolume != null)
                growthVolume.weight = Mathf.Lerp(0f, volumeTargetWeight, k);

            if (growDust != null)
            {
                var em = growDust.emission;
                em.rateOverTime = Mathf.Lerp(0f, maxDustRate, k); // más polvo cuanto más crece
            }

            yield return null;
        }

        plantMesh.localScale = originalScale;
        if (growthVolume != null) growthVolume.weight = volumeTargetWeight;

        // Estallido de floración
        if (bloomAnimator != null && !string.IsNullOrEmpty(bloomTrigger))
            bloomAnimator.SetTrigger(bloomTrigger);

        if (bloomSMR != null && bloomBlendshapeIndex >= 0)
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

        if (growDust != null)
        {
            // Apaga el polvo suavemente
            var em = growDust.emission;
            em.rateOverTime = 0f;
            growDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (bloomBurst != null)
            bloomBurst.Play(); // un burst y se apaga solo
    }

    IEnumerator Shrink()
    {
        float shrinkDuration = 0.3f;
        float t = 0f;
        Vector3 from = plantMesh.localScale;
        Vector3 to = scaleOnlyY ? new Vector3(originalScale.x, 0.001f, originalScale.z) : Vector3.one * 0.001f;
        float startWeight = growthVolume != null ? growthVolume.weight : 0f;

        // Cortar partículas al perder tracking
        if (growDust != null)
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

            if (growthVolume != null)
            {
                float f = Mathf.Clamp01(t / Mathf.Max(0.0001f, volumeFadeOutTime));
                growthVolume.weight = Mathf.Lerp(startWeight, 0f, f);
            }
            yield return null;
        }

        plantMesh.localScale = to;
        if (growthVolume != null) growthVolume.weight = 0f;

        if (bloomSMR != null && bloomBlendshapeIndex >= 0)
            bloomSMR.SetBlendShapeWeight(bloomBlendshapeIndex, 0f);
    }
}
