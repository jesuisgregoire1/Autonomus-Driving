using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigate : MonoBehaviour
{
    public Camera _camera;
    public int zoomSpeed;

    private void Start()
    {
        //_camera = gameObject.GetComponent<Camera>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(horizontalInput, verticalInput, 0) * 10 * Time.deltaTime);
        
        
    }

    private Vector3 move;
    void LateUpdate()
    {
        float val = Input.GetAxis("Mouse ScrollWheel");
        float currentZoom = _camera.fieldOfView + val;
        _camera.fieldOfView = Mathf.MoveTowards(_camera.fieldOfView, currentZoom, zoomSpeed*100 * Time.deltaTime);
       
    }
}
