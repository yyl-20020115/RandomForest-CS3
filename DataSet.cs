using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace MachineLearning; 

class DataSet 
{
    private readonly string[] headers;
    private readonly bool[] isColNumeric;
    List<string[]> dataSet = [];
    private readonly string[][][] colConditions;

    public DataSet(string dataSouce) 
    {
        //takes data from a .csv file and stores it as a list of string arrays
        //takes the topline as the files headers

        if (File.Exists(dataSouce)) 
        {
            foreach (var line in File.ReadAllLines(dataSouce)) 
            {
                var dataInput = line.Split(',');
                var result = dataInput[^1];

                if (headers == null) 
                {
                    headers = dataInput;
                    isColNumeric = new bool[dataInput.Length];
                }
                else 
                {
                    dataSet.Add(dataInput);
                }                    
            }

            for (int col = 0; col < this.headers.Length; col++) 
            {
                isColNumeric[col] = IsColNumeric(col);
            }
        }
        else 
        {
            Console.WriteLine("File does not exist");
        }

        colConditions = new string[this.headers.Length - 1][][];
        for (int col = 0; col < this.headers.Length - 1; col++)
        {
            colConditions[col] = DetermineConditions(col);
        }
    }

    public DataSet((string[] _headers, bool[] _isColNumeric, string[][][] _colConditions, List<string[]> data) dataSource) 
    {
        //takes a tuple of data and headers and appropriatly assigns them
        //this is the output from the CloneData() function
        headers = dataSource._headers;
        isColNumeric = dataSource._isColNumeric;
        dataSet = dataSource.data;
        colConditions = dataSource._colConditions;
    }

    public int GetNumCol() => this.headers.Length;

    public int GetNumEntries() => dataSet.Count;

    public string[] GetEntry(int row) => this.dataSet[row];

    public string[] GetHeaders() => this.headers;

    public string[] GetColValues(int col) 
    {
        //returns all unique values in a column            
        List<string> values = [];
        values.AddRange(from string[] entry in this.dataSet
                        let value = entry[col]
                        where !values.Contains(value)
                        select value);
        return [.. values];
    }

    public string GetColType(int col) 
    {
        //returns whether the column has numeric/double data or strings
        //ranked data treated as numeric data for simplicity
        string colType = isColNumeric[col] ? "double" : "string";
        return colType;
    }

    public string[][] GetColConditions(int col) => colConditions[col];

    public (DataSet, DataSet) CreateBootstrapedDataSet() 
    {
        //randomly takes entries from the dataset to create a new dataset
        //entries can be repeated
        Random rnd = new();

        List<string[]> bootStrapedData = [];
        List<int> entryIndexUsed = [];

        int dataSetSize = this.dataSet.Count;

        for (int entryNum = 0; entryNum < dataSetSize; entryNum++) 
        {
            int nextIndex = rnd.Next(0, dataSetSize);

            string[] entryToAdd = this.dataSet[nextIndex];

            if (!entryIndexUsed.Contains(nextIndex)) 
            {
                entryIndexUsed.Add(nextIndex);
            }

            string[] clonedEntry = new string[entryToAdd.Length];

            for (int i = 0; i < entryToAdd.Length; i++) 
            {
                clonedEntry[i] = entryToAdd[i];
            }

            bootStrapedData.Add(clonedEntry);
        }

        DataSet bootStrapedDataSet = new DataSet((this.headers, this.isColNumeric, this.colConditions, bootStrapedData));
        DataSet outOfBagDataSet = new DataSet(CloneData(rowToRemove : entryIndexUsed.ToArray()));

        return (bootStrapedDataSet, outOfBagDataSet);
    }

    public int[] CompareDataSets(DataSet toCompare) 
    {
        //finds all the matching entries in a data set and returns their positions
        List<int> matchingEntries = new List<int>();

        for(int thisEntryIndex = 0; thisEntryIndex < this.dataSet.Count; thisEntryIndex++) 
        {
            string[] entry = this.dataSet[thisEntryIndex];

            for (int toCompareIndex = 0; toCompareIndex < toCompare.GetNumEntries(); toCompareIndex++) 
            {
                if (CompareEntries(entry, toCompare.GetEntry(toCompareIndex))) 
                {
                    matchingEntries.Add(thisEntryIndex);
                    break;
                }
            }
        }

        return matchingEntries.ToArray();
    }

    private bool CompareEntries(string[] entry1, string[] entry2) 
    {
        bool areSame = true;

        for (int index = 0; index < entry1.Length; index++) 
        {
            if (entry1[index] != entry2[index]) 
            {
                areSame = false;
                break;
            }
        }

        return areSame;
    }

