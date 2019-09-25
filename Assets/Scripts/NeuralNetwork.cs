using UnityEngine;
using System.Collections;

[System.Serializable]
public class NeuralNetwork
{
    [SerializeField]
    int numInputs;
    [SerializeField]
    int numHidden;
    [SerializeField]
    int numOutputs;

    [SerializeField]
    Matrix wih; // weights from input to hidden
    [SerializeField]
    Matrix who; // weights from hidden to output
    [SerializeField]
    Matrix bh1; // bias in hidden nodes 1
    [SerializeField]
    Matrix bo; // bias in output nodes


    public NeuralNetwork(int i, int h, int o)
    {
        numInputs = i;
        numHidden = h;
        numOutputs = o;

        wih = new Matrix(numHidden, numInputs);
        who = new Matrix(numOutputs, numHidden);
        bh1 = new Matrix(numHidden, 1);
        bo = new Matrix(numOutputs, 1);

        wih.Randomize();
        who.Randomize();
        bh1.Randomize();
        bo.Randomize();
    }

    public float[] FeedForward(float[] inputArray)
    {
        // converts input array into matrix
        Matrix inputMatrix = new Matrix(inputArray);

        // calculates through hidden nodes
        Matrix weightedHidden1 = wih.Dot(inputMatrix);
        Matrix weightedBiasedHidden1 = weightedHidden1.Add(bh1);
        Matrix hiddenOutput1 = weightedBiasedHidden1.Activate();

        // calculates through output nodes
        Matrix weightedOutput = who.Dot(hiddenOutput1);
        Matrix weightedBiasedOutput = weightedOutput.Add(bo);
        Matrix outputOutput = weightedBiasedOutput.Activate();

        // returns results of output
        return outputOutput.ToArray();
    }

    // make a baby
    public NeuralNetwork[] CrossOver(NeuralNetwork other)
    {
        NeuralNetwork[] results = new NeuralNetwork[2];
        NeuralNetwork result1 = new NeuralNetwork(numInputs, numHidden, numOutputs);
        NeuralNetwork result2 = new NeuralNetwork(numInputs, numHidden, numOutputs);
        results[0] = result1;
        results[1] = result2;
        result1.wih = wih.CrossOver(other.wih)[0];
        result1.who = who.CrossOver(other.who)[0];
        result1.bh1 = bh1.CrossOver(other.bh1)[0];
        result1.bo = bo.CrossOver(other.bo)[0];
        result2.wih = wih.CrossOver(other.wih)[1];
        result2.who = who.CrossOver(other.who)[1];
        result2.bh1 = bh1.CrossOver(other.bh1)[1];
        result2.bo = bo.CrossOver(other.bo)[1];
        return results;
    }

    public void Mutate(float mutationRate)
    {
        wih.Mutate(mutationRate);
        who.Mutate(mutationRate);
        bh1.Mutate(mutationRate);
        bo.Mutate(mutationRate);
    }

    public NeuralNetwork Copy()
    {
        NeuralNetwork result = new NeuralNetwork(numInputs, numHidden, numOutputs);
        result.wih = wih.Copy();
        result.who = who.Copy();
        result.bh1 = bh1.Copy();
        result.bo = bo.Copy();
        return result;
    }
}
