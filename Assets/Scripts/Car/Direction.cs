using System;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Serialization;

public class Direction : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private GameObject forcePoint;
    private CarController _carController;
    [SerializeField] private float tireGripFactor;
    [SerializeField]private float tireMass;

    private void Start()
    {
        _rb = transform.root.GetComponent<Rigidbody>();
        _carController = transform.root.GetComponent<CarController>();
    }
    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _carController.GetMaxDistance))
        {
            Vector3 steeringDir = forcePoint.transform.right;
            Vector3 tireWorldVel = _rb.GetPointVelocity(forcePoint.transform.position);
            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
            float desiredVelChange = -steeringVel * tireGripFactor;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            _rb.AddForceAtPosition(steeringDir * (tireMass * desiredAccel * Time.fixedDeltaTime),forcePoint.transform.position);
        }
    }
}