    public (string[], bool[], string[][][], List<string[]>) CloneData(int[] colToRemove = null, int[] rowToRemove = null) 
    {
        //clones the data whilst removing any specifed entries and/or columns

        if (colToRemove == null) 
        {
            colToRemove = new int[0];
        }

        if (rowToRemove == null) 
        {
            rowToRemove = new int[0];
        }

        //clones the data sets headers removing any specified headers
        List<string> clonedHeaders = new List<string>();
        List<bool> clonedColType = new List<bool>();

        for (int col = 0; col < this.headers.Length; col++) 
        {
            if (!colToRemove.Contains(col)) 
            {
                clonedHeaders.Add(this.headers[col]);
                clonedColType.Add(this.isColNumeric[col]);
            }
        }

        //clones the data removing any specified entries and/or columns
        List<string[]> clonedData = new List<string[]>();

        for (int row = 0; row < this.dataSet.Count; row++) 
        {
            if (!rowToRemove.Contains(row)) 
            {
                string[] entry = this.dataSet[row];
                string[] clonedEntry = new string[entry.Length - colToRemove.Length];

                int posToAdd = 0;

                for (int col = 0; col < entry.Length; col++) 
                {
                    if (!colToRemove.Contains(col)) 
                    {
                        clonedEntry[posToAdd++] = entry[col];
                    }
                }

                clonedData.Add(clonedEntry);
            }                
        }

        //clones the conditions removing the conditions for any specified columns
        List<string[][]> clonedCondtions = new List<string[][]>();

        for (int col = 0; col < this.colConditions.Length; col++) 
        {
            if (!colToRemove.Contains(col)) 
            {
                string[][] allCond = new string[this.colConditions[col].Length][];

                for (int j = 0; j < allCond.Length; j++) 
                {
                    string[] cond = new string[this.colConditions[col][j].Length];

                    for (int k = 0; k < cond.Length; k++)
                    {
                        cond[k] = this.colConditions[col][j][k];
                    }

                    allCond[j] = cond;
                }

                clonedCondtions.Add(allCond);
            }
        }

        return (clonedHeaders.ToArray(), clonedColType.ToArray(), clonedCondtions.ToArray(), clonedData);
    }

    public int[] SelectEntries(int checkCol, string entryCheck) 
    {
        //selects any entries who's column is a specified value

        List<int> selectedEntries = new List<int>();

        for (int row = 0; row < this.dataSet.Count; row++) 
        {
            string[] entry = this.dataSet[row];

            if (entry[checkCol] == entryCheck) 
            {
                selectedEntries.Add(row);
            }
        }

        return selectedEntries.ToArray();
    }

    public int[] SelectEntries(int checkCol, string[] entryCheck) 
    {
        //selects any entries who's column is a specified value

        List<int> selectedEntries = new List<int>();

        for (int row = 0; row < this.dataSet.Count; row++) 
        {
            string[] entry = this.dataSet[row];

            if (entryCheck.Contains(entry[checkCol])) 
            {
                selectedEntries.Add(row);
            }
        }

        return selectedEntries.ToArray();
    }

    public int[] SelectEntries(int checkCol, double entryCheck) 
    {
        //selects any entries who's column is less than or equal to a specified value

        List<int> selectedEntries = new List<int>();

        for (int row = 0; row < this.dataSet.Count; row++) 
        {
            string[] entry = this.dataSet[row];

            if (Convert.ToDouble(entry[checkCol]) <= entryCheck) 
            {
                selectedEntries.Add(row);
            }
        }

        return selectedEntries.ToArray();
    }

    public void PrintDataSet() 
    {
        //prints the data and headers onto the console

        foreach (string header in this.headers) 
        {
            Console.Write("\t\t" + header);
        }

        Console.Write("\n");

        foreach (string[] entry in this.dataSet) 
        {
            string toOutput = "";

            foreach (string value in entry) 
            {
                toOutput += "\t\t" + value;
            }

            Console.WriteLine(toOutput);
        }
    }

    public string MostCommonColValue(int col) 
    {
        //finds the most common value within a column
        string mostComVal = "";
        
        int highestValNum = 0;

        foreach (string val in GetColValues(col)) 
        {
            int valNum = SelectEntries(col, val).Length;

            if (valNum > highestValNum) 
            {
                mostComVal = val;
                highestValNum = valNum;
            }
        }

        return mostComVal;
    }

