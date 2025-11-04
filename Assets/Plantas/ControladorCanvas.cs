using UnityEngine;

public class ControladorCanvas : MonoBehaviour
{
    [Header("Panel a mostrar/ocultar")]
    public GameObject regarCanvas;   // arrastra aquí ESTE MISMO panel/Canvas

    void Reset()
    {
        if (regarCanvas == null) regarCanvas = gameObject;
    }

    public void ShowCanvas()
    {
        if (regarCanvas) regarCanvas.SetActive(true);
    }

    public void HideCanvas()
    {
        if (regarCanvas) regarCanvas.SetActive(false);
        Debug.Log("[Canvas] Oculto");
    }
}
