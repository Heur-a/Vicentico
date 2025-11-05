using System;
using System.Collections.Generic;
using UnityEngine;

public class ControladorDistanciaCartas : MonoBehaviour
{
    public List<Transform> cartas;
    [SerializeField] private Dictionary<(String carta1, String carta2),float> _distancias = new Dictionary<(String,String),float>();
    public float distanciaMin = 3f;
    public bool suficienteDistancia = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _distancias.Clear();
        _distancias = CalculateDistances(cartas);
        foreach (var keyValuePair in _distancias)
        {
            if (keyValuePair.Value < distanciaMin)
            {
                suficienteDistancia = false;
                Debug.Log(keyValuePair);
            }
            else suficienteDistancia = true;
        }
        
    }
    
    public Dictionary<(String carta1, String carta2) , float> CalculateDistances(List<Transform> transforms)
    {
        Dictionary<(String carta1, String carta2) , float> distanceDictionary = new Dictionary<(String, String) , float>();
        
        for (int i = 0; i < transforms.Count; i++)
        {
            for (int j = i + 1; j < transforms.Count; j++)
            {
                if (transforms[i] == null || transforms[j] == null) continue;
                
                float distance = Vector3.Distance(transforms[i].position, transforms[j].position);
                string carta1 = $"{transforms[i].name}";
                string carta2 = $"{transforms[j].name}";
                
                distanceDictionary.Add((carta1,carta2),distance);
            }
        }
        
        return distanceDictionary;
    }
}