    private bool IsColNumeric(int col) 
    {
        //determines whether a column is numeric or not
        bool isNumeric = true;

        foreach (string value in GetColValues(col)) 
        {
            if (!Regex.IsMatch(value, @"\d+")) 
            {
                isNumeric = false;
                break;
            }
        }

        return isNumeric;
    }

    private string[][] DetermineConditions(int col) 
    {
        string[][] colConditions;

        //determines all the unique conditions to use when evaluating each column
        string[] uniqueVals = this.GetColValues(col);

        if (this.GetColType(col) == "double") 
        {
            //handles numeric data
            double[] allNums = new double[uniqueVals.Length];

            for (int i = 0; i < uniqueVals.Length; i++) 
            {
                allNums[i] = Convert.ToDouble(uniqueVals[i]);
            }

            double[] allCond = FindAllNumericConditions(allNums);

            //all data handled as strings so numeric conditions converted back
            colConditions = new string[allCond.Length][];

            for (int i = 0; i < colConditions.Length; i++) 
            {
                colConditions[i] = new string[] { Convert.ToString(allCond[i]) };
            }
        } 
        else 
        {
            colConditions = FindAllCombinations(uniqueVals);
        }

        return colConditions;
    }

    private double[] FindAllNumericConditions(double[] allValues) 
    {
        //sorts each unique value and finds the midpoints between them
        List<double> allCond = new List<double>();

        Array.Sort(allValues);

        for (int i = 0; i < allValues.Length - 1; i++) 
        {
            allCond.Add((allValues[i] + allValues[i + 1]) / 2);
        }

        return allCond.ToArray();
    }

    private string[][] FindAllCombinations(string[] allPosValues) 
    {
        //finds all combinations of unique values 
        List<string[]> posConds = new List<string[]>();

        //finds combinations using 1 value then 2, 3 and so on till it reaches maxNumConds
        int maxNumConds = allPosValues.Length - 1;
        int currentMaxConds = 1;
        
        //position to take values from allPosValues
        int startPos = 0;
        int posIndex = startPos;

        //records how many possible combinations of a certain length can be found
        int numFound = 0;
        int totalComb = CalculateTotalCombinations(currentMaxConds, allPosValues.Length);

        List<string> posCond = new List<string>();

        while (currentMaxConds <= maxNumConds) 
        {
            //iteratest through all values to find different combinations
            string nextValue = allPosValues[posIndex++];

            if (posCond.Count < currentMaxConds && !posCond.Contains(nextValue)) 
            {
                posCond.Add(nextValue);
            }
            else 
            {
                //removes values till a start position is found in allPosValues that can fill posCond
                string valToRemove;
                int posInAllValues;

                do 
                {
                    valToRemove = posCond[posCond.Count - 1];

                    posInAllValues = 0;

                    for (int i = 0; i < allPosValues.Length; i++) 
                    {
                        if (valToRemove == allPosValues[i]) 
                        {
                            posInAllValues = i;
                            break;
                        }
                    }

                    startPos = posInAllValues + 1;
                    posCond.Remove(valToRemove);

                } while (allPosValues.Length - startPos < currentMaxConds - posCond.Count);

                posIndex = startPos;
            }

            if (posCond.Count == currentMaxConds)
            {
                //found a combination of conditions with the required length
                posConds.Add(posCond.ToArray());
                posCond.RemoveAt(posCond.Count - 1);    //removes final condition to prepare for next combination
                numFound++;
            }

            if (posIndex >= allPosValues.Length) 
            {
                //reached end of allPosValues array so loops back to specified start position
                posIndex = startPos;
            }

            if (numFound == totalComb) 
            {
                //all possible conditions of the required lenght have been found so resets to find the next set
                numFound = 0;
                totalComb = CalculateTotalCombinations(++currentMaxConds, allPosValues.Length);
                startPos = 0;
                posIndex = startPos;
                posCond.Clear();
            }
        }

        return posConds.ToArray();
    }

    private int CalculateTotalCombinations(int numChosen, int totalOptions) 
    {
        //total combinations without repition = n!/(r!(n-r)!)
        int numComb = CalcFactorial(totalOptions) / (CalcFactorial(numChosen) * CalcFactorial(totalOptions - numChosen));

        return numComb;
    }

    private int CalcFactorial(int n) 
    {
        //calculates the factorial eg: 5! = 5 * 4 * 3 * 2 * 1
        int result = 1;

        for (int i = 1; i <= n; i++) 
        {
            result *= i;
        }

        return result;
    }
}