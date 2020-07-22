using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class LoadGCODE : MonoBehaviour
{
    string gcodeDir = @"gcode";
    float nozzleDiameter = 0;
    float bedX = 0;
    float bedY = 0;
    float bedZ = 0;
    float actualZheight = 0;

    DirectoryInfo directoryInfo = null;
    FileInfo[] fileInfo = null;
    string[] actualSelectedGcode = null;
    int lastSelection = 0;
    string[][] parsedGcode;
    void Start()
    {
        parsedGcode[0] = new string[] { "Test", "sdads", "dfdf" };
        // parsedGcode[0] = testa;
        // ReadGcodeDir();
    }
    void Update()
    {
        // Debug.Log("read gcode dir :   Last selection: " + lastSelection + "  value selected : " + GameObject.Find("loadGcode").GetComponent<Dropdown>().value);
        ReadGcodeDir();
        if (lastSelection != GameObject.Find("loadGcode").GetComponent<Dropdown>().value)
        {
            LoadGcodeFile();
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

            parsedGcode = null;
            Array.Resize(ref parsedGcode, actualSelectedGcode.Length);
            // Debug.Log("GCODE Lines: " + actualSelectedGcode.Length);
            Debug.Log("[loading] Gcode File");

            // if (lastSelection != GameObject.Find("loadGcode").GetComponent<Dropdown>().value)
            // {
            ParseGcode();
            // }
            lastSelection = GameObject.Find("loadGcode").GetComponent<Dropdown>().value;
        }
    }

    void ParseGcode()
    {
        for (int i = 0; i < actualSelectedGcode.Length; i++)
        {
            parsedGcode[i] = new string[10];
            if (actualSelectedGcode[i].StartsWith(";"))
            {
                if (actualSelectedGcode[i].StartsWith(";Dimension"))
                {
                    string[] dimensions = actualSelectedGcode[i].Split(' ');
                    bedX = float.Parse(dimensions[1]);
                    bedY = float.Parse(dimensions[2]);
                    bedZ = float.Parse(dimensions[3]);
                    nozzleDiameter = float.Parse(dimensions[4]);

                    Debug.Log(bedX + " " + bedY + " " + bedZ + " " + nozzleDiameter);



                    SetBuildplate(bedX, bedY);
                    Debug.Log(actualSelectedGcode[i]);

                }
            }
            // Debug.Log(i + " : " + actualSelectedGcode[i]);
            if (actualSelectedGcode[i].StartsWith("G"))
            {
                if (actualSelectedGcode[i].StartsWith("G0"))
                {
                    // Debug.Log("code length " + actualSelectedGcode[i].Length);
                    string[] g0 = actualSelectedGcode[i].Split(' ');
                    // Debug.Log("debug go : " + g0);
                    // Debug.Log("G0 code length " + g0.Length);
                    String x_temp = "";
                    String y_temp = "";
                    String z_temp = "";
                    String f_temp = "";
                    String e_temp = "";

                    for (int l = 0; l < g0.Length; l++)
                    {
                        switch (g0[l][0])
                        {
                            case 'F':
                                f_temp = g0[l];
                                break;
                            case 'X':
                                x_temp = g0[l];
                                break;
                            case 'Y':
                                y_temp = g0[l];
                                break;
                            case 'Z':
                                y_temp = g0[l];
                                break;
                            case 'E':
                                e_temp = g0[l];
                                break;
                            default:
                                break;
                        }
                    }

                    if (string.IsNullOrEmpty(x_temp)){ x_temp = " ";}
                    if (y_temp == null){ y_temp = " ";}
                    if (z_temp == null){ z_temp = " ";}
                    if (f_temp == null){ f_temp = " ";}
                    if (e_temp == null){ e_temp = " ";}

                    parsedGcode[i] = new String[] {"G0", x_temp, y_temp, z_temp, f_temp, e_temp };
                }
                if (actualSelectedGcode[i].StartsWith("G1"))
                {
                    // Debug.Log("code length " + actualSelectedGcode[i].Length);
                    string[] g1 = actualSelectedGcode[i].Split(' ');
                    // Debug.Log("debug go : " + g0);
                    // Debug.Log("G0 code length " + g0.Length);
                    String x_temp = "";
                    String y_temp = "";
                    String z_temp = "";
                    String f_temp = "";
                    String e_temp = "";

                    for (int l = 0; l < g1.Length; l++)
                    {
                        switch (g1[l][0])
                        {
                            case 'F':
                                f_temp = g1[l];
                                break;
                            case 'X':
                                x_temp = g1[l];
                                break;
                            case 'Y':
                                y_temp = g1[l];
                                break;
                            case 'Z':
                                y_temp = g1[l];
                                break;
                            case 'E':
                                e_temp = g1[l];
                                break;
                            default:
                                break;
                        }
                    }
                    if (x_temp == null){ x_temp = " ";}
                    if (y_temp == null){ y_temp = " ";}
                    if (z_temp == null){ z_temp = " ";}
                    if (f_temp == null){ f_temp = " ";}
                    if (e_temp == null){ e_temp = " ";}

                    parsedGcode[i] = new String[] {"G1", x_temp, y_temp, z_temp, f_temp, e_temp };
                }
            }
        }

        void SetBuildplate(float x, float y)
        {
            if (GameObject.Find("loadGcode").GetComponent<Dropdown>().value != 0)
            {
                GameObject.Find("buildplate").transform.position = new Vector3(x / 2 / 1000, -1, y / 2 / 1000);
                GameObject.Find("buildplate").transform.localScale = new Vector3(x / 1000, 1, y / 1000);
                GameObject.Find("Main Camera").transform.position = new Vector3(x / 2 / 1000, 200, -200);

                // Transform bp = GameObject.Find("buildplate").transform;
                // // GameObject.Find("MainCamera").transform.LookAt(bp);
            }
        }
    }
}