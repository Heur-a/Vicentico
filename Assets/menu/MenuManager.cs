using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class MenuManager : MonoBehaviour
{
    [Header("Vuforia Image Target")]
    [SerializeField] private ObserverBehaviour imageTarget; // Referencia al Image Target de Vuforia
    
    [Header("Configuración de posición del menú en el mundo")]
    [SerializeField] private Vector3 offsetPosicion = new Vector3(0, 0.1f, 0); // Posición relativa al Image Target
    [SerializeField] private Vector3 rotacionMenu = new Vector3(90, 0, 0); // Rotación del menú (90 grados para que mire hacia arriba)
    [SerializeField] private float escalaMenu = 0.1f; // Escala del menú en World Space - MUCHO MÁS GRANDE
    
    [Header("Referencias a los Canvas")]
    [SerializeField] private GameObject canvasNormal;
    [SerializeField] private GameObject tutorialVicentico; // Canvas Tutorial_vicentico (instrucciones)
    [SerializeField] private GameObject pageTemplate; // Template de página dentro de Tutorial_vicentico
    [SerializeField] private GameObject canvasManipulacion; // Canvas de manipulación
    
    [Header("Referencias de Manipulación")]
    [SerializeField] private ManipularObjeto scriptManipulacion; // Script que maneja la manipulación
    [SerializeField] private GameObject objetoFBX; // Objeto FBX a manipular
    [SerializeField] private Toggle toggleManipulacion; // Toggle opcional
    
    [Header("Referencias de Video")]
    [SerializeField] private VideoPlayerController videoPlayerController; // Script del reproductor de video
    [SerializeField] private GameObject canvasVideo; // Canvas del reproductor de video
    
    void Start()
    {
        // Configurar los canvas para World Space y posicionarlos como hijos del Image Target
        if (imageTarget != null)
        {
            ConfigurarCanvasWorldSpace(canvasNormal);
            ConfigurarCanvasWorldSpace(tutorialVicentico);
            ConfigurarCanvasWorldSpace(canvasManipulacion);
            
            // Configurar canvas de video en World Space ANTES de que el VideoPlayerController lo inicialice
            if (canvasVideo != null)
            {
                ConfigurarCanvasWorldSpace(canvasVideo);
                Debug.Log("✅ Canvas Video configurado en World Space");
            }
            
            Debug.Log("✅ Canvas configurados en World Space y anclados al Image Target");
        }
        else
        {
            Debug.LogError("❌ Image Target no asignado en MenuManager - Asígnalo en el Inspector");
        }
        
        // Estado inicial: Canvas normal OCULTO hasta que se detecte el Image Target
        if (canvasNormal != null)
            canvasNormal.SetActive(false);
        
        // Asegurarse de que Tutorial_vicentico esté oculto al inicio
        if (tutorialVicentico != null)
            tutorialVicentico.SetActive(false);
            
        // Asegurarse de que el page template esté oculto al inicio
        if (pageTemplate != null)
            pageTemplate.SetActive(false);
        
        // Asegurarse de que el canvas de manipulación esté cerrado al inicio
        if (canvasManipulacion != null)
            canvasManipulacion.SetActive(false);
            
        // Asegurarse de que el objeto FBX esté oculto al inicio
        if (objetoFBX != null)
            objetoFBX.SetActive(false);
            
        // Asegurarse de que el canvas de video esté oculto al inicio
        if (canvasVideo != null)
            canvasVideo.SetActive(false);
            
        // Suscribirse a los eventos de Vuforia para detectar el Image Target
        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged += OnTargetStatusChanged;
            Debug.Log("✅ Suscrito a eventos de Vuforia Image Target");
        }
    }
    
    // Configurar un canvas para World Space y hacerlo hijo del Image Target
    private void ConfigurarCanvasWorldSpace(GameObject canvasObj)
    {
        if (canvasObj == null || imageTarget == null) return;
        
        // Obtener el componente Canvas
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas != null)
        {
            // Configurar como World Space
            canvas.renderMode = RenderMode.WorldSpace;
            
            // Configurar el sorting order para asegurar que se vea
            canvas.sortingOrder = 10;
            
            // Asegurar que el canvas esté habilitado
            canvas.enabled = true;
            
            Debug.Log($"✅ Canvas {canvasObj.name} configurado en World Space con sorting order {canvas.sortingOrder}");
        }
        
        // Hacer que el canvas sea hijo del Image Target PRIMERO
        canvasObj.transform.SetParent(imageTarget.transform, false);
        
        // Configurar el RectTransform del canvas para centrarlo
        RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Asegurar que el RectTransform tenga un tamaño adecuado
            rectTransform.sizeDelta = new Vector2(1920, 1080); // Tamaño estándar Full HD
            
            // Centrar el pivot y anchor del canvas
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        // Posicionar el canvas relativo al Image Target (centrado)
        canvasObj.transform.localPosition = offsetPosicion;
        canvasObj.transform.localRotation = Quaternion.Euler(rotacionMenu);
        canvasObj.transform.localScale = Vector3.one * escalaMenu;
        
        Debug.Log($"✅ Canvas {canvasObj.name} anclado al Image Target en posición {offsetPosicion}");
    }
    
    void OnDestroy()
    {
        // Desuscribirse de los eventos al destruir el objeto
        if (imageTarget != null)
        {
            imageTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
    
    // Método llamado cuando cambia el estado del Image Target
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            OnTrackingFound();
        }
        else
        {
            OnTrackingLost();
        }
    }
    
    // Cuando se detecta el Image Target
    private void OnTrackingFound()
    {
        Debug.Log("🎯 Image Target DETECTADO - Mostrando menú");
        
        // Solo mostrar el canvas normal si no hay otros canvas activos
        if (canvasNormal != null && !tutorialVicentico.activeSelf && !canvasManipulacion.activeSelf)
        {
            canvasNormal.SetActive(true);
        }
    }
    
    // Cuando se pierde el Image Target
    private void OnTrackingLost()
    {
        Debug.Log("❌ Image Target PERDIDO - Ocultando menú");
        
        // Ocultar todos los canvas cuando se pierde el target
        if (canvasNormal != null)
            canvasNormal.SetActive(false);
            
        if (tutorialVicentico != null)
            tutorialVicentico.SetActive(false);
            
        if (pageTemplate != null)
            pageTemplate.SetActive(false);
            
        if (canvasManipulacion != null)
            canvasManipulacion.SetActive(false);
            
        if (objetoFBX != null)
            objetoFBX.SetActive(false);
            
        if (canvasVideo != null)
            canvasVideo.SetActive(false);
    }
    
    // Llamado por el botón "Instrucciones" - Activa Tutorial_vicentico y Page_template
    public void MostrarInstrucciones()
    {
        Debug.Log("=== BOTÓN INSTRUCCIONES PULSADO ===");
        
        // Ocultar el menú normal
        canvasNormal.SetActive(false);
        Debug.Log("✅ Canvas Normal: FALSE");
        
        // Activar (checked) Tutorial_vicentico
        tutorialVicentico.SetActive(true);
        Debug.Log("✅ Tutorial_vicentico: TRUE (checked)");
        
        // Activar (checked) Page_template
        pageTemplate.SetActive(true);
        Debug.Log("✅ Page_template: TRUE (checked)");
        
        Debug.Log("=== FIN ===");
    }
    
    // Llamado por la "X" (botón cerrar) - Desactiva Tutorial_vicentico y Page_template
    public void MostrarCanvasNormal()
    {
        Debug.Log("=== BOTÓN CERRAR PULSADO ===");
        
        // Desactivar Tutorial_vicentico
        tutorialVicentico.SetActive(false);
        Debug.Log("✅ Tutorial_vicentico: FALSE (unchecked)");
        
        // Desactivar Page_template
        pageTemplate.SetActive(false);
        Debug.Log("✅ Page_template: FALSE (unchecked)");
        
        // Mostrar el menú normal
        canvasNormal.SetActive(true);
        Debug.Log("✅ Canvas Normal: TRUE");
        
        Debug.Log("=== FIN ===");
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
    
    // Llamado por el botón "Video Intro"
    public void AbrirVideoIntro()
    {
        Debug.Log("=== ABRIENDO VIDEO INTRO ===");
        
        // Ocultar el menú principal
        if (canvasNormal != null)
        {
            canvasNormal.SetActive(false);
            Debug.Log("✅ Canvas Normal ocultado");
        }
        
        // Mostrar el canvas del video
        if (canvasVideo != null)
        {
            canvasVideo.SetActive(true);
            Debug.Log("✅ Canvas Video mostrado");
        }
        else
        {
            Debug.LogError("❌ Canvas Video es NULL - ASÍGNALO EN EL INSPECTOR");
            return;
        }
        
        // Iniciar reproducción del video (si hay un controlador)
        if (videoPlayerController != null)
        {
            videoPlayerController.AbrirVideo();
            Debug.Log("✅ Reproductor de video iniciado");
        }
        
        // IMPORTANTE: Forzar configuración World Space después de que VideoPlayerController lo modifique
        StartCoroutine(ForzarVideoEnWorldSpace());
        
        Debug.Log("=== FIN ABRIR VIDEO INTRO ===");
    }
    
    // Coroutine para mantener el canvas de video en World Space
    private System.Collections.IEnumerator ForzarVideoEnWorldSpace()
    {
        // Esperar un frame para que VideoPlayerController haga sus cambios
        yield return null;
        
        if (canvasVideo != null)
        {
            Canvas canvas = canvasVideo.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Forzar World Space
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 20;
                Debug.Log("🔧 Canvas Video forzado a World Space después de VideoPlayerController");
            }
            
            // Asegurar que esté como hijo del Image Target
            if (imageTarget != null && canvasVideo.transform.parent != imageTarget.transform)
            {
                canvasVideo.transform.SetParent(imageTarget.transform, false);
                canvasVideo.transform.localPosition = offsetPosicion;
                canvasVideo.transform.localRotation = Quaternion.Euler(rotacionMenu);
                canvasVideo.transform.localScale = Vector3.one * escalaMenu;
                Debug.Log("🔧 Canvas Video reposicionado en Image Target");
            }
            
            // Asegurar que el RectTransform esté centrado
            RectTransform rectTransform = canvasVideo.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                Debug.Log("🔧 RectTransform del video centrado");
            }
        }
    }
    
    // Llamado por el botón cerrar del canvas de video
    public void CerrarVideo()
    {
        Debug.Log("=== CERRANDO VIDEO ===");
        
        // Ocultar el canvas de video
        if (canvasVideo != null)
        {
            canvasVideo.SetActive(false);
            Debug.Log("✅ Canvas Video ocultado");
        }
        
        // Detener el video si hay un controlador
        if (videoPlayerController != null)
        {
            // Asumiendo que existe un método para cerrar/detener
            // Si no existe, esto dará error y deberás ajustarlo
            Debug.Log("⚠️ Detener video en VideoPlayerController");
        }
        
        // Volver a mostrar el menú principal
        if (canvasNormal != null)
        {
            canvasNormal.SetActive(true);
            Debug.Log("✅ Canvas Normal visible");
        }
        
        Debug.Log("=== FIN CERRAR VIDEO ===");
    }
}