using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using Unity.VisualScripting;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;


public class ReplayCar
{
	public List<double> states;
	public double reward;

	public ReplayCar(double vertical, double horizontalRight, double horizontalDiagonalRight, double horizontalLeft,
		double horizontalDiagonalLeft, double xPos, double zPos, double xRefPos, double yRefPos, double r)
	{
		states = new List<double>();
		states.Add(vertical);
		states.Add(horizontalRight);
		states.Add(horizontalDiagonalRight);
		states.Add(horizontalLeft);
		states.Add(horizontalDiagonalLeft);
		states.Add(xPos);
		states.Add(zPos);
		states.Add(xRefPos);
		states.Add(yRefPos);
		reward = r;
	}
}


public class Brain : MonoBehaviour {

	ANN ann;

	float reward = 0.0f;							//reward to associate with actions
		
	private List<ReplayCar> replayMemoryCar = new List<ReplayCar>();//memory - list of past actions and rewards
	int mCapacity = 10000;							//memory capacity
	
	float discount = 0.99f;							//how much future states affect rewards
	float exploreRate = 100.0f;						//chance of picking random action
	float maxExploreRate = 100.0f;					//max chance value
    float minExploreRate = 0.1f;					//min chance value
    float exploreDecay = 0.01f;					//chance decay amount for each update

	Vector3 ballStartPos;							//record start position of object
	private Vector3 carStartPosition;
	int failCount = 0;								//count when the ball is dropped
	private float speed;
	float tiltSpeed = 0.5f;						    //max angle to apply to tilting each update
													//make sure this is large enough so that the q value
													//multiplied by it is enough to recover balance
													//when the ball gets a good speed up
	float timer = 0;								//timer to keep track of balancing
	float maxBalanceTime = 0;						//record time ball is kept balanced	
	private float maxTime = 0;

	private PathFinding _pathFinding;

	[SerializeReference]public List<Node> path;

	public float forward;

	public float steerRight;

	public float steerLeft;

	public float noSteer;

	private float distanceMeasurement;
	// Use this for initialization
	void Start ()
	{
		ann = new ANN(9,3,2,4,0.2f);
		Time.timeScale = 5.0f;
		_pathFinding = FindObjectOfType<PathFinding>();
		path = _pathFinding.path;
		carStartPosition = path[0].worldPosition;
		distanceMeasurement = Mathf.Abs(Vector3.Distance(this.transform.position, path[counter].worldPosition));
	}

	GUIStyle guiStyle = new GUIStyle();
	void OnGUI()
	{
		guiStyle.fontSize = 25;
		guiStyle.normal.textColor = Color.white;
		GUI.BeginGroup (new Rect (10, 10, 600, 230));
		GUI.Box (new Rect (0,0,140,140), "Stats", guiStyle);
		GUI.Label(new Rect (10,25,500,30), "Fails: " + failCount, guiStyle);
		GUI.Label(new Rect (10,50,500,30), "ExploreRate " + exploreRate, guiStyle);
		GUI.Label(new Rect (10,75,500,30), "Time no hit: " + maxBalanceTime, guiStyle);
		GUI.Label(new Rect (10,100,500,30), "CT no hit: " + timer, guiStyle);
		GUI.Label(new Rect (10,125,500,30), "distanceToPoint: " + distanceMeasurement, guiStyle);
		GUI.Label(new Rect (10,150,500,30), "CurrentDistanceToPoint: " + Vector3.Distance(this.transform.position, path[counter].worldPosition), guiStyle);
		GUI.Label(new Rect (10,175,500,30), "Reward: " + reward, guiStyle);
		GUI.Label(new Rect (10,200,500,30), "Angle: " + Vector3.Dot(transform.forward, path[counter].worldPosition), guiStyle);
		
		GUI.EndGroup ();
	}

