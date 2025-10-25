using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Referencias a los Canvas")]
    [SerializeField] private GameObject canvasNormal;
    [SerializeField] private GameObject canvasInstrucciones;
    
    void Start()
    {
        // Estado inicial: Canvas normal visible, instrucciones oculto
        MostrarCanvasNormal();
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
}