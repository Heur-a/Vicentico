using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class TargetObserver : MonoBehaviour
{
    private ImageTargetBehaviour imageTarget;
    private TargetInfoHolder.TargetStatus targetStatus;
    private bool firstTimeTracked = true;
    private TargetInfoHolder targetInfoHolder;

    public void Initialize(ImageTargetBehaviour target, TargetInfoHolder.TargetStatus status)
    {
        imageTarget = target;
        targetStatus = status;
        targetInfoHolder = TargetInfoHolder.GetInstance();
        
        // Inicializar la lista de distancias si es null
        if (targetStatus.Distancias == null)
            targetStatus.Distancias = new List<TargetInfoHolder.DistanciaCartas>();
        
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
            
            // Marcar como actualmente trackeado
            targetStatus.IsCurrentlyTracked = true;
        }
        else
        {
            // No está siendo trackeado
            targetStatus.IsCurrentlyTracked = false;
        }
        
        UpdateStaticTargetStatus();
    }
    
    private void UpdateStaticTargetStatus()
    {
        foreach (TargetInfoHolder.TargetStatus status in targetInfoHolder.targetStatuses)
        {
            if (status.Equals(targetStatus))
            {
                status.IsCurrentlyTracked = targetStatus.IsCurrentlyTracked;
                status.hasBeenActive = targetStatus.hasBeenActive;
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