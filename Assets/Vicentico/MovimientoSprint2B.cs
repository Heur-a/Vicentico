using UnityEditor;
using UnityEngine;

public class MovimientoSprint2B : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    Camera _camMain;
    private Vector3 _raycastHit;
    public Transform player;
    void Start()
    {
        _camMain = Camera.main;
        player.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        var locationRayCast = GetLocationRayCast();
        if (locationRayCast.hasHit)
        {
            player.position = locationRayCast.pointWorldLocation;
        }
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
}
