using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class LoadGCODE : MonoBehaviour
{
    string gcodeDir = @"gcode";
    // Start is called before the first frame update
    void Start()
    {
        // clear file list and populate files from gcode folder with a "none" at front
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Clear();
        DirectoryInfo directoryInfo = new DirectoryInfo(gcodeDir);
        Dropdown.OptionData firstData = new Dropdown.OptionData("none");
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(firstData);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

        foreach (FileInfo file in fileInfo)
        {
            if (file.Name == ".DS_Store"){} // do not load Spotlight files
            else
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData(file.Name);
                GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(optionData);
                GameObject.Find("loadGcode").GetComponent<Dropdown>().value = 0;
            }
        }

        // Log gcode file to Log
        // for testing set selected file to file 1 in the list
        GameObject.Find("loadGcode").GetComponent<Dropdown>().value = 1;
        Dropdown test = GameObject.Find("loadGcode").GetComponent<Dropdown>();
        string newpath = gcodeDir + "/" + test.options[test.value].text;
        string[] lines = File.ReadAllLines(newpath);

        for (int i = 0; i < lines.Length; i++)
        {
            Debug.Log(i + " : " + lines[i]);
        }
    }

    void Update()
    {

    }
}