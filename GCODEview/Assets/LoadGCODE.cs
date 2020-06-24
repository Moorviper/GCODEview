using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadGCODE : MonoBehaviour
{
    string gcodeDir = @"gcode";
    // Start is called before the first frame update
    void Start()
    {
        
        DirectoryInfo directoryInfo = new DirectoryInfo(gcodeDir);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Clear();
        foreach (FileInfo file in fileInfo)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData(file.Name);
            GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(optionData);
            GameObject.Find("loadGcode").GetComponent<Dropdown>().value = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}