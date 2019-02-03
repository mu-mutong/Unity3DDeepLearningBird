using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnMaster : MonoBehaviour {
    public GameObject Column;
    public float colddown = 0.5f;
    public float min = -3f, max = 3f;
    public int poolSize = 6;
    public float spawnRate = 4.5f;
    
    float nextSpawn = 0;

    private GameObject[] columns;
    private int currentIndex = 0;
    private float spawnXPosition = 10f;
    private Vector2 spawnp = new Vector2(10f, 0f);

    // Use this for initialization
    void Awake () {
        columns = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            columns[i] = (GameObject)Instantiate(Column, spawnp, Quaternion.identity);
            columns[i].SetActive(false);
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        spawnRate = Random.Range(0, 1f);
       
        if (Time.time - spawnRate > nextSpawn)
        {
            colddown = 4.5f;
            
            nextSpawn = Time.time + colddown; 
            Vector3 spawnp = transform.position;
            spawnp.y += Random.Range(min, max);
            columns[currentIndex].transform.position = new Vector2(spawnXPosition, spawnp.y);  // reset position
            columns[currentIndex].SetActive(true);
            //Instantiate(columns[currentIndex], spawnp,transform.rotation);
            currentIndex++;
            if (currentIndex >= poolSize)
            {                 // check overflow
                currentIndex = 0;
            }
        }

    }
}
