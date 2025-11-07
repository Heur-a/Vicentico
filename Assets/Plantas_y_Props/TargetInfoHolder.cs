using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class TargetInfoHolder
{
    private static TargetInfoHolder _instance;

    public List<TargetStatus> targetStatuses;
    
    public static TargetInfoHolder GetInstance() {
        if (_instance == null) {
            _instance = new TargetInfoHolder();
        }
        return _instance;
    }
    
    private TargetInfoHolder()
    {
        targetStatuses = new List<TargetStatus>();
    }

    public void AddTargetStatus(TargetStatus status)
    {
        targetStatuses.Add(status);
    }
        
    public class TargetStatus
    {
        public Transform carta;
        public List<DistanciaCartas> distancias;
        public bool hasBeenActive = false;
        public bool isCurrentlyTracked = false;
        public ImageTargetBehaviour imageTarget;
        public TipoCarta tipo;

        public TargetStatus(ImageTargetBehaviour imageTarget, TipoCarta tipo)
        {
            this.imageTarget = imageTarget;
            this.tipo = tipo;
            carta = imageTarget.GetComponent<Transform>();
            distancias = new List<DistanciaCartas>();
        }

        public List<DistanciaCartas> Distancias
        {
            get => distancias;
            set => distancias = value;
        }

        public bool HasBeenActive
        {
            get => hasBeenActive;
            set => hasBeenActive = value;
        }

        public bool IsCurrentlyTracked
        {
            get => isCurrentlyTracked;
            set => isCurrentlyTracked = value;
        }

        protected bool Equals(TargetStatus other)
        {
            return Equals(carta, other.carta);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TargetStatus)obj);
        }

        public override int GetHashCode()
        {
            return (carta != null ? carta.GetHashCode() : 0);
        }
    }
        
    public enum TipoCarta
    {
        Vicentico,
        Planta,
        Bebedero
    }
        
    public class DistanciaCartas
    {
        public Transform carta;
        public float distancia;
        
        public DistanciaCartas(Transform carta, float dist)
        {
            this.carta = carta;
            distancia = dist;
        }
    }
}