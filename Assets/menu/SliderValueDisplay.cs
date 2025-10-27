using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValueDisplay : MonoBehaviour
{
    [Header("Configuración")]
    public Slider targetSlider;
    public TextMeshProUGUI displayText;
    
    void Start()
    {
        Debug.Log("=== Iniciando SliderValueDisplay en: " + gameObject.name + " ===");
        
        // Diagnóstico detallado
        if (targetSlider == null)
        {
            Debug.LogError("❌ TargetSlider es NULL en " + gameObject.name);
            
            // Intentar encontrar automáticamente
            targetSlider = FindSliderInParents();
            if (targetSlider != null)
                Debug.Log("✅ Slider encontrado automáticamente: " + targetSlider.name);
        }
        else
        {
            Debug.Log("✅ TargetSlider asignado: " + targetSlider.name);
        }
        
        if (displayText == null)
        {
            Debug.LogError("❌ DisplayText es NULL en " + gameObject.name);
            
            // Intentar encontrar automáticamente
            displayText = GetComponent<TextMeshProUGUI>();
            if (displayText != null)
                Debug.Log("✅ TextMeshProUGUI encontrado automáticamente en el mismo objeto");
        }
        else
        {
            Debug.Log("✅ DisplayText asignado: " + displayText.name);
        }
        
        // Configurar si tenemos ambas referencias
        if (targetSlider != null && displayText != null)
        {
            targetSlider.onValueChanged.AddListener(UpdateDisplay);
            UpdateDisplay(targetSlider.value);
            Debug.Log("✅ Configuración completada exitosamente para: " + gameObject.name);
        }
        else
        {
            Debug.LogError("🚫 No se pudo completar la configuración para: " + gameObject.name);
            Debug.Log("   - TargetSlider: " + (targetSlider != null ? "ASIGNADO" : "FALTANTE"));
            Debug.Log("   - DisplayText: " + (displayText != null ? "ASIGNADO" : "FALTANTE"));
        }
    }
    
    private Slider FindSliderInParents()
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            Slider foundSlider = parent.GetComponent<Slider>();
            if (foundSlider != null)
                return foundSlider;
            parent = parent.parent;
        }
        return null;
    }
    
    void UpdateDisplay(float value)
    {
        if (displayText != null)
        {
            displayText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
}