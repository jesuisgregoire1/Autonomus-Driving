using Unity.VisualScripting;
using UnityEngine;

public class Tires : MonoBehaviour
{

    private WheelProp _wheelProp;
    private CarController _carController;
    private InputManager _inputManager;
    private Suspension _suspension;
    private Engine _engine;
    public Transform _forcePoint;
    public bool wheelFrontLeft;
    public bool wheelFronRigth;
    public float steerAngle;
    
    private void Start()
    {
        _wheelProp = GetComponentInChildren<WheelProp>();
        _carController = transform.root.GetComponent<CarController>();
        _inputManager = transform.root.GetComponent<InputManager>();
        _suspension = transform.GetComponentInParent<Suspension>();
        _engine = transform.GetComponentInParent<Engine>();
    }

    private float _count = 0;
    [SerializeField]private float rotationSpeed;
    private float angle = 0;

    private void FixedUpdate()
    {
        _count += rotationSpeed * Time.deltaTime;
        if (Physics.Raycast(_suspension.transform.position, -_suspension.transform.up, out RaycastHit hit, _carController.GetMaxDistance))
        {
            transform.position = hit.point - Vector3.down * _wheelProp.GetWheelRadius.y/2;
        }
        else
        {
            transform.position = _suspension.transform.position + Vector3.down * _suspension.restLengthLength;
        }
        
        if (_carController._brain.steerRight > 0)
        {
            angle += rotationSpeed * Time.deltaTime;
            angle = Mathf.Clamp(angle, -steerAngle, steerAngle);
        }
        if (_carController._brain.steerLeft < 0)
        {
            angle -= rotationSpeed * Time.deltaTime;
            angle = Mathf.Clamp(angle, steerAngle, -steerAngle);
        }
        else if (_carController._brain.steerLeft == 0 && _carController._brain.steerRight == 0)
        {
            if (angle >= 0)
            {
                angle -= rotationSpeed * Time.deltaTime;
                angle = Mathf.Clamp(angle, 0,angle);
            }
            else if(angle <= 0)
            {
                angle += rotationSpeed * Time.deltaTime;
                angle = Mathf.Clamp(angle, angle,0);
            }
        }
        angle += rotationSpeed * Time.deltaTime;
        angle = Mathf.Clamp(angle, -steerAngle, steerAngle);
        //transform.localEulerAngles = new Vector3(_inputManager.GetVertical*_count, angle, 0f);
        _forcePoint.localEulerAngles = new Vector3(0, angle, 0);

    }
}
