using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float rotation;
    [SerializeField] float speed;
    Rigidbody wheelRb;
    void Start()
    {
        wheelRb = gameObject.GetComponent<Rigidbody>();    
    }

    void Update()
    {
        wheelRb.AddTorque(Vector3.right * speed * Time.deltaTime);
    }
}
