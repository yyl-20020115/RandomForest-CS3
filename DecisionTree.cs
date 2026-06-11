using System;
using System.Linq;
using System.Collections.Generic;

namespace MachineLearning; 

abstract class Tree 
{
    protected Nodes RootNode;
    protected DataSet dataSet;
    protected string[][][] colConditions;

    public string GetDecision(string[] inputData) 
    {
        return RootNode.GetDecision(inputData);
    }
}

class DecisionTree : Tree
{
    public DecisionTree(string dataSouce) 
    {
        dataSet = new DataSet(dataSouce);

        colConditions = new string[this.dataSet.GetNumCol() - 1][][];
        for (int col = 0; col < this.dataSet.GetNumCol() - 1; col++)
        {
            colConditions[col] = this.dataSet.GetColConditions(col);
        }

        RootNode = new Node(dataSet, new int[0], _conditions : colConditions);
    }
}

class RandomTree : Tree 
{
    public RandomTree(DataSet _dataSet, int numColsUsed = 0) 
    {
        dataSet = _dataSet;

        colConditions = new string[this.dataSet.GetNumCol() - 1][][];
        for (int col = 0; col < this.dataSet.GetNumCol() - 1; col++)
        {
            colConditions[col] = this.dataSet.GetColConditions(col);
        }

        Random rnd = new Random();

        List<int> colsToUseList = new List<int>();

        if (numColsUsed > 0 && numColsUsed < this.dataSet.GetNumCol()) 
        {
            colsToUseList = new List<int>();
        }

        while (colsToUseList.Count < numColsUsed) 
        {
            int nextCol = rnd.Next(0, this.dataSet.GetNumCol() - 1);

            if (!colsToUseList.Contains(nextCol)) 
            {
                colsToUseList.Add(nextCol);
            }
        }

        int[] colsToUse = (colsToUseList.Count > 0) ? colsToUseList.ToArray() : null;

        RootNode = new Node(_dataSet, new int[0], colConditions, colsToUse, numColsUsed);
    }
}

abstract class Nodes 
{
    abstract public string GetDecision(string[] input);
}

class Node : Nodes 
{
    private Nodes leftNode;
    private Nodes rightNode;
    private int checkCol;
    private string type;
    private string[] condition;
    private int[] colsUsed;

    public Node(DataSet nodeDataSet, int[] _colsUsed, string[][][] _conditions, int[] _colsToUse = null, int numColsToUse = 0) 
    {
        colsUsed = _colsUsed;

        float nodeImpurity = CalculateImpurity(nodeDataSet);

        (int col, string[] split, float impurity) = FindBestSplit(nodeDataSet,  _conditions, _colsToUse);

        checkCol = col;
        type = nodeDataSet.GetColType(col);
        condition = split;

        if (nodeDataSet.GetNumCol() - 1 > colsUsed.Length) 
        {
            if (impurity < nodeImpurity) 
            {
                (DataSet _leftNode, DataSet _rightNode) = SeperateData(checkCol, nodeDataSet, condition);

                //updates the array of cols used in previous nodes
                int[] toPassColsUsed = new int[colsUsed.Length + 1];

                for (int i = 0; i < colsUsed.Length; i++) 
                {
                    toPassColsUsed[i] = colsUsed[i];
                }

                toPassColsUsed[toPassColsUsed.Length - 1] = checkCol;

                //randomly selects columns to use when splitting the next node if randomly generating a tree
                List<int> colsToUseList = new List<int>();
                
                numColsToUse = (numColsToUse + toPassColsUsed.Length >= nodeDataSet.GetNumCol()) ? numColsToUse - 1 : numColsToUse;

                if (numColsToUse > 0) 
                {
                    Random rnd = new Random();

                    while (colsToUseList.Count < numColsToUse) 
                    {
                        int nextCol = rnd.Next(0, nodeDataSet.GetNumCol() - 1);

                        if (!toPassColsUsed.Contains(nextCol) && !colsToUseList.Contains(nextCol)) 
                        {
                            colsToUseList.Add(nextCol);
                        }
                    }
                }

                int[] colsToUse = (colsToUseList.Count > 0) ? colsToUseList.ToArray() : null;

                if (CalculateImpurity(_leftNode) != 0) 
                {
                    leftNode = new Node(_leftNode, toPassColsUsed, _conditions, colsToUse, numColsToUse);
                }
                else 
                {
                    //data split perfectly so only one decision can be made
                    string decision = _leftNode.MostCommonColValue(_leftNode.GetNumCol() - 1);
                    leftNode = new Leaf(decision);
                }

                if (CalculateImpurity(_rightNode) != 0) 
                {
                    rightNode = new Node(_rightNode, toPassColsUsed, _conditions, colsToUse, numColsToUse);
                }
                else 
                {
                    //data split perfectly so only one decision can be made
                    string decision = _rightNode.MostCommonColValue(_rightNode.GetNumCol() - 1);
                    rightNode = new Leaf(decision);
                }
            }
            else 
            {
                //splitting the data further doesn't result in more accurate decisions
                //adds two of the same leaf nodes effectively making the current node a leaf node
                string decision = nodeDataSet.MostCommonColValue(nodeDataSet.GetNumCol() - 1);

                leftNode = new Leaf(decision);
                rightNode = new Leaf(decision);

                if (type == "double") 
                {
                    condition = new string[] { "0" };
                }
                else 
                {
                    condition = new string[] { "" };
                }
            }  
        }
        else 
        {
            //no more columns to make decisions from
            //adds two of the same leaf nodes effectively making the current node a leaf node
            string decision = nodeDataSet.MostCommonColValue(nodeDataSet.GetNumCol() - 1);

            if (type == "double") 
            {
                condition = new string[] { "0" };
            }
            else 
            {
                condition = new string[] { "" };
            }

            leftNode = new Leaf(decision);
            rightNode = new Leaf(decision);
        }                      
    }

