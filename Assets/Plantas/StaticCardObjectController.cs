using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class StaticCardObjectController : MonoBehaviour
{
    public TargetInfoHolder cardsInfoHolder;
    
    [Header("Configuración")]
    public List <ImageTargetBehaviour> targetsPlantas = new List<ImageTargetBehaviour>();
    public ImageTargetBehaviour cartaVicentico;
    public float distanciaMinima = 3f;
    
    [Header("Colores")]
    public Color colorDistanciaBuena = Color.green;
    public Color colorDistanciaMala = Color.red;
    
    [Header("Estado")]
    public bool suficienteDistancia = false;
    public List<GameObject> cartasCumplenDistancia = new List<GameObject>();
    public List<GameObject> cartasNoCumplenDistancia = new List<GameObject>();

    // Diccionario para mantener referencia de los observers
    private Dictionary<ImageTargetBehaviour, TargetObserver> targetObservers = new Dictionary<ImageTargetBehaviour, TargetObserver>();

    void Start()
    {
        // Añadir cartas al objeto global
        cardsInfoHolder = TargetInfoHolder.GetInstance();
        
        foreach (ImageTargetBehaviour cartaPlanta in targetsPlantas)
        {
            // Crear el TargetStatus
            var targetStatus = new TargetInfoHolder.TargetStatus(cartaPlanta, TargetInfoHolder.TipoCarta.Planta);
            
            // Agregar al holder
            cardsInfoHolder.AddTargetStatus(targetStatus);
            
            // Crear y configurar el observer para este target
            CreateTargetObserver(cartaPlanta, targetStatus);
        }
        
        //Agregar Vicentico
        var targetVicentico = new TargetInfoHolder.TargetStatus(cartaVicentico, TargetInfoHolder.TipoCarta.Vicentico);
        cardsInfoHolder.AddTargetStatus(targetVicentico);
    }

    // Método para crear observers para cada target
    private void CreateTargetObserver(ImageTargetBehaviour target, TargetInfoHolder.TargetStatus status)
    {
        // Crear un GameObject para el observer o agregarlo al target existente
        TargetObserver observer = target.gameObject.AddComponent<TargetObserver>();
        observer.Initialize(target, status);
        
        // Guardar referencia
        targetObservers[target] = observer;
    }

    void Update()
    {
        CalcularTodasLasDistancias();
        
        // Opcional: Verificar el estado de los targets de tipo Planta
        CheckPlantaTargetsStatus();
    }
    
    private void CalcularTodasLasDistancias()
    {
        foreach (var targetStatus in cardsInfoHolder.targetStatuses)
        {
            Transform posIni = targetStatus.carta;

            foreach (var cartaFinal in targetsPlantas)
            {
                Transform posFinal = cartaFinal.GetComponent<Transform>();
                if (posIni == posFinal) continue;
                float distancia = Vector3.Distance(posIni.position, posFinal.position);
                TargetInfoHolder.DistanciaCartas distACarta = new TargetInfoHolder.DistanciaCartas(posFinal, distancia);
                targetStatus.Distancias.Add(distACarta);
            }
            
            Transform posFinalVicentio = cartaVicentico.GetComponent<Transform>();
            if (posIni == posFinalVicentio) continue;
            float distanciaVicentico = Vector3.Distance(posIni.position, posFinalVicentio.position);
            TargetInfoHolder.DistanciaCartas distACartaVicentico = new TargetInfoHolder.DistanciaCartas(posFinalVicentio, distanciaVicentico);
            targetStatus.Distancias.Add(distACartaVicentico);
            
            
        }
    }

    // Método para verificar el estado de los targets de tipo Planta
    private void CheckPlantaTargetsStatus()
    {
        foreach (var targetStatus in cardsInfoHolder.targetStatuses)
        {
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta)
            {
                // Puedes hacer algo con los targets Planta que ya han sido activados
                if (targetStatus.HasBeenActive)
                {
                    // El target Planta ha sido visto por primera vez
                    // Aquí puedes agregar lógica adicional
                }
            }
        }
    }

    // Método para obtener todos los targets Planta que han sido vistos
    public List<TargetInfoHolder.TargetStatus> GetPlantaTargetsSeen()
    {
        List<TargetInfoHolder.TargetStatus> plantasVistas = new List<TargetInfoHolder.TargetStatus>();
        
        foreach (var targetStatus in cardsInfoHolder.targetStatuses)
        {
            if (targetStatus.tipo == TargetInfoHolder.TipoCarta.Planta && targetStatus.HasBeenActive)
            {
                plantasVistas.Add(targetStatus);
            }
        }
        
        return plantasVistas;
    }

    void OnDestroy()
    {
        // Limpiar los observers cuando se destruya el objeto
        foreach (var observer in targetObservers.Values)
        {
            if (observer != null)
            {
                Destroy(observer);
            }
        }
        targetObservers.Clear();
    }
}