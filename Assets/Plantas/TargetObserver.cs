using UnityEngine;
using Vuforia;

public class TargetObserver : MonoBehaviour
{
    private ImageTargetBehaviour imageTarget;
    private TargetInfoHolder.TargetStatus targetStatus;
    private bool firstTimeTracked = true;

    public void Initialize(ImageTargetBehaviour target, TargetInfoHolder.TargetStatus status)
    {
        imageTarget = target;
        targetStatus = status;
        
        // Suscribirse a los eventos de tracking
        var observerBehaviour = imageTarget.GetComponent<ObserverBehaviour>();
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // Verificar si el target está siendo trackeado
        if (status.Status == Status.TRACKED || 
            status.Status == Status.EXTENDED_TRACKED)
        {
            if (firstTimeTracked && targetStatus != null)
            {
                // Solo marcar como activo si es de tipo Planta y es la primera vez
                if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta && !targetStatus.HasBeenActive)
                {
                    targetStatus.HasBeenActive = true;
                    firstTimeTracked = false;
                    Debug.Log($"Target Planta detectado por primera vez! HasBeenActive = {targetStatus.HasBeenActive}");
                }
            }
        }
    }

    void OnDestroy()
    {
        // Desuscribirse de los eventos al destruir el objeto
        if (imageTarget != null)
        {
            var observerBehaviour = imageTarget.GetComponent<ObserverBehaviour>();
            if (observerBehaviour != null)
            {
                observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
            }
        }
    }
}