using System;
using System.Collections.Generic;

namespace MachineLearning;

class Program
{
    static void Main(string[] args)
    {
        // BinaryTest();
        // MultChoiceTest();
        // RankedTest();
        // NumTest();

        // RandomTest();
        // RandomForestTest();

        LargeRandomTest();

        // DataSetTest();

        Console.ReadKey();
    }

    static void BinaryTest() 
    {
        Console.WriteLine("Binary Test\n");

        DecisionTree dT = new("TestData\\IsSunnyTest.csv");

        List<string[]> testEntries =
        [
            ["TRUE", "FALSE"], //TRUE
            ["FALSE", "FALSE"], //FALSE
            ["TRUE", "TRUE"], //TRUE
        ];

        foreach (string[] entry in testEntries) 
        {
            Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), dT.GetDecision(entry));
        }

        Console.WriteLine();
    }

    static void MultChoiceTest() 
    {
        Console.WriteLine("Multiple Choice Test\n");

        DecisionTree dT = new("TestData\\DessertTest.csv");

        List<string[]> testEntries =
        [
            ["APPLE", "YES"], //YES
            ["PEAR", "YES"], //YES
            ["PEAR", "NO"], //NO
            ["PINAPPLE", "NO"], //YES
            ["AUBERGINE", "YES"], //NO
            ["BANNANA", "YES"], //YES
            ["LEMON", "NO"], //NO
            ["LEMON", "YES"], //NO
        ];
        

        foreach (string[] entry in testEntries) 
        {
            Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), dT.GetDecision(entry));
        }

        Console.WriteLine();
    }

    static void RankedTest() 
    {
        Console.WriteLine("Numeric Ranked Data Test\n");

        DecisionTree dT = new("TestData\\CorDTest.csv");

        List<string[]> testEntries =
        [
            ["2", "4"], //CURRY
            ["1", "5"], //CURRY
            ["3", "3"], //DESSERT
            ["3", "4"], //CURRY
            ["5", "1"], //DESSERT
            ["4", "2"], //DESSERT
            ["4", "3"], //DESSERT
            ["3", "5"], //CURRY
            ["1", "2"], //DESSERT
            ["2", "4"], //CURRY
            ["1", "3"], //CURRY
        ];
        

        foreach (var entry in testEntries) 
        {
            Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), dT.GetDecision(entry));
        }

        Console.WriteLine();
    }

    static void NumTest() 
    {
        Console.WriteLine("Numeric Data Test\n");

        DecisionTree dT = new("TestData\\LikesFruitTest.csv");

        List<string[]> testEntries =
        [
            ["YES", "100"], //NO
            ["YES", "125"], //YES
            ["NO", "600"], //YES
            ["NO", "300"], //YES
            ["YES", "40"], //NO
            ["NO", "150"], //NO
            ["NO", "325"], //YES
            ["YES", "70"], //NO
        ];


        foreach (var entry in testEntries)
        {
            Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), dT.GetDecision(entry));
        }

        Console.WriteLine();
    }

    static void RandomTest() 
    {
        Console.WriteLine("Random Test\n");

        DataSet dS = new DataSet("TestData\\LikesFruitTest.csv");

        List<string[]> testEntries = new List<string[]>();

        testEntries.Add(new string[] {"YES", "100"});
        testEntries.Add(new string[] {"YES", "125"});
        testEntries.Add(new string[] {"NO", "600"});
        testEntries.Add(new string[] {"NO", "300"});
        testEntries.Add(new string[] {"YES", "40"});
        testEntries.Add(new string[] {"NO", "150"});
        testEntries.Add(new string[] {"NO", "325"});
        testEntries.Add(new string[] {"YES", "70"});
        
        for (int i = 0; i < 10; i++) 
        {
            (DataSet bootStrappedDS, DataSet outOfBagDS) = dS.CreateBootstrapedDataSet();

            RandomTree rT = new RandomTree(bootStrappedDS, 0);

            foreach (string[] entry in testEntries) 
            {
                Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), rT.GetDecision(entry));
            }

            Console.WriteLine();
        }
        
    }

    static void RandomForestTest() 
    {
        Console.WriteLine("Random Forest Test\n");

        RandomForest rF = new RandomForest("TestData\\LikesFruitTest.csv");

        List<string[]> testEntries = new List<string[]>();

        testEntries.Add(new string[] {"YES", "100"});
        testEntries.Add(new string[] {"YES", "125"});
        testEntries.Add(new string[] {"NO", "600"});
        testEntries.Add(new string[] {"NO", "300"});
        testEntries.Add(new string[] {"YES", "40"});
        testEntries.Add(new string[] {"NO", "150"});
        testEntries.Add(new string[] {"NO", "325"});
        testEntries.Add(new string[] {"YES", "70"});

        foreach (string[] entry in testEntries) 
        {
            Console.WriteLine(@"For entry {0} : decision is {1}", string.Join(", ", entry), rF.GetDecision(entry));
        }

        Console.WriteLine();
        
    }

    static void LargeRandomTest() 
    {
        Console.WriteLine("Testing Random Forest With Large DataSet\n");

        RandomForest rF = new RandomForest("TestData\\LargeRandomTest.csv");

        Random rnd = new Random();

        List<string[]> testData = new List<string[]>();
        List<string> results = new List<string>();

        Console.WriteLine("Test logic : RandNum3 > RandNum2 AND RandNum3 * RandNum2 / RandNum1 < RandNum4\n");

        for (int i = 0; i < 100; i++) 
        {
            double[] testEntry = new double[] { rnd.Next(0, 1000) / 10.0, rnd.Next(0, 1000) / 100.0, rnd.Next(0, 1000) / 100.0, rnd.Next(0, 1000) / 1000.0 };

            if (testEntry[2] > testEntry[1] && testEntry[2] * testEntry[1] / testEntry[0] < testEntry[3]) 
            {
                results.Add("TRUE");
            }
            else 
            {
                results.Add("FALSE");
            }

            string[] entrytoAdd = new string[testEntry.Length];

            for (int j = 0; j < testEntry.Length; j++) 
            {
                entrytoAdd[j] = testEntry[j].ToString();
            }

            testData.Add(entrytoAdd);
        }

        int numCorrect = 0;

        for (int i = 0; i < testData.Count; i++) 
        {
            string[] test = testData[i];

            string decision = rF.GetDecision(test);

            string toWrite = string.Format("Test: {0}, result is {1}", string.Join(", ", test), decision);

            if (decision == results[i]) 
            {
                Console.WriteLine(toWrite + ": PASSED");
                numCorrect++;
            }
            else 
            {
                Console.WriteLine(toWrite + ": FAILED");
            }
        }

        Console.WriteLine();
        Console.WriteLine(@"Error in test : {0}%", (1.0 - (double)numCorrect / (double)testData.Count) * 100);
        Console.WriteLine();
    }

    static void DataSetTest() 
    {
        Console.WriteLine("Testing DataSet Functionality\n");

        DataSet dS = new DataSet("TestData\\LikesFruitTest.csv");
        dS.PrintDataSet();

        Console.WriteLine();

        for (int i = 0; i < dS.GetNumCol(); i++) 
        {
            Console.Write("\t\t" + dS.GetColType(i));
        }

        Console.WriteLine("\n");
        Console.WriteLine("Testing Selecting Entries\n");

        int[] selectedEntries = dS.SelectEntries(0, "TRUE");
        Console.WriteLine(selectedEntries.Length);

        Console.WriteLine();
        Console.WriteLine("Testing Cloning DataSet\n");

        int[] colToRemove = new int[] {};
        int[] rowToRemove = new int[] {};

        DataSet dS2 = new DataSet(dS.CloneData(colToRemove, selectedEntries));
        dS2.PrintDataSet();

        Console.WriteLine("\n");
        Console.WriteLine("Testing Boot Strapping data set\n");

        (DataSet bootStrappedDS, DataSet outOfBagDS) = dS.CreateBootstrapedDataSet();
        Console.WriteLine("Bootstrapped data :");
        bootStrappedDS.PrintDataSet();

        Console.WriteLine("\nOut of Bag Data :");
        outOfBagDS.PrintDataSet();

        Console.WriteLine("\n");

    }
}
