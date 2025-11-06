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
    
    public int currentFrame = 0;
    public int fps = 24;
    [SerializeField] private float seconds = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        chorro.Stop();
    }

    void Update()
    {
        if (anim == null || chorro == null)
            return;

        // Convertir el tiempo normalizado (0–1) a frame
        if (estaOrinando)
        {
            currentFrame = ContadorFrames();
        }
        
        // Controlar el chorro según el frame
        if (estaOrinando && currentFrame >= frameInicio && currentFrame < frameFin)
        {
            chorro.Play();
            estaOrinando = true;
        }
        else if (estaOrinando && currentFrame >= frameFin)
        {
            chorro.Stop();
            estaOrinando = false;
            currentFrame = 0;
            seconds = 0;
        }
        
        
    }

    public void empezarMear()
    {
        anim.SetTrigger("TriggerMear");
        estaOrinando = true;
    }

    int ContadorFrames()
    {
        seconds += Time.deltaTime;
        return Mathf.FloorToInt(seconds * fps) % (frameFin + 1);
    }
}
