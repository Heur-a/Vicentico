using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias a los Canvas")]
    [SerializeField] private GameObject canvasNormal;
    [SerializeField] private GameObject canvasInstrucciones;
    [SerializeField] private GameObject canvasManipulacion; // Canvas de manipulación
    
    [Header("Referencias de Manipulación")]
    [SerializeField] private ManipularObjeto scriptManipulacion; // Script que maneja la manipulación
    [SerializeField] private GameObject objetoFBX; // Objeto FBX a manipular
    [SerializeField] private Toggle toggleManipulacion; // Toggle opcional
    
    void Start()
    {
        // Estado inicial: Canvas normal visible, otros ocultos
        MostrarCanvasNormal();
        
        // Asegurarse de que el canvas de manipulación esté cerrado al inicio
        if (canvasManipulacion != null)
            canvasManipulacion.SetActive(false);
            
        // Asegurarse de que el objeto FBX esté oculto al inicio
        if (objetoFBX != null)
            objetoFBX.SetActive(false);
    }
    
    // Llamado por el botón "Instrucciones" en el Canvas normal
    public void MostrarInstrucciones()
    {
        canvasNormal.SetActive(false);
        canvasInstrucciones.SetActive(true);
    }
    
    // Llamado por la "X" (botón cerrar) en el Canvas de instrucciones
    public void MostrarCanvasNormal()
    {
        canvasInstrucciones.SetActive(false);
        canvasNormal.SetActive(true);
    }
    
    // Llamado por el botón "Escalar/Rotar" - Abre el canvas de manipulación
    public void AbrirManipulacion()
    {
        Debug.Log("=== ABRIENDO MANIPULACIÓN ===");
        
        // Ocultar el menú principal
        if (canvasNormal != null)
        {
            canvasNormal.SetActive(false);
            Debug.Log("✅ Canvas Normal ocultado");
        }
        else
        {
            Debug.LogError("❌ Canvas Normal es NULL");
        }
        
        // Mostrar el canvas de manipulación
        if (canvasManipulacion != null)
        {
            canvasManipulacion.SetActive(true);
            Debug.Log("✅ Canvas Manipulación mostrado: " + canvasManipulacion.name);
            Debug.Log("   - Estado activo: " + canvasManipulacion.activeSelf);
        }
        else
        {
            Debug.LogError("❌ Canvas Manipulación es NULL - ASÍGNALO EN EL INSPECTOR");
        }
        
        // Mostrar el objeto FBX
        if (objetoFBX != null)
        {
            objetoFBX.SetActive(true);
            Debug.Log("✅ Objeto FBX mostrado: " + objetoFBX.name);
        }
        else
        {
            Debug.LogWarning("⚠️ Objeto FBX es NULL");
        }
        
        // Marcar el toggle
        if (toggleManipulacion != null)
        {
            toggleManipulacion.isOn = true;
            Debug.Log("✅ Toggle marcado");
        }
        
        // Activar la manipulación
        if (scriptManipulacion != null)
        {
            scriptManipulacion.ActivarManipulacion();
            Debug.Log("✅ Script de manipulación activado");
        }
        else
        {
            Debug.LogWarning("⚠️ Script Manipulacion es NULL");
        }
            
        Debug.Log("=== FIN ABRIR MANIPULACIÓN ===");
    }
    
    // Llamado por el botón cerrar del canvas de manipulación
    public void CerrarManipulacion()
    {
        // Ocultar el canvas de manipulación
        if (canvasManipulacion != null)
            canvasManipulacion.SetActive(false);
        
        // Ocultar el objeto FBX
        if (objetoFBX != null)
            objetoFBX.SetActive(false);
        
        // Desmarcar el toggle
        if (toggleManipulacion != null)
            toggleManipulacion.isOn = false;
        
        // Desactivar la manipulación
        if (scriptManipulacion != null)
            scriptManipulacion.DesactivarManipulacion();
        
        // Volver a mostrar el menú principal
        if (canvasNormal != null)
            canvasNormal.SetActive(true);
            
        Debug.Log("Canvas de manipulación cerrado - Menú principal visible");
    }
    
    // Alternar manipulación (opcional)
    public void ToggleManipulacion()
    {
        if (canvasManipulacion != null && canvasManipulacion.activeSelf)
        {
            CerrarManipulacion();
        }
        else
        {
            AbrirManipulacion();
        }
    }
}