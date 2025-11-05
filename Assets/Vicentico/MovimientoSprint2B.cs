using UnityEngine;

public class MovimientoSprint2B : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    Camera _camMain;
    private Vector3 _lastRaycastHit;
    public Transform player;
    public float maxSpeed = 3f;
    public float sensibilidadMovimiento = 0.2f;
    public Transform marca;
    private (bool hasHit, Vector3 pointWorldLocation) _locationRayCast;
    public float riseTime = 0.5f;
    private float _timer;
    void Start()
    {
        _camMain = Camera.main;
        player.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        ActualizarUltimoPunto();
        
        // Si hemos llegado resetear velocidad
        ResetTimer();
        //Calculamos la velocidad de este momento
        float calculatedSpeed = CalculateExponentialSpeed(maxSpeed, riseTime);
        
        // Movemos el perro y la marca
        PonerMarca();
        MoverPerro(calculatedSpeed);
        
    }

    private bool ActualizarUltimoPunto()
    {
        if (_locationRayCast.hasHit)
        {
            //No movemos la cámara
            if (Mathf.Abs(_locationRayCast.pointWorldLocation.x - _lastRaycastHit.x) < sensibilidadMovimiento
                &&
                Mathf.Abs(_locationRayCast.pointWorldLocation.z - _lastRaycastHit.z) < sensibilidadMovimiento
               ) return true;
            
            //Actualizar ultimo punto
            _lastRaycastHit = _locationRayCast.pointWorldLocation;
        }

        return false;
    }

    private void MoverPerro(float speed)
    {
        //TODO: Hacer que se mueva hacia al punto con velocidad
            
        //Sacar direccion movimiento y normalizar
        Vector3 directionRaw = _lastRaycastHit - player.position;
        Vector3 directionNormalized = directionRaw.normalized;
        Debug.DrawRay(player.position, directionNormalized * speed, Color.red);
        player.Translate(speed * Time.deltaTime * directionNormalized, Space.World);
        //Alinear perro
        float degs = Mathf.Atan2(directionNormalized.x, directionNormalized.y) * Mathf.Rad2Deg;
        player.eulerAngles = new Vector3(player.eulerAngles.x, degs, player.eulerAngles.z);
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

    float CalculateExponentialSpeed(float maxDesiredSpeed, float reachTime)
    {   
       
        float c = Mathf.Log(maxDesiredSpeed);
        float exponent = -((reachTime * _timer) - c);
        float speedMomentous = - Mathf.Exp(exponent) + maxDesiredSpeed;
        _timer += Time.deltaTime;
        return speedMomentous;
        
    }

    void ResetTimer()
    {
        if (Mathf.Abs(player.position.x - _lastRaycastHit.x) < sensibilidadMovimiento
            &&
            Mathf.Abs(player.position.z - _lastRaycastHit.z) < sensibilidadMovimiento
           )
        {
            _timer = 0;
        };
    }
}
