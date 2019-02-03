using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour {
    public float speed = 2f;


    private float rad = 0;
    private BirdController Tm;
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void FixedUpdate () {
       // if (GameManger.BirdliveNum > 0)
       // {
            rad = Random.Range(1f, 2f);
            transform.Translate(Vector3.left * speed * Time.deltaTime * rad);//保证执行频率，防止移动过快
       // }
      
       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string[] BirdTag = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        for(int i = 0; i < 10; i++)
        {
            if (collision.gameObject.tag == BirdTag[i])
            {
                Tm = GameObject.FindGameObjectWithTag(BirdTag[i]).GetComponent<BirdController>();
                Tm.AddScore();
                //Debug.Log(Tm.tag+" "+Tm.ShowScore());
            }
        }
       
    }
}
