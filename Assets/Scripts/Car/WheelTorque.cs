using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTorque : MonoBehaviour
{
    private Transform _parentGo;
    private Engine _engineProperties;
    private int _distance = 2;
    private WheelProp _wheel;
    [SerializeField] private int speed;
    [SerializeField] private GameObject wheel1, wheel2;

    private void Start()
    {
        _parentGo = gameObject.transform.parent.gameObject.transform;
        _engineProperties = _parentGo.GetComponent<Engine>();
        _wheel = GetComponent<Suspension>().GetWheelProp;
    }
    private void FixedUpdate()
    {
        if(Physics.Raycast(gameObject.transform.position, Vector3.down,
            out RaycastHit hitPoint, _distance))
        {
            print(hitPoint.transform.name);
            Debug.DrawRay(gameObject.transform.position, hitPoint.point - gameObject.transform.position
                -new Vector3(0, _wheel.GetWheelRadius.y/2, 0), Color.red);
            //AddTorque(transform.root.GetComponent<Rigidbody>(), hitPoint.point);
            //print("radius = " + _wheel.GetWheelRadius);
            //print("hit= " + hitPoint.point);
            
        }
        //transform.root.GetComponent<Rigidbody>().AddForce(Vector3.forward * speed * Time.deltaTime);

    }
    private void AddTorque(Rigidbody rb, Vector3 position)
    {
        rb.AddForceAtPosition(Vector3.forward * speed * Time.deltaTime, position);
        //rb.AddTorque(Vector3.forward * 100 * Time.deltaTime);
    }
}
