using UnityEngine;

public class OrinaParticulas : MonoBehaviour
{
    [Header("Sistema de partículas")]
    public ParticleSystem chorro;

    [Header("Frames de animación")]
    public int frameInicio = 15;
    public int frameFin = 40;

    private Animator anim;
    private bool estaOrinando = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (anim == null || chorro == null)
            return;

        // Obtener el estado actual de la animación
        AnimatorStateInfo estado = anim.GetCurrentAnimatorStateInfo(0);

        // Convertir el tiempo normalizado (0–1) a frame
        int frameActual = Mathf.FloorToInt(estado.normalizedTime * estado.length * anim.runtimeAnimatorController.animationClips[0].frameRate);

        // Controlar el chorro según el frame
        if (!estaOrinando && frameActual >= frameInicio && frameActual < frameFin)
        {
            chorro.Play();
            estaOrinando = true;
        }
        else if (estaOrinando && frameActual >= frameFin)
        {
            chorro.Stop();
            estaOrinando = false;
        }
    }
}
