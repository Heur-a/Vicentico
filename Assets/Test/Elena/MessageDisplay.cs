using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// Script para mostrar mensajes temporales en la pantalla
public class MessageDisplay : MonoBehaviour
{
    [Header("Configuración de UI")]
    [Tooltip("Texto donde se mostrarán los mensajes")]
    public TMP_Text messageText;
    
    [Header("Configuración de Mensajes")]
    [Tooltip("Duración por defecto de los mensajes en segundos")]
    public float defaultMessageDuration = 2f;
    
    [Tooltip("Velocidad de fade in/out")]
    public float fadeSpeed = 2f;
    
    [Tooltip("¿Apilar mensajes en cola o sobrescribir?")]
    public bool queueMessages = true;
    
    // Singleton para acceso fácil desde otros scripts
    public static MessageDisplay Instance { get; private set; }
    
    private Queue<MessageData> messageQueue = new Queue<MessageData>();
    private Coroutine currentMessageCoroutine;
    private bool isShowingMessage = false;
    
    // Estructura para guardar datos del mensaje
    private struct MessageData
    {
        public string text;
        public float duration;
        public Color color;
    }
    
    void Awake()
    {
        // Configurar Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (messageText != null)
        {
            // Empezar con el texto invisible
            Color c = messageText.color;
            c.a = 0f;
            messageText.color = c;
            messageText.text = "";
        }
    }
    
    // Método público para mostrar un mensaje
    public void ShowMessage(string message, float duration = -1f, Color? color = null)
    {
        if (messageText == null)
        {
            Debug.LogWarning("No hay TMP_Text asignado en MessageDisplay!");
            return;
        }
        
        // Usar duración por defecto si no se especifica
        if (duration < 0)
            duration = defaultMessageDuration;
        
        // Usar color blanco si no se especifica
        Color messageColor = color ?? Color.white;
        
        MessageData data = new MessageData
        {
            text = message,
            duration = duration,
            color = messageColor
        };
        
        if (queueMessages)
        {
            // Añadir a la cola
            messageQueue.Enqueue(data);
            
            // Si no está mostrando ningún mensaje, empezar
            if (!isShowingMessage)
            {
                ProcessNextMessage();
            }
        }
        else
        {
            // Cancelar mensaje actual y mostrar el nuevo
            if (currentMessageCoroutine != null)
            {
                StopCoroutine(currentMessageCoroutine);
            }
            currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(data));
        }
    }
    
    // Procesar el siguiente mensaje de la cola
    void ProcessNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            MessageData data = messageQueue.Dequeue();
            currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(data));
        }
    }
    
    // Corrutina para mostrar el mensaje con fade in/out
    IEnumerator DisplayMessageCoroutine(MessageData data)
    {
        isShowingMessage = true;
        
        // Configurar el texto
        messageText.text = data.text;
        Color targetColor = data.color;
        
        // Fade In
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += fadeSpeed * Time.deltaTime;
            targetColor.a = alpha;
            messageText.color = targetColor;
            yield return null;
        }
        
        // Mantener visible
        yield return new WaitForSeconds(data.duration);
        
        // Fade Out
        while (alpha > 0f)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            targetColor.a = alpha;
            messageText.color = targetColor;
            yield return null;
        }
        
        // Limpiar
        messageText.text = "";
        isShowingMessage = false;
        
        // Procesar siguiente mensaje si hay
        if (queueMessages)
        {
            ProcessNextMessage();
        }
    }
    
    // Método para limpiar todos los mensajes
    public void ClearMessages()
    {
        messageQueue.Clear();
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        isShowingMessage = false;
        
        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = 0f;
            messageText.color = c;
            messageText.text = "";
        }
    }
}