	public int visibleDistance = 20;
	double verticalDistance;
	double horizontalRightDistance;
	double horizontalDiagonalRightDistance;
	double horizontalLeftDistance;
	double horizontalDiagonalLeftDistance;

	public bool stayCollision;
	public bool enterCollision
		;
	// Update is called once per frame
	void Update ()
	{
		//print("Here");
		if (Physics.Raycast(gameObject.transform.position, transform.forward,out RaycastHit hitVertical, visibleDistance))
		{
			//verticalDistance = 1-hitVertical.distance/visibleDistance;
			Debug.DrawRay(transform.position, Vector3.forward * hitVertical.distance, Color.green);
			verticalDistance = hitVertical.distance;	
		}
		if (Physics.Raycast(gameObject.transform.position, transform.right,out RaycastHit hitHorizontalRight, visibleDistance))
		{
			//horizontalRightDistance = 1-hitHorizontalRight.distance/visibleDistance;
			Debug.DrawRay(transform.position, transform.right * hitHorizontalRight.distance, Color.green);
			horizontalRightDistance = hitHorizontalRight.distance;
		}
		if (Physics.Raycast(gameObject.transform.position, -transform.right,out RaycastHit hitHorizontalLeft, visibleDistance))
		{
			//horizontalLeftDistance = 1-hitHorizontalLeft.distance/visibleDistance;
			Debug.DrawRay(transform.position, -transform.right * hitHorizontalLeft.distance, Color.green);
			horizontalLeftDistance = hitHorizontalLeft.distance;
		}
		if (Physics.Raycast(gameObject.transform.position, Quaternion.AngleAxis(45,transform.up)*this.transform.forward,out RaycastHit hitHorizontalRightDiagonal, visibleDistance))
		{
			//horizontalDiagonalRightDistance = 1-hitHorizontalRightDiagonal.distance/visibleDistance;
			horizontalDiagonalRightDistance = hitHorizontalRightDiagonal.distance;
			Debug.DrawRay(transform.position, Quaternion.AngleAxis(45,transform.up)*this.transform.forward * hitHorizontalRightDiagonal.distance, Color.green);
		}
		if (Physics.Raycast(gameObject.transform.position, Quaternion.AngleAxis(-45,transform.up)*this.transform.forward,out RaycastHit hitHorizontalLeftDiagonal, visibleDistance))
		{
			//horizontalDiagonalLeftDistance = 1-hitHorizontalLeftDiagonal.distance/visibleDistance;
			horizontalDiagonalLeftDistance = hitHorizontalLeftDiagonal.distance;
			Debug.DrawRay(transform.position, Quaternion.AngleAxis(-45,transform.up)*this.transform.forward * hitHorizontalLeftDiagonal.distance, Color.green);
		}
		
		
		if (Input.GetKeyDown("space"))
		{
			ResetCarPosition();
		}
	}
	
	private int counter = 5;
	
