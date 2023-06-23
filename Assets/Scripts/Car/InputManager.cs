using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private float _horizontal;
    private float _vertical;
    
    public float GetHorizontal { get; private set; }
    public float GetVertical { get; private set; }
    void Update()
    {
        GetHorizontal = Input.GetAxisRaw("Horizontal");
        GetVertical = Input.GetAxisRaw("Vertical");
    }
}
