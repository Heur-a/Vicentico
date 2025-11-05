using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Vuforia;

public class StaticCardObjectController : MonoBehaviour
{
    public TargetInfoHolder CardsInfoHolder;
    
    [Header("Configuración")]
    public List<ImageTargetBehaviour> targetsPlantas = new List<ImageTargetBehaviour>();
    public ImageTargetBehaviour cartaVicentico;
    public float distanciaMinima = 3f;
    
    [Header("Colores")]
    public Color colorDistanciaBuena = Color.green;
    public Color colorDistanciaMala = Color.red;

    // Diccionario para mantener referencia de los observers
    private Dictionary<ImageTargetBehaviour, TargetObserver> _targetObservers = new Dictionary<ImageTargetBehaviour, TargetObserver>();
    private DistanceRayDrawer _rayDrawer;
    private bool _initializationComplete = false;
    
    public List<float> distances = new List<float>();

    void Start()
    {
        // Añadir cartas al objeto global
        CardsInfoHolder = TargetInfoHolder.GetInstance();
        
        // Inicializar la lista de targetStatuses si es null
        if (CardsInfoHolder.targetStatuses == null)
            CardsInfoHolder.targetStatuses = new List<TargetInfoHolder.TargetStatus>();
        
        foreach (ImageTargetBehaviour cartaPlanta in targetsPlantas)
        {
            // Crear el TargetStatus
            var targetStatus = new TargetInfoHolder.TargetStatus(cartaPlanta, TargetInfoHolder.TipoCarta.Planta);
            
            // Agregar al holder
            CardsInfoHolder.AddTargetStatus(targetStatus);
            
            // Crear y configurar el observer para este target
            CreateTargetObserver(cartaPlanta, targetStatus);
        }
        
        // Agregar Vicentico
        var targetVicentico = new TargetInfoHolder.TargetStatus(cartaVicentico, TargetInfoHolder.TipoCarta.Vicentico);
        CardsInfoHolder.AddTargetStatus(targetVicentico);
        
        // Inicializar el DistanceRayDrawer después de un frame para asegurar que todo está configurado
        Invoke(nameof(InitializeRayDrawer), 0.1f);
    }

    // Método para crear observers para cada target
    private void CreateTargetObserver(ImageTargetBehaviour target, TargetInfoHolder.TargetStatus status)
    {
        // Crear un GameObject para el observer o agregarlo al target existente
        TargetObserver observer = target.gameObject.AddComponent<TargetObserver>();
        observer.Initialize(target, status);
        
        // Guardar referencia
        _targetObservers[target] = observer;
    }

    private void InitializeRayDrawer()
    {
        // Añadir el componente DistanceRayDrawer al mismo GameObject
        _rayDrawer = gameObject.AddComponent<DistanceRayDrawer>();
        _rayDrawer.Initialize(this, CardsInfoHolder, distanciaMinima, colorDistanciaBuena, colorDistanciaMala);
        
        _initializationComplete = true;
    }

    void Update()
    {
        if (!_initializationComplete) return;
        
        CalcularTodasLasDistancias();
        
        // Actualizar la visibilidad de los hijos basado en la distancia
        UpdateChildrenVisibilityBasedOnDistance();
    }
    
    private void CalcularTodasLasDistancias()
    {
        foreach (var targetStatus in CardsInfoHolder.targetStatuses)
        {
            // Limpiar distancias anteriores
            targetStatus.Distancias.Clear();
            
            Transform posIni = targetStatus.carta;

            // Calcular distancias a todas las plantas
            foreach (var cartaPlanta in targetsPlantas)
            {
                Transform posFinal = cartaPlanta.GetComponent<Transform>();
                if (posIni == posFinal) continue;
                float distancia = Vector3.Distance(posIni.position, posFinal.position);
                TargetInfoHolder.DistanciaCartas distACarta = new TargetInfoHolder.DistanciaCartas(posFinal, distancia);
                targetStatus.Distancias.Add(distACarta);
            }
            
            // Calcular distancia a Vicentico
            Transform posFinalVicentico = cartaVicentico.GetComponent<Transform>();
            if (posIni != posFinalVicentico)
            {
                float distanciaVicentico = Vector3.Distance(posIni.position, posFinalVicentico.position);
                TargetInfoHolder.DistanciaCartas distACartaVicentico = new TargetInfoHolder.DistanciaCartas(posFinalVicentico, distanciaVicentico);
                targetStatus.Distancias.Add(distACartaVicentico);
            }
        }
    }

    // Método para verificar el estado de los targets de tipo Planta
    private void CheckPlantaTargetsStatus()
    {
        foreach (var targetStatus in CardsInfoHolder.targetStatuses)
        {
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta)
            {
                if (targetStatus.HasBeenActive)
                {
                    // El target Planta ha sido visto por primera vez
                    // Podemos realizar acciones adicionales aquí si es necesario
                }
            }
        }
    }

    // Método para actualizar la visibilidad de los hijos basado en la distancia
    private void UpdateChildrenVisibilityBasedOnDistance()
    {
        foreach (var targetStatus in CardsInfoHolder.targetStatuses)
        {
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta && targetStatus.HasBeenActive)
            {
                bool shouldBeVisible = CheckDistanceRequirements(targetStatus);
                SetChildrenVisibility(targetStatus, shouldBeVisible);
            }
        }
    }

    // Verificar si se cumplen los requisitos de distancia
    private bool CheckDistanceRequirements(TargetInfoHolder.TargetStatus targetStatus)
    {
        foreach (var distanciaCarta in targetStatus.Distancias)
        {
            Debug.Log("Distancia carta + =" + distanciaCarta.distancia );
            // Si alguna distancia es menor que la mínima, no cumple los requisitos
            if (distanciaCarta.distancia <= distanciaMinima)
            {
                return false;
            }
        }
        return true;
    }

    // Activar o desactivar los hijos del target
    private void SetChildrenVisibility(TargetInfoHolder.TargetStatus targetStatus, bool visible)
    {
        if (targetStatus.carta != null)
        {
            foreach (Transform child in targetStatus.carta)
            {
                child.gameObject.SetActive(visible);
            }
        }
    }
    
    void OnDestroy()
    {
        // Limpiar los observers cuando se destruya el objeto
        foreach (var observer in _targetObservers.Values)
        {
            if (observer != null)
            {
                Destroy(observer);
            }
        }
        _targetObservers.Clear();
    }
}