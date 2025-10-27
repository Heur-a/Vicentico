using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("Referencias UI")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    
    [Header("Brillo")]
    public Image brightnessOverlay; // Panel negro semitransparente
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Cargar valores guardados
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.7f);
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 0.8f);
        
        // Configurar listeners
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        brightnessSlider.onValueChanged.AddListener(ChangeBrightness);
        
        // Aplicar valores iniciales
        ChangeVolume(volumeSlider.value);
        ChangeBrightness(brightnessSlider.value);
    }
    
    public void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }
    
    public void ChangeBrightness(float brightness)
    {
        if (brightnessOverlay != null)
        {
            Color color = brightnessOverlay.color;
            color.a = 1 - brightness; // Más brillo = menos opacidad
            brightnessOverlay.color = color;
        }
        PlayerPrefs.SetFloat("Brightness", brightness);
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}