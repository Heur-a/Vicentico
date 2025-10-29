using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private RawImage videoDisplay; // Donde se muestra el video
    [SerializeField] private Button playPauseButton; // Botón de play/pause
    [SerializeField] private Image playPauseImage; // Imagen del botón (cambia entre play y pause)
    [SerializeField] private Slider progressSlider; // Barra de progreso
    [SerializeField] private Text timeText; // Texto del tiempo restante
    
    [Header("Imágenes del Botón")]
    [SerializeField] private Sprite playSprite; // Imagen de PLAY (▶)
    [SerializeField] private Sprite pauseSprite; // Imagen de PAUSE (❚❚)
    
    [Header("Configuración")]
    [SerializeField] private VideoClip videoClip; // El video a reproducir
    [SerializeField] private GameObject canvasVideo; // Canvas que contiene el reproductor
    [SerializeField] private GameObject canvasMenu; // Canvas del menú principal
    [SerializeField] private AudioSource audioMenu; // AudioSource del menú (opcional, para silenciarlo)
    
    // Componentes internos
    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private RenderTexture renderTexture;
    private bool isDraggingSlider = false;
    private float volumenOriginalMenu = 1f; // Para guardar el volumen original del menú

    void Start()
    {
        Debug.Log("🎬 VideoPlayerController - Start()");
        
        // Crear y configurar el VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        
        // Asignar el video
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            Debug.Log("✅ Video clip asignado: " + videoClip.name);
        }
        else
        {
            Debug.LogError("❌ NO hay video clip asignado");
        }
        
        // Verificar referencias de UI
        Debug.Log("📋 Verificando referencias de UI:");
        Debug.Log($"   - videoDisplay: {(videoDisplay != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - playPauseButton: {(playPauseButton != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - playPauseImage: {(playPauseImage != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - progressSlider: {(progressSlider != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - timeText: {(timeText != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - playSprite: {(playSprite != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - pauseSprite: {(pauseSprite != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - canvasVideo: {(canvasVideo != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - canvasMenu: {(canvasMenu != null ? "✅" : "❌ NULL")}");
        Debug.Log($"   - audioMenu: {(audioMenu != null ? "✅ (opcional)" : "⚠️ NULL (opcional)")}");
        
        // Guardar volumen original del menú
        if (audioMenu != null)
        {
            volumenOriginalMenu = audioMenu.volume;
        }
        
        // Configurar el slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
            progressSlider.value = 0;
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        // Configurar el botón
        if (playPauseButton != null)
        {
            playPauseButton.onClick.AddListener(TogglePlayPause);
        }
        
        // Eventos del video
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // Ocultar el canvas del video al inicio
        if (canvasVideo != null)
        {
            canvasVideo.SetActive(false);
        }
        
        // Preparar el video
        videoPlayer.Prepare();
        
        // Actualizar el botón al estado inicial
        UpdatePlayPauseButton();
    }
    
    // Función para ABRIR el reproductor de video
    public void AbrirVideo()
    {
        Debug.Log("🎬 Abriendo reproductor de video");
        
        // Silenciar el audio del menú
        if (audioMenu != null)
        {
            audioMenu.Pause();
            Debug.Log("🔇 Audio del menú pausado");
        }
        
        // Ocultar el menú
        if (canvasMenu != null)
        {
            canvasMenu.SetActive(false);
            Debug.Log("✅ Menú ocultado");
        }
        
        // Mostrar el canvas del video
        if (canvasVideo != null)
        {
            canvasVideo.SetActive(true);
            Debug.Log("✅ Canvas de video mostrado");
            
            // Verificar si los elementos están activos
            if (playPauseButton != null)
                Debug.Log($"   - Botón Play/Pause activo: {playPauseButton.gameObject.activeSelf}");
            if (timeText != null)
                Debug.Log($"   - Texto tiempo activo: {timeText.gameObject.activeSelf}");
        }
        
        // Reproducir automáticamente
        Play();
    }
    
    // Función para CERRAR el reproductor de video
    public void CerrarVideo()
    {
        Debug.Log("❌ Cerrando reproductor de video");
        
        // Pausar el video
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            Pause();
        }
        
        // Ocultar el canvas del video
        if (canvasVideo != null)
        {
            canvasVideo.SetActive(false);
        }
        
        // Mostrar el menú
        if (canvasMenu != null)
        {
            canvasMenu.SetActive(true);
        }
        
        // Reanudar el audio del menú
        if (audioMenu != null)
        {
            audioMenu.UnPause();
            Debug.Log("🔊 Audio del menú reanudado");
        }
    }

    void Update()
    {
        // Actualizar slider y tiempo mientras el video se reproduce
        if (videoPlayer != null && videoPlayer.isPlaying && !isDraggingSlider)
        {
            if (videoPlayer.frameCount > 0)
            {
                // Actualizar slider
                float progress = (float)(videoPlayer.time / videoPlayer.length);
                if (progressSlider != null)
                {
                    progressSlider.value = progress;
                }
                
                // Actualizar texto de tiempo
                UpdateTimeText();
            }
        }
    }

    // Función para el botón Play/Pause
    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    // Reproducir el video
    public void Play()
    {
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            // Crear RenderTexture si no existe
            if (renderTexture == null && videoDisplay != null)
            {
                renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0);
                videoPlayer.targetTexture = renderTexture;
                videoDisplay.texture = renderTexture;
            }
            
            videoPlayer.Play();
            UpdatePlayPauseButton();
            Debug.Log("▶ Video reproduciendo");
        }
    }

    // Pausar el video
    public void Pause()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            UpdatePlayPauseButton();
            Debug.Log("⏸ Video pausado");
        }
    }

    // Actualizar la imagen del botón play/pause
    private void UpdatePlayPauseButton()
    {
        if (playPauseImage != null)
        {
            if (videoPlayer.isPlaying)
            {
                // Mostrar imagen de PAUSE
                playPauseImage.sprite = pauseSprite;
            }
            else
            {
                // Mostrar imagen de PLAY
                playPauseImage.sprite = playSprite;
            }
        }
    }

    // Actualizar el texto del tiempo
    private void UpdateTimeText()
    {
        if (timeText != null && videoPlayer != null)
        {
            double timeRemaining = videoPlayer.length - videoPlayer.time;
            timeText.text = FormatTime(timeRemaining);
        }
    }

    // Formatear tiempo en formato mm:ss
    private string FormatTime(double seconds)
    {
        int minutes = Mathf.FloorToInt((float)seconds / 60f);
        int secs = Mathf.FloorToInt((float)seconds % 60f);
        return string.Format("{0}:{1:00}", minutes, secs);
    }

    // Cuando el usuario mueve el slider
    private void OnSliderChanged(float value)
    {
        if (isDraggingSlider && videoPlayer != null && videoPlayer.isPrepared)
        {
            videoPlayer.time = value * videoPlayer.length;
            UpdateTimeText();
        }
    }

    // Llamar cuando el usuario empieza a arrastrar el slider
    public void OnSliderDragStart()
    {
        isDraggingSlider = true;
    }

    // Llamar cuando el usuario suelta el slider
    public void OnSliderDragEnd()
    {
        isDraggingSlider = false;
    }

    // Cuando el video está preparado
    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("✅ Video preparado. Duración: " + FormatTime(vp.length));
        UpdateTimeText();
    }

    // Cuando el video termina
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("🏁 Video terminado");
        UpdatePlayPauseButton();
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}