using UnityEngine;

public class ResetPos : MonoBehaviour
{
    
    public Transform target;
    private Vector3 _initalPos;
    private Quaternion _initalRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _initalPos = target.localPosition;
        _initalRot = target.localRotation;
    }

    public void Reset()
    {
        target.SetLocalPositionAndRotation(_initalPos,_initalRot);
    }
}
