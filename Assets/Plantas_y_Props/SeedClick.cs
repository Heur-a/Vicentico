using UnityEngine;
using UnityEngine.EventSystems;

public class SeedClick : MonoBehaviour
{
    public ControladorCanvas canvasCtrl; // arrastra aquí tu panel/canvas
    public void OnClicked()
    {
        if (canvasCtrl != null)
        {
            canvasCtrl.ShowCanvas();
            Debug.Log("[SeedClickable] Click en semilla -> mostrar Canvas");
        }
        else
        {
            Debug.LogWarning("[SeedClickable] canvasCtrl no asignado");
        }
    }
}