	void FixedUpdate()
	{
		timer += Time.deltaTime;
		List<double> states = new List<double>();
		List<double> qs = new List<double>();
		states.Add(verticalDistance);
		states.Add(horizontalRightDistance);
		states.Add(horizontalDiagonalRightDistance);
		states.Add(horizontalLeftDistance);
		states.Add(horizontalDiagonalLeftDistance);
		states.Add(transform.position.x);
		states.Add(transform.position.z);
		states.Add(path[counter].worldPosition.x);
		states.Add(path[counter].worldPosition.z);
		
		qs = SoftMax(ann.CalcOutput(states));
		double maxQ = qs.Max();
		int maxQIndex = qs.ToList().IndexOf(maxQ);
		exploreRate = Mathf.Clamp(exploreRate - exploreDecay, minExploreRate, maxExploreRate);

		if (Random.Range(0, 100) < exploreRate)
			maxQIndex = Random.Range(0, 2);

		if (maxQIndex == 0)
		{
			forward = (float) qs[maxQIndex]*2;
		}
		else if (maxQIndex == 1)
		{
			steerRight = (float) qs[maxQIndex];
			steerLeft = 0;
		}
		else if (maxQIndex == 2)
		{
			steerLeft = (float) -qs[maxQIndex];
			steerRight = 0;
		}
		if (Vector3.Distance(this.transform.position, path[counter].worldPosition) < distanceMeasurement)
		{
			reward = 1/Vector3.Distance(this.transform.position, path[counter].worldPosition);
		}
		else if(Vector3.Distance(this.transform.position, path[counter].worldPosition) > distanceMeasurement)
		{
			reward = -50.0f;
		}
		if (Vector3.Dot(transform.forward, path[counter].worldPosition) < -1.5f)
		{
			reward = -50.0f;
		}
		if (stayCollision)
		{
			reward = -10.0f;
		}
		//else if (enterCollision)
		//{
		//	reward = -1f;
		//}


		ReplayCar lastReplayCarMemory = new ReplayCar(verticalDistance, horizontalRightDistance,
			horizontalDiagonalRightDistance, horizontalLeftDistance, horizontalDiagonalLeftDistance
			, transform.position.x, transform.position.z, path[counter].worldPosition.x, path[counter].worldPosition.z, reward);

		if (replayMemoryCar.Count > mCapacity)
			replayMemoryCar.RemoveAt(0);


		replayMemoryCar.Add(lastReplayCarMemory);

		if ((Math.Abs(transform.position.z - path[counter].worldPosition.z) < 0.5f 
		    && Mathf.Abs(transform.position.x - path[counter].worldPosition.x) < 0.5f) || stayCollision)
		{
			for (int i = replayMemoryCar.Count - 1; i >= 0; i--)
			{
				List<double> toutputsOld = new List<double>();
				List<double> toutputsNew = new List<double>();
				toutputsOld = SoftMax(ann.CalcOutput(replayMemoryCar[i].states));
				double maxQOld = toutputsOld.Max();
				int action = toutputsOld.ToList().IndexOf(maxQOld);
				double feedback;
				if (i == replayMemoryCar.Count - 1)
				{
					feedback = replayMemoryCar[i].reward;
				}
				else
				{
					toutputsNew = SoftMax(ann.CalcOutput(replayMemoryCar[i + 1].states));
					maxQ = toutputsNew.Max();
					feedback = (replayMemoryCar[i].reward +
					            discount * maxQ);
				}

				toutputsOld[action] = feedback;
				ann.Train(replayMemoryCar[i].states, toutputsOld);
			}

			if (timer > maxBalanceTime)
			{
				maxBalanceTime = timer;
			}

			timer = 0;

			this.transform.rotation = Quaternion.identity;
			
			replayMemoryCar.Clear();
			failCount++;
			if (!stayCollision && (path.Count - counter >= 5))
			{
				counter += 5;
				distanceMeasurement = Vector3.Distance(this.transform.position, path[counter].worldPosition);
			}
			else
			{
				ResetCarPosition();
			}
		}
	}
	
	private void ResetCarPosition()
	{
		gameObject.transform.position = carStartPosition + new Vector3(0,0.1f, 0);
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		stayCollision = false;
	}

	List<double> SoftMax(List<double> values) 
    {
      double max = values.Max();

      float scale = 0.0f;
      for (int i = 0; i < values.Count; ++i)
        scale += Mathf.Exp((float)(values[i] - max));

      List<double> result = new List<double>();
      for (int i = 0; i < values.Count; ++i)
        result.Add(Mathf.Exp((float)(values[i] - max)) / scale);

      return result; 
    }

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "Collision")
		{
			enterCollision = true;
		}
	}

	private void OnCollisionStay(Collision col)
	{
		if (col.gameObject.tag == "Collision")
		{
			stayCollision = true;
		}
	}

	private void OnCollisionExit(Collision col)
	{
		stayCollision = false;
		enterCollision = false;
	}
}
