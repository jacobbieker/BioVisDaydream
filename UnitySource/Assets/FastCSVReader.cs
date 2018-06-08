using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class FastCSVReader : MonoBehaviour {
    protected string text = "";


    public static List<Dictionary<string, int>> Read(string file)
    {
        StreamReader reader = new StreamReader(file);

        var list = new List<Dictionary<string, int>>(); //declare dictionary list

        var header = reader.ReadLine().Split(',');
        string line;

        do
        {
            var entry = new Dictionary<string, int>();
            int n;
            line = reader.ReadLine();

            if (line != null)
            {
                var values = line.Split(',');
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    int.TryParse(values[j], out n);

                    entry[header[j]] = n;
                }

                list.Add(entry);
                    
            }

        }
        while (line != null);

        return list;






    }
}
