using UnityEngine;
using System.Collections;

public class MenuManagerMejorado : MonoBehaviour
{
    [Header("Referencias a los Canvas")]
    [SerializeField] private GameObject canvasNormal;
    [SerializeField] private GameObject canvasInstrucciones;
    
    [Header("Animaciones (opcional)")]
    [SerializeField] private Animator animatorNormal;
    [SerializeField] private Animator animatorInstrucciones;
    
    void Start()
    {
        MostrarCanvasNormal();
    }
    
    public void MostrarInstrucciones()
    {
        if (animatorNormal != null && animatorInstrucciones != null)
        {
            // Con animaciones
            StartCoroutine(TransicionConAnimacion(false));
        }
        else
        {
            // Sin animaciones
            canvasNormal.SetActive(false);
            canvasInstrucciones.SetActive(true);
        }
    }
    
    public void MostrarCanvasNormal()
    {
        if (animatorNormal != null && animatorInstrucciones != null)
        {
            // Con animaciones
            StartCoroutine(TransicionConAnimacion(true));
        }
        else
        {
            // Sin animaciones
            canvasInstrucciones.SetActive(false);
            canvasNormal.SetActive(true);
        }
    }
    
    private IEnumerator TransicionConAnimacion(bool volverANormal)
    {
        if (!volverANormal)
        {
            // Ir a instrucciones
            animatorNormal.SetTrigger("Salir");
            yield return new WaitForSeconds(0.3f);
            canvasNormal.SetActive(false);
            canvasInstrucciones.SetActive(true);
            animatorInstrucciones.SetTrigger("Entrar");
        }
        else
        {
            // Volver a normal
            animatorInstrucciones.SetTrigger("Salir");
            yield return new WaitForSeconds(0.3f);
            canvasInstrucciones.SetActive(false);
            canvasNormal.SetActive(true);
            animatorNormal.SetTrigger("Entrar");
        }
    }
    
    // Para cerrar con tecla ESC
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canvasInstrucciones.activeSelf)
        {
            MostrarCanvasNormal();
        }
    }
}