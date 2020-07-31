using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class LoadGCODE : MonoBehaviour
{
    private float nextActionTime = 0.0f;
    public float period = 5.0f;
    string gcodeDir = @"gcode";
    float nozzleDiameter = 0;
    float bedX = 0;
    float bedY = 0;
    float bedZ = 0;
    // float actualZheight = 0;

    DirectoryInfo directoryInfo = null;
    // FileInfo[] fileInfo = null;
    string[] actualSelectedGcode = null;
    int lastSelection = 0;
    string[][] parsedGcode;
    int parseCounter = 0;
    int actualGCODElines = 0;
    GameObject segment;
    String adebugtext = "";
    String lastF;
    String lastX;
    String lastY;
    String lastZ;
    String lastE;

    float actualZheight = 0f;
    float actualLayerHeight = 0f;
    float actualExtrusionWidth = 0f;


    void Start()
    {
        // string assetPath = Application.streamingAssetsPath;

        // bool isWebGl = assetPath.Contains("://") ||
        //                  assetPath.Contains(":///");
        segment = GameObject.Find("Cylinder");


    }
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            Debug.Log("Parse test: " + float.Parse("X62.229".Replace('.', ',').Substring(1)));

            // Debug.Log("read gcode dir :   Last selection: " + lastSelection + "  value selected : " + GameObject.Find("loadGcode").GetComponent<Dropdown>().value);
            ReadGcodeDir();
            if (lastSelection != GameObject.Find("loadGcode").GetComponent<Dropdown>().value)
            {
                LoadGcodeFile();
            }
        }

    }

    void ReadGcodeDir()
    {

        // clear file list and populate files from gcode folder with a "none" at front
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Clear();
        Dropdown.OptionData firstData = new Dropdown.OptionData("none");
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(firstData);

        directoryInfo = new DirectoryInfo(gcodeDir);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

        Debug.Log("[update] file tree");

        foreach (FileInfo file in fileInfo)
        {
            if (file.Name == ".DS_Store") { } // do not load Spotlight files
            else
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData(file.Name);
                GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(optionData);
                // GameObject.Find("loadGcode").GetComponent<Dropdown>().value = 0;
            }
        }
        // lastSelection = 0;
    }
    void LoadGcodeFile()
    {
        if (GameObject.Find("loadGcode").GetComponent<Dropdown>().value != 0)
        {
            Dropdown test = GameObject.Find("loadGcode").GetComponent<Dropdown>();
            string newpath = gcodeDir + "/" + test.options[test.value].text;
            actualSelectedGcode = File.ReadAllLines(newpath);
            actualGCODElines = actualSelectedGcode.Length;
            // Debug.Log("actual selected gecode length: " + actualSelectedGcode.Length);

            parsedGcode = null;
            // Array.Resize(ref parsedGcode, actualSelectedGcode.Length);
            Array.Resize(ref parsedGcode, countGlines());


            // Debug.Log("GCODE Lines: " + actualSelectedGcode.Length);
            Debug.Log("[loading] Gcode File");
            Debug.Log("Selected Gcode-file has :" + countGlines() + " g lines.");

            ParseGcode();

            lastSelection = GameObject.Find("loadGcode").GetComponent<Dropdown>().value;
        }
        else
        {
            GameObject.Find("buildplate").transform.position = new Vector3(1 / 2, -1, 1 / 2);
            GameObject.Find("buildplate").transform.localScale = new Vector3(1, 1, 1);
            GameObject.Find("Main Camera").transform.position = new Vector3(1 / 2, 200, -200);
        }
    }


    int countGlines()
    {
        int temp = parseCounter = 0;
        for (int i = 0; i < actualSelectedGcode.Length; i++)
        {
            if (actualSelectedGcode[i].StartsWith("G")) { temp += 1; }
        }
        return temp;
    }

    void ParseGcode()
    {
        parseCounter = 0;
        for (int i = 0; i < actualSelectedGcode.Length; i++)
        {
            if (actualSelectedGcode[i].StartsWith(";"))
            {
                if (actualSelectedGcode[i].StartsWith(";Dimension"))
                {
                    string[] dimensions = actualSelectedGcode[i].Split(' ');
                    bedX = float.Parse(dimensions[1].Replace('.', ','));
                    bedY = float.Parse(dimensions[2].Replace('.', ','));
                    bedZ = float.Parse(dimensions[3].Replace('.', ','));
                    nozzleDiameter = float.Parse(dimensions[4].Replace('.', ','));

                    Debug.Log(bedX + " " + bedY + " " + bedZ + " " + nozzleDiameter);



                    SetBuildplate(bedX, bedY);
                    Debug.Log(actualSelectedGcode[i]);

                }
                // actual tool position height
                if (actualSelectedGcode[i].StartsWith(";Z:"))
                {
                    string[] temp = actualSelectedGcode[i].Split(':');
                    actualZheight = float.Parse(temp[1].Replace('.', ','));

                }
                // layer Height
                if (actualSelectedGcode[i].StartsWith(";HEIGHT"))
                {
                    string[] temp = actualSelectedGcode[i].Split(':');
                    actualLayerHeight = float.Parse(temp[1].Replace('.', ','));

                }
            }



            // Debug.Log(i + " : " + actualSelectedGcode[i]);
            if (actualSelectedGcode[i].StartsWith("G"))
            {

                // Array.Resize(ref parsedGcode, parsedGcode.Length+1);
                parsedGcode[parseCounter] = new string[6];

                if (actualSelectedGcode[i].StartsWith("G0"))
                {
                    // Debug.Log("code length " + actualSelectedGcode[i].Length);
                    string[] g0 = actualSelectedGcode[i].Split(' ');
                    // Debug.Log("debug go : " + g0);
                    // Debug.Log("G0 code length " + g0.Length);
                    String x_temp = "";
                    String y_temp = "";
                    // String z_temp = "";
                    String f_temp = "";
                    String e_temp = "";

                    for (int l = 0; l < g0.Length; l++)
                    {
                        switch (g0[l][0])
                        {
                            case 'F':
                                f_temp = g0[l].Replace('.', ',').Substring(1);
                                lastF = g0[l].Replace('.', ',').Substring(1);
                                break;
                            case 'X':
                                x_temp = g0[l].Replace('.', ',').Substring(1);
                                lastX = g0[l].Replace('.', ',').Substring(1);
                                break;
                            case 'Y':
                                y_temp = g0[l].Replace('.', ',').Substring(1);
                                lastY = g0[l].Replace('.', ',').Substring(1);
                                break;
                            case 'Z':
                                z_temp = g0[l].Replace('.', ',').Substring(1);
                                lastZ = g0[l].Replace('.', ',').Substring(1);
                                break;
                            case 'E':
                                e_temp = g0[l].Replace('.', ',').Substring(1);
                                break;
                            default:
                                break;
                        }
                    }

                    // if (string.IsNullOrEmpty(x_temp)) { x_temp = " "; }
                    if (string.IsNullOrEmpty(x_temp)) { x_temp = lastX; }
                    if (string.IsNullOrEmpty(y_temp)) { y_temp = lastY; }
                    // if (string.IsNullOrEmpty(z_temp)) { z_temp = lastZ; }
                    if (string.IsNullOrEmpty(e_temp)) { e_temp = " "; }
                    if (string.IsNullOrEmpty(f_temp)) { f_temp = lastF; }
                    // if (x_temp == null) { x_temp = lastX; }
                    // if (y_temp == null) { y_temp = lastY; }
                    // if (z_temp == null) { z_temp = lastZ; }
                    // if (f_temp == null) { f_temp = lastF; }
                    // if (e_temp == null) { e_temp = " "; }

                    // Debug.Log("NEW " + x_temp + " : " + y_temp + " : " + z_temp + " : " + f_temp + " : " + e_temp);
                    // Debug.Log("old--- " + lastX + " : " + lastY + " : " + lastZ + " : " + lastF + " : " + lastE);



                    parsedGcode[parseCounter] = new String[] { "G0", x_temp, y_temp, z_temp, f_temp, e_temp };

                    // Debug.Log("G0" + " x: " + x_temp + " y: " +  y_temp + " z: " +  z_temp + " f: " +  f_temp + " e: " +  e_temp);
                    adebugtext += parsedGcode[parseCounter][0] + " : " + parsedGcode[parseCounter][1] + " : " + parsedGcode[parseCounter][2] + " : " + parsedGcode[parseCounter][3] + " : " + parsedGcode[parseCounter][4] + " : " + parsedGcode[parseCounter][5] + "\n";
                    // System.IO.File.WriteAllText("filename.txt", string.Join("\n", parsedGcode[i][0] , " : " + parsedGcode[i][1] , " : " , parsedGcode[i][2] , " : " , parsedGcode[i][3] , " : " , parsedGcode[i][4] , " : " , parsedGcode[i][5] , " : "));
                    parseCounter += 1;
                }
                if (actualSelectedGcode[i].StartsWith("G1"))
                {
                    // Debug.Log("code length " + actualSelectedGcode[i].Length);
                    string[] g1 = actualSelectedGcode[i].Split(' ');
                    // Debug.Log("debug go : " + g0);
                    // Debug.Log("G0 code length " + g0.Length);
                    String x_temp = "";
                    String y_temp = "";
                    // String z_temp = "";
                    String f_temp = "";
                    String e_temp = "";

                    for (int l = 0; l < g1.Length; l++)
                    {
                        switch (g1[l][0])
                        {
                            case 'F':
                                f_temp = g1[l].Replace('.', ',').Substring(1);
                                break;
                            case 'X':
                                x_temp = g1[l].Replace('.', ',').Substring(1);
                                break;
                            case 'Y':
                                y_temp = g1[l].Replace('.', ',').Substring(1);
                                break;
                            case 'Z':
                                // y_temp = g1[l].Replace('.', ',').Substring(1);
                                break;
                            case 'E':
                                e_temp = g1[l].Replace('.', ',').Substring(1);
                                break;
                            default:
                                break;
                        }
                    }

                    // if (string.IsNullOrEmpty(x_temp)) { x_temp = " "; }

                    // if (x_temp == null) { x_temp = lastX; }
                    // if (y_temp == null) { y_temp = lastY; }
                    // if (z_temp == null) { z_temp = lastZ; }
                    // if (f_temp == null) { f_temp = lastF; }
                    // if (e_temp == null) { e_temp = " "; }

                    // Debug.Log("NEW " + x_temp + " : " + y_temp + " : " + z_temp + " : " + f_temp + " : " + e_temp);
                    // Debug.Log("old--- " + lastX + " : " + lastY + " : " + lastZ + " : " + lastF + " : " + lastE);

                    // if (x_temp == null) { x_temp = " "; }
                    // if (y_temp == null) { y_temp = " "; }
                    // if (z_temp == null) { z_temp = " "; }
                    // if (f_temp == null) { f_temp = " "; }
                    // if (e_temp == null) { e_temp = " "; }


                    if (e_temp.StartsWith("-") || (string.IsNullOrEmpty(x_temp) && string.IsNullOrEmpty(y_temp)))
                    {

                    }
                    else
                    {
                        if (string.IsNullOrEmpty(x_temp)) { x_temp = lastX; }
                        if (string.IsNullOrEmpty(y_temp)) { y_temp = lastY; }
                        if (string.IsNullOrEmpty(z_temp)) { z_temp = lastZ; }
                        if (string.IsNullOrEmpty(e_temp)) { e_temp = " "; }
                        if (string.IsNullOrEmpty(f_temp)) { f_temp = lastF; }



                        parsedGcode[parseCounter] = new String[] { "G1", x_temp, y_temp, z_temp, f_temp, e_temp };

                        adebugtext += parsedGcode[parseCounter][0] + " : " + parsedGcode[parseCounter][1] + " : " + parsedGcode[parseCounter][2] + " : " + parsedGcode[parseCounter][3] + " : " + parsedGcode[parseCounter][4] + " : " + parsedGcode[parseCounter][5] + "\n";
                        // System.IO.File.WriteAllText("file
                        // System.IO.File.WriteAllText("filename.txt", string.Join("\n", parsedGcode[i][0] + " : " + parsedGcode[i][1] + " : " + parsedGcode[i][2] + " : " + parsedGcode[i][3] + " : " + parsedGcode[i][4] + " : " + parsedGcode[i][5] + " : "), bool True);
                        parseCounter += 1;
                    }


                }
            }
        }

        System.IO.File.WriteAllText("clean.txt", adebugtext);
        // Debug.Log("Parse test: "+ float.Parse(parsedGcode[10][1]) );

        // create_mesh();

        // Vector3 two = new Vector3(0.0f, 0.0f, 0.0f);
        // Vector3 eins = new Vector3(100.0f, 100.0f, 100.0f);
        // create_segment(eins, two);
        // float.Parse()
        testausgabe();

    }

    void testausgabe()
    {
        Debug.Log("Testausgabe : " + parsedGcode.Length);

        // Vector3 eins = new Vector3(20.0f, 0.3f/2, 20.0f);
        // Vector3 two = new Vector3(200.0f, 0.3f/2, 40.0f);
        // create_segment(eins, two);
        create_mesh();

        // for (int i = 0; i < parsedGcode.Length; i++)
        // {
        //     Debug.Log("Testausgabe : " + parsedGcode[i].Length);
        //     // for (int s = 0; s < 1; s++)
        //     // {
        //     //     if (string.IsNullOrEmpty(parsedGcode[i][s])) { Debug.Log("WERTE leer: " + i + " : " + s  ); }
        //     //     // Debug.Log("BEUG" + parsedGcode[i][s]);
        //     // }           
        // }

    }

    void SetBuildplate(float x, float y)
    {
        if (GameObject.Find("loadGcode").GetComponent<Dropdown>().value != 0)
        {
            GameObject.Find("buildplate").transform.position = new Vector3(x / 2, -0.5f, y / 2);
            GameObject.Find("buildplate").transform.localScale = new Vector3(x, 1, y);
            GameObject.Find("Main Camera").transform.position = new Vector3(x / 2, 200, -200);

            // Transform bp = GameObject.Find("buildplate").transform;
            // // GameObject.Find("MainCamera").transform.LookAt(bp);
        }
        // else
        // {
        //     GameObject.Find("buildplate").transform.position = new Vector3(1 / 2 / 1000, -1, 1 / 2 / 1000);
        //     GameObject.Find("buildplate").transform.localScale = new Vector3(1 / 1000, 1, 1 / 1000);
        //     GameObject.Find("Main Camera").transform.position = new Vector3(1 / 2 / 1000, 200, -200);
        // }
    }

    void create_segment(Vector3 p1, Vector3 p2, float extrusionWidth, float extrusionHeight)
    {
        // Debug.Log("P1 :" + p1.x + " " + p1.y + " " + p1.z);
        // Debug.Log("P2 :" + p2.x + " " + p2.y + " " + p2.z);
        Vector3 pos = Vector3.Lerp(p2, p1, (float)0.5);
        Vector3 zero = new Vector3(0, 0, 0);
        GameObject segObj = (GameObject)Instantiate(segment);
        segObj.transform.position = pos;
        segObj.transform.up = p2 - p1;
        float laenge = (Vector3.Distance(p1, p2));
        Debug.Log("laenge segemet: " + laenge);

        // segObj.transform.localScale = new Vector3(1,(1.0f*laenge/2f) , 1);
        segObj.transform.localScale = new Vector3(extrusionWidth, laenge / 2, extrusionHeight);

        segObj.transform.position = zero;
        segObj.transform.position = pos;
        // Debug.Log("pos.x :" + pos.x);
        // Debug.Log("pos.y :" + pos.y);
        // Debug.Log("pos.z :" + pos.z);
        // Debug.Log("---- Distance : " + Vector3.Distance(p1,p2));


        // segObj.transform.localScale.y = Vector3.Distance(p1,p2);
        // segObj.transform.localScale = new Vector3(1.0f, Vector3.Distance(p1,p2), 1.0f);

    }

    void create_mesh()
    {
        for (int i = 7; i < countGlines() - 20; i++)
        {
            Vector3 temp1 = new Vector3(float.Parse(parsedGcode[i - 1][1]),
            // 0.3f,
             float.Parse(parsedGcode[i - 1][3]),
             float.Parse(parsedGcode[i - 1][2]));
            // Debug.Log(temp1);
            Vector3 temp2 = new Vector3(float.Parse(parsedGcode[i][1]),
            // 0.3f,
            float.Parse(parsedGcode[i][3]),
            float.Parse(parsedGcode[i][2]));
            // Debug.Log(temp2);
            create_segment(temp2, temp1, 0.45f, actualLayerHeight);
        }
    }
}