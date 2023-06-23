using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Tires[] wheels;
    public Brain _brain;
    [Header("Car Specs")] 
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;
    
    //[Header("Inputs")] public float steerInput;
    [SerializeField]private float ackermannAngleLeft;
    [SerializeField]private float ackermannAngleRight;

    [SerializeField]private float maxDistance;
    
    public float GetMaxDistance => maxDistance;
    void FixedUpdate()
    {
        //steerInput = Input.GetAxis("Horizontal");
        
        if (_brain.steerRight > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * _brain.steerRight;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * _brain.steerRight;
        }
        if (_brain.steerLeft < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * _brain.steerLeft;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * _brain.steerLeft;
        }
        else if(_brain.steerLeft == 0 && _brain.steerRight == 0)
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach (var wheel in wheels)
        {
            if (wheel.wheelFrontLeft)
                wheel.steerAngle = ackermannAngleLeft;
            if (wheel.wheelFronRigth)
                wheel.steerAngle = ackermannAngleRight;
           
        }
    }
}
