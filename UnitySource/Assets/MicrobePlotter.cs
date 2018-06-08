using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GracesGames.SimpleFileBrowser.Scripts;

public class MicrobePlotter : MonoBehaviour
{
    public string inputpath;

    // List for holding data from CSV reader
    private List<Dictionary<string, int>> pointList;
    private List<Dictionary<string, int>> clusterList;

    // Indices for columns to be assigned
    public int columnClusterNumber = 0;
    public int columnX = 1;
    public int columnY = 2;
    public int columnZ = 3;

    // Full column names
    public string xName;
    public string yName;
    public string zName;
    public string clusterNumberName;

    public float plotScale = 0.1f;

    // GameObject
    public GameObject microbe;

    // List used to create meshes, if needed
    public List<Vector3> meshPoints;
    public List<int> triangles;

    // Get Material for Microbe
    public Material microbeSurface;// = Resources.Load("MicrobeSurface.mat", typeof(Material)) as Material;

    // Use the file browser prefab
    public GameObject FileBrowserPrefab;

    // Define a file extension
    public string FileExtension = "csv";

    // Input field to get text to save
    private GameObject _textToSaveInputField;

    // Label to display loaded text
    private GameObject _loadedText;

    // Variable to save intermediate input result
    private string _textToSave;

    public bool PortraitMode;

    // Use this for initialization
    void Start()
    {
            //BuildModel(inputpath);

    }

    private void Update()
    {
            

    }


    private float FindMaxValue(string columnName)
    {
        //set initial value to first value
        float maxValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < pointList.Count; i++)
        {
            if (maxValue < Convert.ToSingle(pointList[i][columnName]))
                maxValue = Convert.ToSingle(pointList[i][columnName]);
        }

