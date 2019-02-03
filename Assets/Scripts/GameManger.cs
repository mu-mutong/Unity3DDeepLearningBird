using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour {

    public int BirdliveNum;
    public GameObject MotherBird;
    public int Birdnum=2;
    public string[] BirdTag;
    public float Max = 0;


    private static bool[] BirdLives;
    
    private GameObject[] Childbirds;
    private BirdController Tm;
    private bool Flag = false;
    private Vector2 spawnp = new Vector2(-6f, 0f);
    // Use this for initialization
    void Awake () {
        bool[] BirdLives = new bool[Birdnum];
        
        BuildChildbirds(Childbirds);
        BirdliveNum = Birdnum;
       
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        

        BirdMove();
           
        if (BirdliveNum == 0)
        {
            Debug.Log("End");
            Restart();
            BirdliveNum = Birdnum;
        }
    }
   
    private void BuildChildbirds(GameObject[] Childbirds)
    {
        string[] BirdTag = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        Childbirds = new GameObject[Birdnum];
        for(int i = 0; i < Birdnum; i++)
        {
            spawnp.x = Random.Range(-5f, -1f);
            spawnp.y = Random.Range(-3f, 3f);
            Childbirds[i]= (GameObject)Instantiate(MotherBird, spawnp, Quaternion.identity);
            Childbirds[i].gameObject.tag = BirdTag[i];
            Childbirds[i].SetActive(true);
        }
    }
    private void SetBirdlive(bool[] BirdIslive)
    {
        BirdIslive = new bool[Birdnum];
        for(int i = 0; i < Birdnum; i++)
        {
            BirdIslive[i] = true;
        }

    }
    public void BirdMove()
    {
        string[] BirdTag = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        for (int i = 0; i < Birdnum && BirdliveNum > 0; i++)
        {
            Tm = GameObject.FindGameObjectWithTag(BirdTag[i]).GetComponent<BirdController>();
            if (Tm.BirdIslive)
            {
                Tm.Move();
            }else 
            {
                if (Tm.flag)
                {
                    BirdliveNum--;
                   // Debug.Log(BirdliveNum);
                    Tm.flag = false;
                   
                }
                
            }
        }
    }
    private void Restart()
    {
        float[] BirdScore = new float[Birdnum];
        double[][] Weights = new double[Birdnum][];
        
        string[] BirdTag = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        for (int i = 0; i < Birdnum; i++)
        {
            
            Tm = GameObject.FindGameObjectWithTag(BirdTag[i]).GetComponent<BirdController>();
            BirdScore[i] = Tm.ShowScore();
            Weights[i] = Tm.GetOldWeights();
            Tm.Restart();
        }
        SortWeights(BirdScore,Weights);

        Learn(BirdScore,Weights);
        for (int i = 0; i < Birdnum; i++)
        {
            if (BirdScore[i]>Max)
            {
                Max = BirdScore[i];
                Debug.Log("The max score: " + Max);
            }
           
            Tm = GameObject.FindGameObjectWithTag(BirdTag[i]).GetComponent<BirdController>();

            Tm.SetNewWeights(Weights[i]);
        }
    }
    private void SortWeights(float[] score,double[][] weights)
    {
        for(int i = 0; i < Birdnum; i++)
        {
            for(int j=i+1;j<Birdnum; j++)
            {
                if (score[i] < score[j])
                {
                    float n = 0;
                    double[] m;
                    n = score[i];
                    m = weights[i];
                    score[i] = score[j];
                    weights[i] = weights[j];
                    score[j] = n;
                    weights[j] = m;
                }
            }
        }
    }
    private  void Learn(float[] BirdScore, double[][] weights)
    {
        
        weights = SeletPop(BirdScore, weights);
        CrossPoP(weights);
        VaryPop(BirdScore,weights);
    }
    private double[] Crossover(double[]p1,double[]p2 )
    {
        double[] wts=new double[p1.Length];
        for(int i = 0; i < p1.Length; i++)
        {
            if(System.Math.Abs(p2[i] - p1[i])< 0.001)
            {
                wts[i] = (Random.Range(0, 5) - 0.25f) * p1[i];
            }
            else
            {
                wts[i] = p1[i] + (p2[i] - p1[i]) * Random.Range(-0.5f, 1.5f);
            }
        }
        return wts;
    }
    private void  Mutate(double[] wts)
    {
        for(int j = 0; j < wts.Length; j++)
        {
            if (Random.Range(0, 1f) > 0.5f)
            {
                wts[j] = (Random.Range(-(.5f), (.5f)));
            }
        }
        
    }
    //线性排名选择
    private double[][] SeletPop(float[] score, double[][] weight)
    {
        double Pa = 1.1,Pb=0.2;
        double[] p = new double[Birdnum];
        double[] selection = new double[Birdnum];
        double[][] newwts=new double[Birdnum][];
        double FitSum = 0;
        //计算分配概率
        for (int i = 0; i < Birdnum; i++)
                {
                    int j = i + 1;
                    p[i] = (Pa - Pb / (j + 1)) / j;
                 }
        //求分配概率的总和
        for (int index = 0; index < Birdnum; index++)
        {
                     FitSum += p[index];
        }
        //确定轮盘分布
        for (int index = 0; index < Birdnum; index++)
               {
                    selection[index] = p[index] / FitSum;
        }
        for (int index = 1; index < Birdnum; index++)
        {
                   selection[index] = selection[index] + selection[index - 1];
        }
           //用轮盘进行随机选取，形成新的种群
        for (int popindex = 0; popindex < Birdnum; popindex++)
        {
                double n = Random.Range(0, 100f) / 100.0;
                int index = 0;
                while (n > selection[index])
                {
                    index++;
                }
                newwts[popindex] = weight[index];
        }
        return newwts;
    }
    //杂交算子,离散杂交
    private void CrossPoP(double[][] weight)
    {
        int index = 0,position = 0,i = 0, popSize=Birdnum,chromSize= weight[0].Length;
        double t = 0, temp = 0;
        for (index=0; index < popSize; index++)
        {
            double[] n = new double[weight[i].Length];
            int r =Random.Range(0,popSize);
            n = weight[index];
            weight[index] = weight[r];
            weight[r] = n;
        }
        for (index = 0; index < popSize; index += 2)
        {
            t = Random.Range(0f, 1f);
            if (t < 0.7)
            {
                position = Random.Range(0, chromSize);
                for (i = position + 1; i < chromSize; i++)
                {
                    temp = weight[index + 1][i];
                    weight[index + 1][i] = weight[index][i];
                    weight[index][i] = temp;
                    
                }
                
            }
            
        }
    }
    private void VaryPop(float[] score,double[][] weight)
    {
        int i = 0, j = 0, popSize = Birdnum, chromSize = weight[0].Length;
        double Pm = 0.3,up = 1f,down = -1f;
        for (i = 0; i < popSize; i++)
        {
            for (j = 0; j < chromSize; j++)
            {
                double r = Random.Range(0f, 1f);
                if (r < Pm)
                {
                    double t = 1 - score[i] * 0.9999 /200;
                              //突变区间
                    double u = (1 - System.Math.Pow(r, System.Math.Pow(t, 2))) * (up - weight[i][j]);
                    if (u > up)
                    {
                        u = up;
                    }
                    if (u < down)
                    {
                        u = down;
                    }
                    double l = (1 - System.Math.Pow(r, System.Math.Pow(t, 2))) * (weight[i][j] - down);
                    if (l > up)
                    {
                        l = up;
                    }
                    if (l < down)
                    {
                        l = down;
                    }
                   
                    int p = Random.Range(0, 9)%2;
                    if (p == 0)
                    {
                        weight[i][j] = u;
                    }
                    else
                    {
                        weight[i][j] = l;
                    }
                }
            }
        }
    }
    /* 
    （1）非一致性变异

在传统的遗传算法中，突变的情况是与代数无关的。但是进化刚开始时，
就是需要向各个方向大步发展进行尝试，进化到了后期，解已经相对较优了，
进行局部搜索可能更有利于找到更好的解。显然传统的方法是不行的，
必须找到一种将变异幅度和代数相联系的策略。

所以给出如下方法：s={v1,v2,v3,v4……},vk被选中进行变异，它的定义区间为{ak,bk},

vk1=vk+h(t,bk-vk);  vk2=vk-h(t,vk-ak);

（t为代数，h（t,y）=y*(1-r^(1-t/T)^p),r是0~1的随机数，T为最大代数，p式一个参数，
一般取值为2~5）

新的vk随机在vk1和vk2中进行选取，这样就实现了之前提出要求。

（2）自适应性变异

非一致性变异加强了局部搜索能力，但是与解的好坏无关，
但是我们可能希望的是好的解搜索范围较小，坏的解搜索范围较大这样，
所以在非一致性变异上进行一些修正。

我们只需要将h(t,y)中的t换为T，T是一个与解的质量有关的数，

T=1-f(s)/fmax

f(s)是某个体的适应值，fmax是所解问题的最优结果，
当然fmax不太好确定，所以找一个近似替代就可以了，比如当代最优解或是历史最优解。


     */

}
