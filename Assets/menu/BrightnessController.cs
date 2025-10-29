using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el brillo del menú mediante un slider y un overlay oscuro
/// </summary>
public class BrightnessController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image brightnessOverlay; // Panel/Imagen de overlay negro para oscurecer
    [SerializeField] private Slider brightnessSlider; // Slider para ajustar el brillo
    
    [Header("Configuración de Brillo")]
    [SerializeField] private float brightnessInicial = 1f; // Brillo inicial (1 = sin oscurecer, 0 = completamente oscuro)
    [SerializeField] private Color colorOverlay = Color.black; // Color del overlay (negro por defecto)
    
    void Start()
    {
        // Configurar el slider
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f; // Completamente oscuro
            brightnessSlider.maxValue = 1f; // Sin oscurecer
            brightnessSlider.value = brightnessInicial;
            
            // Suscribirse al evento del slider
            brightnessSlider.onValueChanged.AddListener(CambiarBrillo);
            
            Debug.Log("✅ Slider de brillo configurado");
        }
        else
        {
            Debug.LogError("❌ Brightness Slider no asignado en el Inspector");
        }
        
        // Configurar el overlay
        if (brightnessOverlay != null)
        {
            // Configurar el color del overlay
            brightnessOverlay.color = colorOverlay;
            
            // Aplicar el brillo inicial
            CambiarBrillo(brightnessInicial);
            
            Debug.Log("✅ Brightness Overlay configurado");
        }
        else
        {
            Debug.LogError("❌ Brightness Overlay no asignado en el Inspector");
        }
    }
    
    /// <summary>
    /// Cambia el brillo del menú ajustando la transparencia del overlay
    /// </summary>
    /// <param name="valor">Valor de brillo (0 = oscuro, 1 = claro)</param>
    public void CambiarBrillo(float valor)
    {
        if (brightnessOverlay != null)
        {
            // Invertir el valor: cuando el slider está en 1 (máximo brillo), el overlay debe ser transparente (alpha = 0)
            // cuando el slider está en 0 (mínimo brillo), el overlay debe ser opaco (alpha = 1)
            float alpha = 1f - valor;
            
            Color nuevoColor = brightnessOverlay.color;
            nuevoColor.a = alpha;
            brightnessOverlay.color = nuevoColor;
            
            Debug.Log($"🔆 Brillo ajustado: {valor:F2} (Overlay alpha: {alpha:F2})");
        }
    }
    
    /// <summary>
    /// Establece el brillo a un valor específico
    /// </summary>
    /// <param name="valor">Valor de brillo entre 0 y 1</param>
    public void EstablecerBrillo(float valor)
    {
        valor = Mathf.Clamp01(valor); // Asegurar que esté entre 0 y 1
        
        if (brightnessSlider != null)
        {
            brightnessSlider.value = valor;
        }
        
        CambiarBrillo(valor);
    }
    
    /// <summary>
    /// Aumenta el brillo
    /// </summary>
    public void AumentarBrillo()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.value = Mathf.Clamp01(brightnessSlider.value + 0.1f);
        }
    }
    
    /// <summary>
    /// Disminuye el brillo
    /// </summary>
    public void DisminuirBrillo()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.value = Mathf.Clamp01(brightnessSlider.value - 0.1f);
        }
    }
    
    /// <summary>
    /// Resetea el brillo al valor inicial
    /// </summary>
    public void ResetearBrillo()
    {
        EstablecerBrillo(brightnessInicial);
    }
}

