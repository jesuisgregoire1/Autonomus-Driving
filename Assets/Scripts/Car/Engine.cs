using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Engine : MonoBehaviour
{
    [SerializeField] private float horsePower;
    [SerializeField] private float rpm;
    [SerializeField] private float speed;
    [SerializeField] private float gear;
    [SerializeField] private float force;
    private Rigidbody _rb;
    private Dictionary<int, int> engine = new Dictionary<int, int>();
    private List<int> revolutionsPerMinute = new List<int>() {800, 1312, 1800, 2276,
        2800, 3316, 3806, 4300 , 4770, 5300 , 5800, 6300};
    private List<int> torque = new List<int>() { 116, 135, 148, 157, 165, 172, 178, 184,
        188, 187, 183, 171 };
    [SerializeField] private GameObject forcePoint;
    private InputManager _inputManager;
    [SerializeField] private int resistenceForce;
    public Brain brain;
    public List<int> GetRevolutions
    {
        get
        {
            return revolutionsPerMinute;
        }
    }

    public List<int> GetTorque
    {
        get
        {
            return torque;
        }
    }

    public Dictionary<int, int> GetEngineProperties
    {
        get
        {
            return engine;
        }
    }

    private CarController _carController;
    private void Awake()
    {
        if(torque.Count == revolutionsPerMinute.Count)
        {
            for(int i = 0; i < revolutionsPerMinute.Count; ++i)
            {
                engine.Add(torque[i], revolutionsPerMinute[i]);
            }
        }
        else
        {
            throw new Exception("The lists length must be the same");
        }
    }

    private void Start()
    {
        _rb = gameObject.transform.root.GetComponent<Rigidbody>();
        _carController = transform.root.GetComponent<CarController>();
        _inputManager = transform.root.GetComponent<InputManager>();
    }

    private void FixedUpdate()
    {
        if(Physics.Raycast(transform.position, -transform.up,out RaycastHit hit,_carController.GetMaxDistance))
        {
            //print("force = " + brain.forward);
            _rb.AddForceAtPosition(forcePoint.transform.forward * (brain.forward * (force *Time.fixedDeltaTime)), forcePoint.transform.position);
        }
    }
    
}
