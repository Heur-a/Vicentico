using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class MovimientoSprint2B : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    Camera _camMain;
    private Vector3 _lastRaycastHit;
    public Transform player;
    public float speed = 3f;
    public float sensibilidadMovimiento = 0.2f;
    public Transform marca;
    private (bool hasHit, Vector3 pointWorldLocation) _locationRayCast;

    void Start()
    {
        _camMain = Camera.main;
        player.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (_locationRayCast.hasHit)
        {
            //Ha llegado al punto
            if (Mathf.Abs(_locationRayCast.pointWorldLocation.x - _lastRaycastHit.x) < sensibilidadMovimiento
                &&
                Mathf.Abs(_locationRayCast.pointWorldLocation.z - _lastRaycastHit.z) < sensibilidadMovimiento
                ) return;
            
            _lastRaycastHit = _locationRayCast.pointWorldLocation;
            
            MoverPerro();
            PonerMarca();
            
        }
    }

    private void MoverPerro()
    {
        //TODO: Hacer que se mueva hacia al punto con velocidad
            
        //Sacar direccion movimiento y normalizar
        Vector3 directionRaw = _lastRaycastHit - player.position;
        Vector3 directionNormalized = directionRaw.normalized;
            
        //Mover perro 
        player.Translate(speed * Time.deltaTime * directionNormalized, Space.World);
    }

    private void PonerMarca()
    {
        marca.position = _lastRaycastHit;
    }

    void FixedUpdate()
    { 
        _locationRayCast = GetLocationRayCast();
    }

    (bool hasHit, Vector3 pointWorldLocation) GetLocationRayCast()
    {
        Ray ray = _camMain.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 pointWorldLocation = Vector3.zero;
        bool hasHit = false;
        
        
        if (Physics.Raycast(ray, out hit))
        {
            pointWorldLocation = hit.point;
            hasHit = true;
        }
        
        return (hasHit, pointWorldLocation);
    }

    float CalculateExponentialSpeed(float speed, float reachTime)
    {
        //TODO: PONER EXPONENCIAL NEGATIVA
        return Mathf.Pow(speed, 2) * reachTime;
    }
}
