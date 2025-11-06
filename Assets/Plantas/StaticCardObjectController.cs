using System.Collections.Generic;
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
    private bool _initializationComplete = false;
    
    // Lista para almacenar las líneas que se deben dibujar cada frame
    private List<LineaRayo> _lineasADibujar = new List<LineaRayo>();

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
        
        _initializationComplete = true;
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

    void Update()
    {
        if (!_initializationComplete) return;
        
        CalcularTodasLasDistancias();
        
        // Actualizar la visibilidad de los hijos basado en la distancia
        UpdateChildrenVisibilityBasedOnDistance();
        
        // Dibujar los rayos cada frame
        DibujarRayos();
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
            Debug.Log(distanciaCarta.distancia + "" +distanciaCarta.carta + "" + targetStatus.carta );
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

    // Método principal para dibujar rayos
    private void DibujarRayos()
    {
        // Limpiar la lista de líneas del frame anterior
        _lineasADibujar.Clear();
        
        // Para cada target status que sea Planta y haya sido visto
        foreach (var targetStatus in CardsInfoHolder.targetStatuses)
        {
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta && targetStatus.HasBeenActive)
            {
                // Calcular y almacenar las líneas que deben dibujarse para este target
                CalcularLineasParaTarget(targetStatus);
            }
        }
        
        // Dibujar todas las líneas almacenadas
        foreach (var linea in _lineasADibujar)
        {
            Debug.DrawLine(linea.desde, linea.hasta, linea.color, Time.deltaTime, false);
        }
    }

    // Calcular las líneas que deben dibujarse para un target específico
    private void CalcularLineasParaTarget(TargetInfoHolder.TargetStatus targetStatus)
    {
        if (targetStatus.carta == null) return;
        
        Vector3 posicionInicial = targetStatus.carta.position;
        
        // Revisar todas las distancias calculadas para este target
        foreach (var distanciaCarta in targetStatus.Distancias)
        {
            if (distanciaCarta.carta == null) continue;
            
            // Solo dibujar líneas hacia targets que también hayan sido vistos (si son plantas)
            if (EsTargetVisibleParaRayos(distanciaCarta.carta))
            {
                Vector3 posicionFinal = distanciaCarta.carta.position;
                Color colorLinea = distanciaCarta.distancia >= distanciaMinima ? colorDistanciaBuena : colorDistanciaMala;
                
                // Añadir la línea a la lista para dibujar
                _lineasADibujar.Add(new LineaRayo(posicionInicial, posicionFinal, colorLinea));
            }
        }
    }

    // Verificar si un target es visible para dibujar rayos hacia él
    private bool EsTargetVisibleParaRayos(Transform targetTransform)
    {
        // Buscar el TargetStatus correspondiente a este transform
        foreach (var status in CardsInfoHolder.targetStatuses)
        {
            if (status.carta == targetTransform)
            {
                // Si es Vicentico, siempre es visible para rayos
                if (status.tipo == TargetInfoHolder.TipoCarta.Vicentico)
                    return true;
                
                // Si es Planta, solo es visible si ha sido vista
                if (status.tipo == TargetInfoHolder.TipoCarta.Planta)
                    return status.HasBeenActive;
            }
        }
        
        return false;
    }

    // Estructura para almacenar información de una línea a dibujar
    private struct LineaRayo
    {
        public Vector3 desde;
        public Vector3 hasta;
        public Color color;
        
        public LineaRayo(Vector3 desde, Vector3 hasta, Color color)
        {
            this.desde = desde;
            this.hasta = hasta;
            this.color = color;
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