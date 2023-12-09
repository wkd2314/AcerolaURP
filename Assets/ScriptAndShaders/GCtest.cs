using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GCtest
{
    // string class_instance array -> ref type
    // => dont allocate in update (causes of frequent GC)
    
    void BadStringExample(int[] intArray)
    {
        // new allocation
        string line = intArray[0].ToString();

        for (int i = 1; i < intArray.Length; i++)
        {
            // new allocation
            // previous line memory is located somewhere else
            // and new memory will be allocated to line variable
            line += ", " + intArray[i].ToString();
        }

        return;
    }

    void BetterStringExample(int[] intArray)
    {
        // initialize with first element,
        // expects to hold 50 characters.
        var builder = new System.Text.StringBuilder(intArray[0].ToString(), 100);

        for (int i = 1; i < intArray.Length; i++)
        {
            builder.Append(intArray[i].ToString());
        }
        
        // example use
        builder.AppendFormat("GHI{0}{1}", 'J', 'K');

        Console.WriteLine("{0} chars: {1}", builder.Length, builder.ToString());

        builder.Insert(0, "IntArray: ");

        // replace all 1 with 4
        builder.Replace('1', '4');
    }
    
    // even concat won't be too much trouble unless called frequently for example

    public class BadStringClass : MonoBehaviour
    {
        public Text scoreBoard;
        public int score;
        private void Update()
        {
            // new allocation
            string scoreText = "Score: " + score.ToString();
            scoreBoard.text = scoreText;
        }
    }

    public class BetterStringClass : MonoBehaviour
    {
        public Text scoreBoard;
        public string scoreText;
        public int score;
        public int oldScore;

        private void Update()
        {
            if (score != oldScore)
            {
                // new allocation
                // same as previous example but called only when score changes
                scoreText = "Score: " + score.ToString();
                scoreBoard.text = scoreText;
                oldScore = score;
            }
        }
    }

    // looks more convenient as it results the list itself.
    // however calling this in update will cause new allocation
    // every frame.
    public float[] BadArrayExample(int numElements)
    {
        var result = new float[numElements];

        for (int i = 0; i < numElements; i++)
        {
            result[i] = Random.value;
        }

        return result;
    }

    // allocate array somewhere else and call this in update.
    public void BetterArrayExample(float[] arrayToFill)
    {
        for (int i = 0; i < arrayToFill.Length; i++)
        {
            arrayToFill[i] = Random.value;
        }
    }

    void GCOFF()
    {
        GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
        GarbageCollector.incrementalTimeSliceNanoseconds = 500;
    }

    void Start()
    {
        // instantiate a 'useless' object that is allocated purely for
        // its effect on the memory manager.
        var tmp = new System.Object[1024];
        
        // make allocations in smaller blocks to avoid them to be treated in a special way
        for (int i = 0; i < 1024; i++)
        {
            tmp[i] = new byte[1024];
        }
        
        // release ref  (now heap starts with size of (1MB = 1024 * 1024 * byte)
        tmp = null; 
    }

    void OnPause()
    {
        System.GC.Collect();
    }
}
