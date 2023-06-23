using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handler : MonoBehaviour
{
   private PathFinding _pathFinding;
   public GameObject carPrefab;
   public GameObject cameraHandler;

   GameObject go;
   [SerializeField] private int speed;
   [SerializeField] private int rotationSpeed;
   private void Start()
   {
      PathFinding.CreateCar += PathFindingOnCreateCar;
   }

   private void LateUpdate()
   {
       if (go != null)
       {
           Camera.main.fieldOfView = 60;
           Vector3 targetPos = go.transform.GetChild(2).transform.position;
           Vector3 newPos = Vector3.Lerp(cameraHandler.transform.position, targetPos, Time.deltaTime * speed);
           cameraHandler.transform.position = newPos;
           cameraHandler.transform.rotation = Quaternion.identity;
           cameraHandler.transform.LookAt(go.transform);
       }
   }
   

   private void PathFindingOnCreateCar(Vector3 position)
   {
       go = Instantiate(carPrefab, position, Quaternion.identity);
       go.SetActive(true);
       cameraHandler.GetComponent<Navigate>().enabled = false;
   }
}
