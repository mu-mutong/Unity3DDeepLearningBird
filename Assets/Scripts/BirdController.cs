using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour {
    public bool BirdIslive;
    public RaycastHit2D[] hit;
    public float Up_force = 200f;
    public double[] Dis;
    public DeepNet Dns;
    public MeshRenderer mMesh;
    public bool flag = true;

    private double Getpoint;
    private Rigidbody2D rg;
    private int Score;
    private Vector2 spawnp = new Vector2(-6f, 0f);
    private float lasttime = 0f;
    private float livetime=0f;
    private BirdController Tm;

    // Use this for initialization
    void Awake () {
        BirdIslive = true;
        Dis = new double[3];
        hit = new RaycastHit2D[3];
        rg = GetComponent<Rigidbody2D>();
        Score = 0;
        BuildDeepnet();
        flag = true;
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        Dis = Distance();
        Getpoint = GetOutput(Dis);
        gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    }
    private double [] Distance() {
        double [] dis = new double[3];
        hit[0] = Physics2D.Raycast(transform.position, transform.right, 20f, 1 << LayerMask.NameToLayer("Test"));
        hit[1] = Physics2D.Raycast(transform.position, transform.up, 20f, 1 << LayerMask.NameToLayer("Test"));
        hit[2] = Physics2D.Raycast(transform.position, -transform.up, 20f, 1 << LayerMask.NameToLayer("Test"));
        for(int i = 0; i < 3; i++) {
            dis[i] = Vector2.Distance(transform.position, hit[i].point)/5;
        }
        
      
        //Debug.Log("right:" + dis[0]+ "up:" + dis[1]+ "down:" + dis[2]);
       
        Debug.DrawRay(transform.position, transform.right, Color.green, Time.deltaTime, true);
        Debug.DrawRay(transform.position, transform.up, Color.green, Time.deltaTime, true);
        Debug.DrawRay(transform.position, -transform.up, Color.green, Time.deltaTime, true);
        return dis;
    }
    private void BuildDeepnet()
    {
        Dns = new DeepNet(3, new int[] { 8 }, 1);
        double[] wts = new double[Dns.wNum];
        for (int j = 0; j < Dns.wNum; j++)
        {
            if (j < Dns.wNum - Dns.bNum)
            {
                wts[j] = Random.Range(0, 1f);
            }
            else
            {
                wts[j] = Random.Range(0, 1f);
            }
        }
        Dns.SetWeights(wts);
    }
    private double GetOutput(double[] Input)
    {
        double[] n = new double[1];
        n = Dns.ComputeOutputs(Input);
        return n[0];
    }
    public void Move()
    {
        Dis = Distance();
        //Debug.Log(this.gameObject.tag+GetOutput(Dis));
        if (GetOutput(Dis) > 0.5)
        {
            float n= (float)GetOutput(Dis) * Up_force;
            rg.AddForce(Vector2.up * n);
            
        }
    }
    public void AddScore()
    {
        Score++;
    }
    public float  ShowScore()
    {
        float n = Time.time-lasttime;
        lasttime = Time.time;

        if (livetime!=0)
        {
            float x = n;
            n = n / livetime;
            livetime = x;
        }
        


        return Score*n;
    }
    public double[] GetOldWeights()
    {
        return Dns.GetWeights();
    }
    public void SetNewWeights(double[] wts)
    {
        Dns.SetWeights(wts);
    }
    public void Restart()
    {
        //Debug.Log(tag+"Last Score "+Score+" Restart");
        spawnp.x = Random.Range(-5f, -1f);
        spawnp.y = Random.Range(-3f, 3f);
        gameObject.GetComponent<Transform>().position = spawnp;
        gameObject.layer = LayerMask.NameToLayer("Default");
        BirdIslive = true;
        flag = true;
        Score = 0;
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        string[] BirdTag = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        for(int i = 0; i < 10; i++)
        {
            Tm = GameObject.FindGameObjectWithTag(BirdTag[i]).GetComponent<BirdController>();
            if (coll.gameObject.tag == BirdTag[i]&&!Tm.BirdIslive)
            {
                float xx;
                float yy;
                float zz;
                float loos = 2f;
                xx = transform.position.x;
                yy = transform.position.y;
                zz = transform.position.z;
                xx +=loos;
                transform.position=new Vector3(xx,yy,zz);
            }
        }
        if (coll.gameObject.tag == "wall")
        {

            float xx;
            float yy;
            float zz;
            
            xx = transform.position.x;
            yy = transform.position.y;
            zz = transform.position.z;
            xx = -15f;
            yy = -10f;
            transform.position = new Vector3(xx, yy, zz);
            gameObject.layer = LayerMask.NameToLayer("Test");
            BirdIslive = false;
        }
    }
}