    override public string GetDecision(string[] input) 
    {
        if (input.Length != 0) 
        {
            string toCheck = input[checkCol];

            if (type == "double")
            {
                //data being looked at is numeric
                if (Convert.ToDouble(toCheck) <= Convert.ToDouble(condition[0])) 
                {
                    return leftNode.GetDecision(input);
                }
                else 
                {
                    return rightNode.GetDecision(input);
                }
            }
            else
            {
                if (condition.Contains(toCheck)) 
                {
                    return leftNode.GetDecision(input);
                }
                else 
                {
                    return rightNode.GetDecision(input);
                }
            }
        } 
        else 
        {
            //no data to make a decision from, next nodes must be leaf nodes
            return leftNode.GetDecision(new string[0]);
        }
        
    }

    private (DataSet, DataSet) SeperateData(int col, DataSet dataSet, string[] conditionToSeperate) 
    {
        //finds all entries that meet the specified conditions
        int[] selectedData;

        if (dataSet.GetColType(col) == "double") 
        {
            //converts to double if dealing with numeric data
            selectedData = dataSet.SelectEntries(col, Convert.ToDouble(conditionToSeperate[0]));
        }
        else 
        {
            selectedData = dataSet.SelectEntries(col, conditionToSeperate);
        }

        //finds all entries that don't meet the specified conditions
        List<int> toRemove = new List<int>();

        for (int toRemoveIndex = 0; toRemoveIndex < dataSet.GetNumEntries(); toRemoveIndex++) 
        {
            if (!selectedData.Contains(toRemoveIndex)) 
            {
                toRemove.Add(toRemoveIndex);
            }
        }

        //creates two data sets corresponding to meeting the specified conditions and not
        DataSet trueData = new DataSet(dataSet.CloneData(rowToRemove : toRemove.ToArray()));
        DataSet falseData = new DataSet(dataSet.CloneData(rowToRemove : selectedData));

        return (trueData, falseData);
    }

    private (int, string[], float) FindBestSplit(DataSet dataSet, string[][][] conditions, int[] colsToUse = null) 
    {
        //finds best way to split a data set to find lowest impurity

        float bestImpurity = 1; //highest possible impurity
        int bestCol = 0;
        string[] bestSplit = new string[0];

        //generates a list of columns that should be used when chosing how to split the dataset
        List<int> cols = new List<int>();

        if (colsToUse == null) 
        {
            for (int col = 0; col < dataSet.GetNumCol() - 1; col++) 
            {
                if (!this.colsUsed.Contains(col)) 
                {
                    cols.Add(col);
                }
            }
        }
        else 
        {
            foreach (int col in colsToUse) 
            {
                cols.Add(col);
            }
        }

        foreach (int col in cols) 
        {
            string[][] toCheck;

            toCheck = conditions[col];

            foreach (string[] comb in toCheck) 
            {
                //calculates weighted impurity of split data and records conditions that improve impurity
                (DataSet leftNode, DataSet rightNode) = SeperateData(col, dataSet, comb);

                float weightedImpurity = 
                    ((float)leftNode.GetNumEntries() / (float)dataSet.GetNumEntries()) * CalculateImpurity(leftNode)
                    + ((float)rightNode.GetNumEntries() / (float)dataSet.GetNumEntries()) * CalculateImpurity(rightNode)
                ;

                if (weightedImpurity < bestImpurity) 
                {
                    bestImpurity = weightedImpurity;
                    bestCol = col;
                    bestSplit = comb;
                }
            }
        }

        return (bestCol, bestSplit, bestImpurity);
    }

    private float CalculateImpurity(DataSet toCalc) 
    {
        //calculates the spread of results to find the data sets impurity

        Dictionary<string, int> resultSpread = new Dictionary<string, int>();

        int resultsCol = toCalc.GetNumCol() - 1; //the final column in the data set

        string[] resultValues = toCalc.GetColValues(resultsCol);

        foreach (string result in resultValues) 
        {
            int numOccurences = toCalc.SelectEntries(resultsCol, result).Length;
            resultSpread.Add(result, numOccurences);
        }

        return CalculateGiniImpurity(resultSpread.Values.ToArray());
    }

    private float CalculateGiniImpurity(int[] values) 
    {
        //takes the spread of all possible results and calculates the Gini impurity

        float giniImpurity = 1;
        float totalValue = values.Sum();

        foreach (float value in values) 
        {
            giniImpurity -= (float)Math.Pow(value / totalValue, 2);
        }

        return giniImpurity;
    }

}

class Leaf : Nodes
{
    private string decision;

    public Leaf(string _decision)
    {
        decision = _decision;
    }

    override public string GetDecision(string[] input) 
    {
        return decision;
    }
}