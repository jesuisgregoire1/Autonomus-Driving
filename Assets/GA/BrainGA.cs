using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BrainGA : MonoBehaviour
{
	int DNALength = 5;
	public DNA dna;
    public GameObject eyes;
    bool seeDownWall = false; 
    bool seeUpWall = false; 
    bool seeBottom = false;
    bool seeTop = false;  
    Vector3 startPosition;  
    public float timeAlive = 0;
    public float distanceTravelled = 0;  
    public int crash = 0;
    bool alive = true;  
    Rigidbody2D rb;    
    
	public void Init()
	{
		//initialise DNA
        //0 forward
        //1 upwall
        //2 downwall
        //3 normal upward
        dna = new DNA(DNALength,200);
        this.transform.Translate(Random.Range(-1.5f,1.5f),Random.Range(-1.5f,1.5f),0);
        startPosition = this.transform.position;
        rb = this.GetComponent<Rigidbody2D>();
	}

    void OnCollisionEnter2D(Collision2D col)
    { 
        if (col.gameObject.tag == "dead")
        {
            alive = false;
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if(col.gameObject.tag == "dead" || 
           col.gameObject.tag == "top" ||
           col.gameObject.tag == "bottom" ||
           col.gameObject.tag == "upwall" ||
           col.gameObject.tag == "downwall")
        {
            crash++;
        }
    }


    void Update()
    {
        if(!alive) return;

        seeUpWall = false;
        seeDownWall = false;
        seeTop = false;
        seeBottom = false;
        RaycastHit2D hit = Physics2D.Raycast(eyes.transform.position, eyes.transform.forward, 1.0f);

        Debug.DrawRay(eyes.transform.position, eyes.transform.forward * 1.0f, Color.red);
        Debug.DrawRay(eyes.transform.position, eyes.transform.up* 1.0f, Color.red);
        Debug.DrawRay(eyes.transform.position, -eyes.transform.up* 1.0f, Color.red);

        if (hit.collider != null)
        {
            if(hit.collider.gameObject.tag == "upwall")
            {
                seeUpWall = true;
            }
            else if(hit.collider.gameObject.tag == "downwall")
            {
                seeDownWall = true;
            }
        }
		hit = Physics2D.Raycast(eyes.transform.position, eyes.transform.up, 1.0f);
		if (hit.collider != null)
        {
            if(hit.collider.gameObject.tag == "top")
            {
                seeTop = true;
            }
        }
        hit = Physics2D.Raycast(eyes.transform.position, -eyes.transform.up, 1.0f);
		if (hit.collider != null)
        {    
            if(hit.collider.gameObject.tag == "bottom")
            {
                seeBottom = true;
            }
        }
        timeAlive = PopulationManager.elapsed;
    }


    void FixedUpdate()
    {
        if(!alive) return;
        
        // read DNA
        float h = 0;
        float v = 1.0f; //dna.GetGene(0);

        if(seeUpWall)
        { 
            h = dna.GetGene(0);
        }
        else if(seeDownWall)
        {
        	h = dna.GetGene(1);
        }
        else if(seeTop)
        {
        	h = dna.GetGene(2);
        }        
        else if(seeBottom)
        {
        	h = dna.GetGene(3);
        }
        else
        {
        	h = dna.GetGene(4);
        }

        rb.AddForce(this.transform.right * v);
        rb.AddForce(this.transform.up * h * 0.1f);
        distanceTravelled = Vector3.Distance(startPosition,this.transform.position);
    }
}

