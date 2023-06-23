using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PopulationManager : MonoBehaviour {

	public GameObject botPrefab;
	public GameObject startingPos;
	public int populationSize = 50;
	List<GameObject> population = new List<GameObject>();
	public static float elapsed = 0;
	public float trialTime = 5;
	int generation = 1;

	GUIStyle guiStyle = new GUIStyle();
	void OnGUI()
	{
		guiStyle.fontSize = 25;
		guiStyle.normal.textColor = Color.white;
		GUI.BeginGroup (new Rect (10, 10, 250, 150));
		GUI.Box (new Rect (0,0,140,140), "Stats", guiStyle);
		GUI.Label(new Rect (10,25,200,30), "Gen: " + generation, guiStyle);
		GUI.Label(new Rect (10,50,200,30), string.Format("Time: {0:0.00}",elapsed), guiStyle);
		GUI.Label(new Rect (10,75,200,30), "Population: " + population.Count, guiStyle);
		GUI.EndGroup ();
	}


	// Use this for initialization
	void Start () {
		for(int i = 0; i < populationSize; i++)
		{
			GameObject b = Instantiate(botPrefab, startingPos.transform.position, this.transform.rotation);
			b.GetComponent<BrainGA>().Init();
			population.Add(b);
		}

		Time.timeScale = 5;
	}

	GameObject Breed(GameObject parent1, GameObject parent2)
	{
		GameObject offspring = Instantiate(botPrefab, startingPos.transform.position, this.transform.rotation);
		BrainGA b = offspring.GetComponent<BrainGA>();
		if(Random.Range(0,100) == 1) //mutate 1 in 100
		{
			b.Init();
			b.dna.Mutate();
		}
		else
		{ 
			b.Init();
			b.dna.Combine(parent1.GetComponent<BrainGA>().dna,parent2.GetComponent<BrainGA>().dna);
		}
		return offspring;
	}

	void BreedNewPopulation()
	{
		List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<BrainGA>().distanceTravelled -
		                                                      o.GetComponent<BrainGA>().crash)).ToList();
		
		population.Clear();
		for (int i = (int) (3*sortedList.Count / 4.0f) - 1; i < sortedList.Count - 1; i++)
		{
    		population.Add(Breed(sortedList[i], sortedList[i + 1]));
    		population.Add(Breed(sortedList[i + 1], sortedList[i]));
    		population.Add(Breed(sortedList[i], sortedList[i + 1]));
    		population.Add(Breed(sortedList[i + 1], sortedList[i]));
		}


		

		//destroy all parents and previous population
		for(int i = 0; i < sortedList.Count; i++)
		{
			Destroy(sortedList[i]);
		}
		generation++;
	}
	
	// Update is called once per frame
	void Update () {
		elapsed += Time.deltaTime;
		if(elapsed >= trialTime)
		{
			BreedNewPopulation();
			elapsed = 0;
		}
	}
}
