using System;
using UnityEngine;

public class WheelProp : MonoBehaviour
{
    public Vector3 GetWheelRadius { get; private set; }
    
    public void WheelRadius()
    {
        GetWheelRadius = gameObject.GetComponent<Renderer>().bounds.size;
    }

    private void Start()
    {
        WheelRadius();
    }
}
