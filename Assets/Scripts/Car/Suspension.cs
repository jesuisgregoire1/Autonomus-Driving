using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Suspension : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]private float _force  = 0;
    private Transform _forceTransform;
    private CarController _carController;
    private int layerGround = 6;
    [SerializeField]private WheelProp wheel;
    [SerializeField]private GameObject forcePoint;

    #region String
    [SerializeField] public float restLengthLength;   
    [SerializeField] private float actualLength = 0.00f;
    [SerializeField] private int stringStrength;
    #endregion
    #region Damper
    [SerializeField] private float damping;
    #endregion
    
    public WheelProp GetWheelProp => wheel;
    
    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        _carController = transform.root.GetComponent<CarController>();
    }
    
    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _carController.GetMaxDistance, 1 << layerGround))
        {
            
            var position = gameObject.transform.position;
            var dis = hit.point - position - Vector3.down * wheel.GetWheelRadius.y / 2;
            _forceTransform = forcePoint.transform;
            _forceTransform.position = dis + position;
            _force = StringForce(restLengthLength, stringStrength, dis.magnitude) + DampedForce(_forceTransform.up, _forceTransform.position);
            rb.AddForceAtPosition( -_forceTransform.up *  (_force * Time.fixedDeltaTime), dis+position);
            Debug.DrawRay(position, hit.point - position - Vector3.down * wheel.GetWheelRadius.y / 2,
                new Color(0.96f, 0.57f, 1f));
        }
    }
    private float StringForce(float restLength, int stringStrength, float distance)
    {
        float offSet = 0;
        actualLength = distance;
        offSet = actualLength - restLength;
        return stringStrength * offSet;
    }
    private float DampedForce(Vector3 up, Vector3 position)
    {
        return Vector3.Dot(up, rb.GetPointVelocity(position)) * damping;
    }
}
