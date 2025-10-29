using UnityEngine;
using UnityEngine.UI;
using TMPro; // ¡Importante para TextMeshPro!
using System.Collections; // ¡Importante para la animación (Corutina)!

/*
 * Esta es la clase que define el contenido de UNA página.
 * [System.Serializable] la hace visible en el Inspector de Unity.
 */
[System.Serializable]
public class TutorialPageContent
{
    public string title;
    [TextArea(5, 10)] // Hace que el campo de texto sea más grande en el Inspector
    public string description;
    public Sprite image; // Arrastra aquí la imagen para la página
}

/*
 * Este es el script principal qu
 */
public class TutorialManager : MonoBehaviour
{
    // --- CAMPOS DE CONTENIDO ---
    // ¡Aquí es donde "copias y pegas" todo tu contenido!
    public TutorialPageContent[] pageContents;

    // --- REFERENCIAS DE UI (Arrastra en el Inspector) ---
    [Header("Referencias de UI")]
    public GameObject pageTemplate; // El panel "Page_Template"
    public TextMeshProUGUI titleTextElement;
    public TextMeshProUGUI descriptionTextElement;
    public Image imageElement;
    public Button nextButton;
    public Button backButton;

    [Header("Referencias de Menú")]
    public GameObject tutorialCanvas;
    public GameObject mainMenuCanvas; // Referencia al canvas del menú principal

    // --- FUENTE Y ANIMACIÓN ---
    [Header("Estilo y Animación")]
    public TMP_FontAsset nasalizationFont; // Arrastra tu "Nasalization Asset" aquí
    public float textAnimationSpeed = 0.05f; // Velocidad del efecto "máquina de escribir"
    public float imageFadeDuration = 0.5f; // Duración del efecto fade de la imagen
    public bool autoConfigureLayout = false; // Configurar automáticamente el layout al abrir (DESACTIVADO para mantener posiciones del Inspector)

    // --- VARIABLES INTERNAS ---
    private int currentPageIndex = 0;
    private Coroutine textAnimationCoroutine;
    private Coroutine imageFadeCoroutine;

    void Start()
    {
        // Aplicar la fuente personalizada al iniciar
        if (nasalizationFont != null)
        {
            titleTextElement.font = nasalizationFont;
            descriptionTextElement.font = nasalizationFont;
        }

        // NOTA: NO desactivamos tutorialCanvas ni pageTemplate aquí
        // El MenuManager se encarga de la gestión de activación/desactivación
        // para evitar conflictos
    }

    // Función para ABRIR el tutorial (desde el menú principal)
    public void OpenTutorial()
    {
        // 1. Ocultar el menú principal
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }
        
        // 2. Mostrar el tutorial
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(true);
        }
        
        if (pageTemplate != null)
        {
            pageTemplate.SetActive(true);
            
            // Configurar el layout automáticamente
            if (autoConfigureLayout)
            {
                ConfigurarLayout();
            }
        }
        
        ShowPage(0);
    }

    // Configura el layout de los elementos: título arriba, descripción izquierda, imagen derecha
    private void ConfigurarLayout()
    {
        // Configurar Título - Arriba centrado
        if (titleTextElement != null)
        {
            RectTransform titleRect = titleTextElement.rectTransform;
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 80);
            titleTextElement.alignment = TextAlignmentOptions.Top;
        }
        
        // Configurar Descripción - Mitad izquierda de la pantalla
        if (descriptionTextElement != null)
        {
            RectTransform descRect = descriptionTextElement.rectTransform;
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(0.5f, 1);
            descRect.pivot = new Vector2(0, 0.5f);
            descRect.anchoredPosition = new Vector2(20, -60);
            descRect.sizeDelta = new Vector2(-40, -160);
            descriptionTextElement.alignment = TextAlignmentOptions.TopLeft;
        }
        
        // Configurar Imagen - Mitad derecha de la pantalla
        if (imageElement != null)
        {
            RectTransform imageRect = imageElement.rectTransform;
            imageRect.anchorMin = new Vector2(0.5f, 0);
            imageRect.anchorMax = new Vector2(1, 1);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = new Vector2(0, -60);
            imageRect.sizeDelta = new Vector2(-40, -160);
            imageElement.preserveAspect = true;
        }
    }

    // Función para CERRAR el tutorial (botón "X")
    public void CloseTutorial()
    {
        // Detener animaciones
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        if (imageFadeCoroutine != null)
        {
            StopCoroutine(imageFadeCoroutine);
        }
        
        // 1. Ocultar el tutorial
        if (tutorialCanvas != null)
        {
            tutorialCanvas.SetActive(false);
        }
        if (pageTemplate != null)
        {
            pageTemplate.SetActive(false);
        }
        
        // 2. Mostrar el menú principal
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }

  

    // Carga el contenido de una página específica
    public void ShowPage(int index)
    {
        // Seguridad: no hacer nada si el índice está fuera de rango
        if (index < 0 || index >= pageContents.Length)
        {
            return;
        }

        currentPageIndex = index;
        
        // Detener las animaciones anteriores antes de empezar nuevas
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        if (imageFadeCoroutine != null)
        {
            StopCoroutine(imageFadeCoroutine);
        }

        // Cargar el contenido de la página actual
        TutorialPageContent content = pageContents[currentPageIndex];

        // 1. Poner el Título (sin animación)
        titleTextElement.text = content.title;

        // 2. Poner la Imagen con efecto fade
        if (content.image != null)
        {
            imageElement.sprite = content.image;
            imageFadeCoroutine = StartCoroutine(FadeInImage());
        }
        else
        {
            imageElement.gameObject.SetActive(false); // Oculta la imagen si no hay ninguna
        }

        // 3. Poner la Descripción (con animación)
        textAnimationCoroutine = StartCoroutine(TypewriterEffect(content.description));

        // 4. Actualizar botones
        UpdateNavigationButtons();
    }

    // Efecto "Máquina de Escribir" para el texto
    IEnumerator TypewriterEffect(string textToType)
    {
        descriptionTextElement.text = ""; // Limpiar el texto primero
        
        // Desactivar botones mientras se escribe para evitar problemas
        nextButton.interactable = false;
        backButton.interactable = false;

        foreach (char c in textToType.ToCharArray())
        {
            descriptionTextElement.text += c;
            yield return new WaitForSeconds(textAnimationSpeed);
        }

        // Al terminar, asegurarse de que los botones tengan el estado correcto
        UpdateNavigationButtons();
    }

    // Efecto de Fade In para la imagen
    IEnumerator FadeInImage()
    {
        if (imageElement == null) yield break;
        
        // Activar el GameObject de la imagen
        imageElement.gameObject.SetActive(true);
        
        // Empezar con transparencia 0 (invisible)
        Color startColor = imageElement.color;
        startColor.a = 0f;
        imageElement.color = startColor;
        
        // Gradualmente aumentar la opacidad
        float elapsedTime = 0f;
        while (elapsedTime < imageFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / imageFadeDuration);
            
            Color newColor = imageElement.color;
            newColor.a = alpha;
            imageElement.color = newColor;
            
            yield return null;
        }
        
        // Asegurar que termine en opacidad completa
        Color finalColor = imageElement.color;
        finalColor.a = 1f;
        imageElement.color = finalColor;
    }

    // Función para el botón "Siguiente"
    public void NextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    // Función para el botón "Atrás"
    public void PreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }

    // Activa/desactiva botones si estamos en la primera o última página
    private void UpdateNavigationButtons()
    {
        backButton.interactable = (currentPageIndex > 0);
        nextButton.interactable = (currentPageIndex < pageContents.Length - 1);
    }
}