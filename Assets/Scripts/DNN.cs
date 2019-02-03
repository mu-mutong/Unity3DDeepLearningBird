using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepNet{                               //基本神经网络类 来源网络 https://msdn.microsoft.com/zh-cn/magazine/mt493293
    public static Random rnd;
    public int nInput;
    public int[] nHidden;
    public int nOutput;
    public int nLayers;
    public double[] iNodes;
    public double[][] hNodes;
    public double[] oNodes;
    public double[][] ihWeights;
    public double[][][] hhWeights;
    public double[][] hoWeights;
    public double[][] hBiases;
    public double[] oBiases;
    public int wNum;
    public int bNum;
    public float score;
    public DeepNet(int numInput, int[] numHidden, int numOutput)
    {
        rnd = new Random();  // seed could be a ctor parameter

        this.nInput = numInput;
        this.nHidden = new int[numHidden.Length];
        for (int i = 0; i < numHidden.Length; ++i)
            this.nHidden[i] = numHidden[i];
        this.nOutput = numOutput;
        this.nLayers = numHidden.Length;

        iNodes = new double[numInput];
        hNodes = MakeJaggedMatrix(numHidden);
        oNodes = new double[numOutput];

        ihWeights = MakeMatrix(numInput, numHidden[0]);
        hoWeights = MakeMatrix(numHidden[nLayers - 1], numOutput);

        hhWeights = new double[nLayers - 1][][];  // if 3 h layer, 2 h-h weights[][]
        for (int h = 0; h < hhWeights.Length; ++h)
        {
            int rows = numHidden[h];
            int cols = numHidden[h + 1];
            hhWeights[h] = MakeMatrix(rows, cols);
        }

        hBiases = MakeJaggedMatrix(numHidden);  // pass an array of lengths
        oBiases = new double[numOutput];
        wNum = NumWeights(nInput, nHidden, nOutput);
    }
    public static double ReLU(double i)
    {
        double n = System.Math.Exp(i);
        double o = 1 / (1 + n);
        return o;
    }
    public static double[][] MakeJaggedMatrix(int[] cols)
    {
        // array of arrays using size info in cols[]
        int rows = cols.Length;  // num rows inferred by col count
        double[][] result = new double[rows][];
        for (int i = 0; i < rows; ++i)
        {
            int ncol = cols[i];
            result[i] = new double[ncol];
        }
        return result;
    }
    public static double[][] MakeMatrix(int rows, int cols)
    {
        double[][] result = new double[rows][];
        for (int i = 0; i < rows; ++i)
            result[i] = new double[cols];
        return result;
    }

    public int NumWeights(int numInput, int[] numHidden, int numOutput)
    {
        int ihWts = numInput * numHidden[0];

        int hhWts = 0;
        for (int j = 0; j < numHidden.Length - 1; ++j)
        {
            int rows = numHidden[j];
            int cols = numHidden[j + 1];
            hhWts += rows * cols;
        }
        int hoWts = numHidden[numHidden.Length - 1] * numOutput;

        int hbs = 0;
        for (int i = 0; i < numHidden.Length; ++i)
            hbs += numHidden[i];

        int obs = numOutput;
        int nw = ihWts + hhWts + hoWts + hbs + obs;
        bNum = hbs + obs;
        return nw;
    }

    public double[] GetWeights()
    {
        double[] wts = new double[wNum];
        int ptr = 0;  // pointer into wts[]

        for (int i = 0; i < nInput; ++i)  // input node
            for (int j = 0; j < hNodes[0].Length; ++j)  // 1st hidden layer nodes
                wts[ptr++] = ihWeights[i][j];

        for (int h = 0; h < nLayers - 1; ++h)  // not last h layer
        {
            for (int j = 0; j < nHidden[h]; ++j)  // from node
            {
                for (int jj = 0; jj < nHidden[h + 1]; ++jj)  // to node
                {
                    wts[ptr++] = hhWeights[h][j][jj];
                }
            }
        }

        int hi = this.nLayers - 1;  // if 3 hidden layers (0,1,2) last is 3-1 = [2]
        for (int j = 0; j < this.nHidden[hi]; ++j)
        {
            for (int k = 0; k < this.nOutput; ++k)
            {
                wts[ptr++] = hoWeights[j][k];
            }
        }

        for (int h = 0; h < nLayers; ++h)  // hidden node biases
        {
            for (int j = 0; j < this.nHidden[h]; ++j)
            {
                wts[ptr++] = hBiases[h][j];
            }
        }

        for (int k = 0; k < nOutput; ++k)
        {
            wts[ptr++] = oBiases[k];
        }
        return wts;
    }

    public void SetWeights(double[] wts)
    {
        int nw = wNum;  // total num wts + biases
        if (wts.Length != nw)
            Debug.LogError("wts erorr");
        int ptr = 0;  // pointer into wts[]

        for (int i = 0; i < nInput; ++i)  // input node
            for (int j = 0; j < hNodes[0].Length; ++j)  // 1st hidden layer nodes
                ihWeights[i][j] = wts[ptr++];

        for (int h = 0; h < nLayers - 1; ++h)  // not last h layer
        {
            for (int j = 0; j < nHidden[h]; ++j)  // from node
            {
                for (int jj = 0; jj < nHidden[h + 1]; ++jj)  // to node
                {
                    hhWeights[h][j][jj] = wts[ptr++];
                }
            }
        }

        int hi = this.nLayers - 1;  // if 3 hidden layers (0,1,2) last is 3-1 = [2]
        for (int j = 0; j < this.nHidden[hi]; ++j)
        {
            for (int k = 0; k < this.nOutput; ++k)
            {
                hoWeights[j][k] = wts[ptr++];
            }
        }

        for (int h = 0; h < nLayers; ++h)  // hidden node biases
        {
            for (int j = 0; j < this.nHidden[h]; ++j)
            {
                hBiases[h][j] = wts[ptr++];
            }
        }

        for (int k = 0; k < nOutput; ++k)
        {
            oBiases[k] = wts[ptr++];
        }
    }

    public double[] ComputeOutputs(double[] xValues)
    {

        for (int i = 0; i < nInput; ++i)  // possible trunc
            iNodes[i] = xValues[i];

        // zero-out all hNodes, oNodes
        for (int h = 0; h < nLayers; ++h)
            for (int j = 0; j < nHidden[h]; ++j)
                hNodes[h][j] = 0.0;

        for (int k = 0; k < nOutput; ++k)
            oNodes[k] = 0.0;

        // input to 1st hid layer
        for (int j = 0; j < nHidden[0]; ++j)  // each hidden node, 1st layer
        {
            for (int i = 0; i < nInput; ++i)
                hNodes[0][j] += ihWeights[i][j] * iNodes[i];
            // add the bias
            hNodes[0][j] -= hBiases[0][j];
            // apply activation
            hNodes[0][j] = ReLU(hNodes[0][j]);
        }

        // each remaining hidden node
        for (int h = 1; h < nLayers; ++h)
        {
            for (int j = 0; j < nHidden[h]; ++j)  // 'to index'
            {
                for (int jj = 0; jj < nHidden[h - 1]; ++jj)  // 'from index'
                {
                    hNodes[h][j] += hhWeights[h - 1][jj][j] * hNodes[h - 1][jj];
                }
                hNodes[h][j] -= hBiases[h][j];  // add bias value
                hNodes[h][j] = ReLU(hNodes[h][j]);  // apply activation
            }
        }

        // compute ouput node values
        for (int k = 0; k < nOutput; ++k)
        {
            for (int j = 0; j < nHidden[nLayers - 1]; ++j)
            {
                oNodes[k] += hoWeights[j][k] * hNodes[nLayers - 1][j];
            }
            oNodes[k] -= oBiases[k];  // add bias value
            oNodes[k] = ReLU(oNodes[k]);                          //Console.WriteLine("Pre-softmax output node [" + k + "] value = " + oNodes[k].ToString("F4"));
        }

        //double[] retResult = Softmax(oNodes);  // softmax activation all oNodes

        //for (int k = 0; k < nOutput; ++k)
        //    oNodes[k] = retResult[k];
        return oNodes;  // calling convenience

    }
}