        //Spit out the max value
        return maxValue;
    }

    private float FindMinValue(string columnName)
    {

        float minValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            if (Convert.ToSingle(pointList[i][columnName]) < minValue)
                minValue = Convert.ToSingle(pointList[i][columnName]);
        }

        return minValue;
    }

    // Build the meshes from list of points (For Neutraphyll, etc)
    private Mesh CreateMesh(Vector3[] vertices, int[] triangles, GameObject instance)
    {
        Mesh mesh = new Mesh();
        instance.AddComponent<MeshRenderer>();
        MeshRenderer mr = instance.GetComponent<MeshRenderer>();
        mr.material = microbeSurface;// new Material(Shader.Find("MicrobeSurface"));
        instance.AddComponent<MeshFilter>();
        instance.GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Color[] colors = new Color[vertices.Length];
        Color triColor = getRandomColor();
        mr.material.SetColor("_Color", triColor);

        for (int i = 0; i < triangles.Length; i++)
        {
            int vertIndex = triangles[i];
            if (i % 3 == 0)
                triColor = getRandomColor();
            colors[vertIndex] = triColor;
        }
        mesh.colors = colors;

        return mesh;
    }

    private Color getRandomColor()
    {
        float red = UnityEngine.Random.Range(0, 1.0f);
        float green = UnityEngine.Random.Range(0, 1.0f);
        float blue = UnityEngine.Random.Range(0, 1.0f);
        return new Color(blue, green, red);
    }

    private List<int> CreateTriangles(List<int> triangles, List<Vector3> points)
    {
        int sizeofList = points.Count;
        Debug.Log(points.Count % 3);
        for (int i = 0; i < (sizeofList / 2); i++)
        {
            // goes as multiple of 3, as needed for triangles, wrapping around
            triangles.Add(i);
            triangles.Add(sizeofList - i - 1);
        }
        // Now the wrapping if not enough
        while(triangles.Count %3 != 0)
        {
            triangles.Add(0);
        }
        //triangles.Add(2);
        return triangles;
    }

    private List<int> CreateTrianglesLine(List<int> triangles, List<Vector3> points)
    {
        int sizeofList = points.Count;
        Debug.Log(points.Count % 3);
        for (int i = 0; i < (sizeofList); i++)
        {
            // goes as multiple of 3, as needed for triangles, wrapping around
            triangles.Add(i);
        }
        // Now the wrapping if not enough
        while (triangles.Count % 3 != 0)
        {
            triangles.Add(0);
        }
        //triangles.Add(2);
        return triangles;
    }

    public void testBuildModel(string inputPath)
    {
        Debug.Log(inputPath);

        Debug.Log("Start FastCSVReader");
        //pointList = FastCSVReader.Read(inputPath);
        Debug.Log("End Fast CSV Reader");

        //Debug.Log("STart CSVReader");
        //CSVReader.Read(inputPath);
        //Debug.Log("End CSVReader");

    }

    public void BuildModel(string inputPath)
    {
        Debug.Log(inputPath);
        pointList = FastCSVReader.Read(inputPath);
        //Log to console
        Debug.Log(pointList);

        // Declare list of strings, fill with keys (column names)
        List<string> columnList = new List<string>(pointList[1].Keys);

        // Print number of keys (using .count)
        Debug.Log("There are " + columnList.Count + " columns in the CSV");

        foreach (string key in columnList)
            Debug.Log("Column name is " + key);

        // Assign column name from columnList to Name variables
        clusterNumberName = columnList[columnClusterNumber];
        xName = columnList[columnX];
        yName = columnList[columnY];
        zName = columnList[columnZ];

        //Get max value of clusters, with assumption that integer increments and so 1-max is the number of things
        int numClusters = (int) FindMaxValue(clusterNumberName);


        // Get maxes of each axis
        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);

        // Get minimums of each axis
        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);

        //Loop through cluster numbers, to keep meshes aligned
        // For every one layer, so same z, remove all ones besides the top and bottom points before using the pointlist
        // Excluding those with the first and last z values for a given cluster
        for (var j = 0; j < numClusters; j++)
        {
            // Create a child GameObject to attach a mesh to, easier to split up and more likely to be composable
            GameObject instance = Instantiate(microbe);
            instance.transform.position = new Vector3(0, 0, 0);
            //Loop through Pointlist
            for (var i = 0; i < pointList.Count; i++)
            {
                // Check if meets the criteria of cluster name
                if (System.Convert.ToSingle(pointList[i][clusterNumberName]) == j)
                {
                    float x = pointList[i][xName];
                    float y = pointList[i][yName] - yMin + 1;
                    float z = pointList[i][zName];

                    meshPoints.Add(new Vector3(x*6*plotScale, y*plotScale, z*plotScale));
                }

                // Get value in poinList at ith "row", in "column" Name, normalize

                // Instantiate as gameobject variable so that it can be manipulated within loop
                //GameObject dataPoint = Instantiate(
                //        PointPrefab,
                //        new Vector3(x, y, z * 6) * plotScale,
                //        Quaternion.identity);
                //dataPoint.transform.localScale = new Vector3(50 * plotScale, 50 * plotScale, 50 * plotScale);

                // Make child of PointHolder object, to keep points within container in hiearchy
                //dataPoint.transform.parent = PointHolder.transform;

                // Assigns original values to dataPointName
               // string dataPointName =
               //     "Cluster: " + pointList[i][clusterNumberName] + " " +
               //     pointList[i][xName] + " "
               //     + pointList[i][yName] + " "
               //     + pointList[i][zName];

                // Assigns name to the prefab
                //dataPoint.transform.name = dataPointName;

                // Gets material color and sets it to a new RGB color we define
                //dataPoint.GetComponent<Renderer>().material.color =
               //      new Color(j*50 + 0f, 255.0f, z % 255.0f, 1.0f);
                //new Color(x % 255.0f, y % 255.0f, z % 255.0f, 1.0f);
                // Add point to meshPoints
                //TODO: Split meshes up into different clusters
                //meshPoints.Add(new Vector3(x, y, z * 6));
                //Destroy(dataPoint);
            }
            // MeshFilter
            //MeshFilter mf = instance.GetComponent<MeshFilter>();
            // Need to split based on cluster number, so different meshes for each
            triangles = CreateTriangles(triangles, meshPoints);
            //Now use the list of Vector3's to make the mesh
            CreateMesh(meshPoints.ToArray(), triangles.ToArray(), instance);
            instance.AddComponent<BoxCollider>();
            // Reset meshPoints = 0 as next cluster is started
            triangles = new List<int>();
            meshPoints = new List<Vector3>();
        }
        
    }

    public void LoadObjFile(string inputPath)
    {
        Debug.Log(inputPath);
        Mesh myMesh = FastOBJImporter.Instance.ImportFile(inputPath);

        // Currently scale is too large for the walking space, so scale it by 0.1
        myMesh.RecalculateNormals();
        GameObject instance = Instantiate(microbe);
        instance.transform.position = new Vector3(0, 0, 0);
        instance.AddComponent<MeshRenderer>();
        MeshRenderer mr = instance.GetComponent<MeshRenderer>();
        mr.material = microbeSurface;
        instance.AddComponent<MeshFilter>();
        instance.GetComponent<MeshFilter>().mesh = myMesh;



    }

    public void BuildGut(string inputPath)
    {
        Debug.Log(inputPath);
        pointList = FastCSVReader.Read(inputPath);
        //Log to console
        Debug.Log(pointList);

        // Declare list of strings, fill with keys (column names)
        List<string> columnList = new List<string>(pointList[1].Keys);

        // Print number of keys (using .count)
        Debug.Log("There are " + columnList.Count + " columns in the CSV");

        foreach (string key in columnList)
            Debug.Log("Column name is " + key);

        // Assign column name from columnList to Name variables
        clusterNumberName = columnList[columnClusterNumber];
        xName = columnList[columnX];
        yName = columnList[columnY];
        zName = columnList[columnZ];

        //Get max value of clusters, with assumption that integer increments and so 1-max is the number of things
        int numClusters = (int)FindMaxValue(clusterNumberName);


        // Get maxes of each axis
        float xMax = FindMaxValue(xName);
        float yMax = FindMaxValue(yName);
        float zMax = FindMaxValue(zName);

        // Get minimums of each axis
        float xMin = FindMinValue(xName);
        float yMin = FindMinValue(yName);
        float zMin = FindMinValue(zName);

        //Loop through cluster numbers, to keep meshes aligned
        // For every one layer, so same z, remove all ones besides the top and bottom points before using the pointlist
        // Excluding those with the first and last z values for a given cluster
        for (var j = 0; j < numClusters; j++)
        {
            // Create a child GameObject to attach a mesh to, easier to split up and more likely to be composable
            GameObject instance = Instantiate(microbe);
            instance.transform.position = new Vector3(0, 0, 0);
            //Loop through Pointlist
            for (var i = 0; i < pointList.Count; i++)
            {
                // Check if meets the criteria of cluster name
                if (System.Convert.ToSingle(pointList[i][clusterNumberName]) == j)
                {
                    float x = pointList[i][xName];
                    float y = pointList[i][yName] - yMin + 1;
                    float z = pointList[i][zName];

                    meshPoints.Add(new Vector3(x * 6 * plotScale, y * plotScale, z * plotScale));
                }

            }
            // Need to split based on cluster number, so different meshes for each
            triangles = CreateTrianglesLine(triangles, meshPoints);
            //Now use the list of Vector3's to make the mesh
            CreateMesh(meshPoints.ToArray(), triangles.ToArray(), instance);
            instance.AddComponent<BoxCollider>();
            // Reset meshPoints = 0 as next cluster is started
            triangles = new List<int>();
            meshPoints = new List<Vector3>();
        }

    }

    // Updates the text to save with the new input (current text in input field)
    public void UpdateTextToSave(string text)
    {
        _textToSave = text;
    }

    // Open the file browser using boolean parameter so it can be called in GUI
    public void OpenFileBrowser(bool saving)
    {
        OpenFileBrowser(saving ? FileBrowserMode.Save : FileBrowserMode.Load);
    }

    // Open a file browser to save and load files
    private void OpenFileBrowser(FileBrowserMode fileBrowserMode)
    {
        // Create the file browser and name it
        GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
        fileBrowserObject.name = "FileBrowser";
        // Set the mode to save or load
        GracesGames.SimpleFileBrowser.Scripts.FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<GracesGames.SimpleFileBrowser.Scripts.FileBrowser>();
        fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape);
        if (fileBrowserMode == FileBrowserMode.Save)
        {
            fileBrowserScript.SaveFilePanel(this, "SaveFileUsingPath", "Microbe Plotter", FileExtension);
        }
        else
        {
            fileBrowserScript.OpenFilePanel(this, "LoadFileUsingPath", FileExtension);
        }
    }

    // Saves a file with the textToSave using a path
    private void SaveFileUsingPath(string path)
    {
        // Make sure path and _textToSave is not null or empty
        if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(_textToSave))
        {
            //BinaryFormatter bFormatter = new BinaryFormatter();
            // Create a file using the path
            //FileStream file = File.Create(path);
            // Serialize the data (textToSave)
            //bFormatter.Serialize(file, _textToSave);
            // Close the created file
            //file.Close();
        }
        else
        {
            Debug.Log("Invalid path or empty file given");
        }
    }

    // Loads a file using a path
    private void LoadFileUsingPath(string path)
    {
        Debug.Log("Path Recieved is: " + path);
        if (path.Length != 0)
        {
            //List<Dictionary<string, object>> pointlist;
            //pointlist = CSVReader.Read(path);

            if (path.Contains("gut"))
            {
                LoadObjFile(path);
            }
            else
            {
                LoadObjFile(path);
            }
            // Convert the file from a byte array into a string
            // We're done working with the file so we can close it
            // Set the LoadedText with the value of the file

            //_loadedText.GetComponent<Text>().text = "Loaded data: from: \n" + path;
        }
        else
        {
            Debug.Log("Invalid path given");
        }
    }

}