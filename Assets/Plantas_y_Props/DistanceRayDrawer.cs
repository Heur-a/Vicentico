using System.Collections.Generic;
using UnityEngine;

public class DistanceRayDrawer : MonoBehaviour
{
    private StaticCardObjectController controller;
    private TargetInfoHolder cardsInfoHolder;
    private float distanciaMinima;
    private Color colorBuena;
    private Color colorMala;
    private bool isInitialized = false;

    public void Initialize(StaticCardObjectController controller, TargetInfoHolder cardsInfoHolder, 
                          float distanciaMinima, Color colorBuena, Color colorMala)
    {
        this.controller = controller;
        this.cardsInfoHolder = TargetInfoHolder.GetInstance();
        this.distanciaMinima = distanciaMinima;
        this.colorBuena = colorBuena;
        this.colorMala = colorMala;
        this.isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
        
        DrawDistanceRays();
    }

    private void DrawDistanceRays()
    {
        foreach (var targetStatus in cardsInfoHolder.targetStatuses)
        {
            // Solo dibujar rayos para targets Planta que han sido vistos
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta && targetStatus.HasBeenActive)
            {
                DrawRaysFromTarget(targetStatus);
            }
        }
    }

    private void DrawRaysFromTarget(TargetInfoHolder.TargetStatus sourceTarget)
    {
        foreach (var distanciaCarta in sourceTarget.Distancias)
        {
            if (distanciaCarta.carta != null && sourceTarget.carta != null)
            {
                Color rayColor = distanciaCarta.distancia >= distanciaMinima ? colorBuena : colorMala;
                
                // Dibujar el rayo
                Debug.DrawLine(sourceTarget.carta.position, distanciaCarta.carta.position, rayColor, 0.1f);
                
                // Opcional: añadir esfera en el punto medio para mejor visualización
                //DrawDistanceIndicator(sourceTarget.carta.position, distanciaCarta.carta.position, rayColor);
            }
        }
    }
    
}