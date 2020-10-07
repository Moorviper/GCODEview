using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using SplineMesh;
// using SplineMeshTiling;
// using SplineNode;


public class LoadGCODE : MonoBehaviour
{
    bool debugPrints = false;

    Slider visibleLayers;


    private float nextActionTime = 0.0f;
    public float period = 5.0f;
    string gcodeDir = @"gcode";
    float nozzleDiameter = 0;
    float bedX = 0;
    float bedY = 0;
    float bedZ = 0;

    int slicerGcode = 0;

    DirectoryInfo directoryInfo = null;
    // FileInfo[] fileInfo = null;
    string[] actualSelectedGcode = null;
    int lastSelection = 0;
    string[][] parsedGcode;

    int parseCounter = 0;
    int actualGCODElines = 0;


    GameObject segment;
    // GameObject meshtest;
    GameObject go;
    String adebugtext = "";

    bool isPrinting = false;



    float scale = 10f;
    float actualZheight = 0f;
    float actualLayerHeight = 0f;
    // float actualExtrusionWidth = 0f;
    // Spline spline;
    // SplineMeshTiling smt;
    // SplineExtrusion splineExtrusion;
    // Material shinyOrange;
    // Material newMat;

    Material Skirt;
    Material OuterShell;
    Material InnerShell;
    Material SolidInfill;
    Material Infill;


    void Start()
    {
        Skirt = Resources.Load<Material>("Skirt");
        OuterShell = Resources.Load<Material>("OuterShell");
        InnerShell = Resources.Load<Material>("InnerShell");
        SolidInfill = Resources.Load<Material>("SolidInfill");
        Infill = Resources.Load<Material>("Infill");

        GameObject temp = GameObject.Find("Slider");

        Debug.Log(temp.GetComponent<Slider>().value);

        visibleLayers = temp.GetComponent<Slider>();

        visibleLayers.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

    }
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            // Debug.Log("Parse test: " + float.Parse("X62.229".Replace('.', ',').Substring(1)));

            // Debug.Log("read gcode dir :   Last selection: " + lastSelection + "  value selected : " + GameObject.Find("loadGcode").GetComponent<Dropdown>().value);
            ReadGcodeDir();
            if (lastSelection != GameObject.Find("loadGcode").GetComponent<Dropdown>().value)
            {
                LoadGcodeFile();
            }
        }

    }

    public void ValueChangeCheck()
    {
        if (debugPrints)
        {
            Debug.Log("Visible Layers :" + visibleLayers.value);
        }
        
        ParseGcode();
    }

    void ReadGcodeDir()
    {

        // clear file list and populate files from gcode folder with a "none" at front
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Clear();
        Dropdown.OptionData firstData = new Dropdown.OptionData("none");
        GameObject.Find("loadGcode").GetComponent<Dropdown>().options.Add(firstData);

        directoryInfo = new DirectoryInfo(gcodeDir);
        FileInfo[] fileInfo = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);


        if (debugPrints)
        {
            Debug.Log("[update] file tree");
        }


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
            // Array.Resize(ref parsedGcode, countGlines());




            // Debug.Log("GCODE Lines: " + actualSelectedGcode.Length);
            if (debugPrints)
            {
                Debug.Log("[loading] Gcode File");
            }

            // Debug.Log("Selected Gcode-file has :" + countGlines() + " g lines.");

            // segment.transform.localScale = new Vector3(0.2f, 1, 0.45f);

            lastSelection = GameObject.Find("loadGcode").GetComponent<Dropdown>().value;
            killExtrusions();
            // killClones();
            // Destroy(meshtest);
            // Destroy(go);

            ParseGcode();


        }
        else
        {
            isPrinting = false;
            killExtrusions();
            // Destroy(meshtest);
            // Destroy(go);
            GameObject.Find("buildplate").transform.position = new Vector3(1 / 2, -1, 1 / 2);
            GameObject.Find("buildplate").transform.localScale = new Vector3(1, 1, 1);
            GameObject.Find("Main Camera").transform.position = new Vector3(1 / 2, 200, -200);
            segment.transform.localScale = new Vector3(0.2f, 1, 0.45f);
        }
    }

    void killExtrusions()
    {
        GameObject[] clones;
        // clones = GameObject.Find("(Clone)");
        clones = GameObject.FindGameObjectsWithTag("extrusion");

        foreach (GameObject clone in clones)
        {
            Destroy(clone);
        }
    }
    int countGlines()
    {
        //Debug.Log("ACTUAL SELECTED GCODE :   " + actualSelectedGcode.Length);

        int temp = parseCounter = 0;
        for (int i = 0; i < actualSelectedGcode.Length; i++)
        {
            if (actualSelectedGcode[i].StartsWith("G0")) { temp += 1; }
            if (actualSelectedGcode[i].StartsWith("G1")) { temp += 1; }

        }
        return temp;
    }

    void ParseGcode()
    {
        killExtrusions();
        // extrusionlines = null;
        // Array.Resize(ref extrusionlines, 1000); // 1000 should Unity anyway crashes long before because it is crap.

        for (int i = 0; i < actualSelectedGcode.Length; i++)
        {
            // misc stuff
            if (actualSelectedGcode[i].StartsWith(";"))
            {
                if (actualSelectedGcode[i].StartsWith(";Sliced"))
                {
                    string[] slicerVersion = actualSelectedGcode[i].Split(' ');
                    if (debugPrints)
                    {
                        Debug.Log("SLICER: " + slicerVersion[2] + " " + slicerVersion[3]);
                    }

                    slicerGcode = 1;

                }
                if (actualSelectedGcode[i].StartsWith(";Dimension"))
                {
                    string[] dimensions = actualSelectedGcode[i].Split(' ');
                    bedX = float.Parse(dimensions[1].Replace('.', ','));
                    bedY = float.Parse(dimensions[2].Replace('.', ','));
                    bedZ = float.Parse(dimensions[3].Replace('.', ','));
                    nozzleDiameter = float.Parse(dimensions[4].Replace('.', ','));

                    SetBuildplate(bedX, bedY);
                    if (debugPrints)
                    {
                        Debug.Log(actualSelectedGcode[i]);
                    }

                }
                // actual tool position height
                if (actualSelectedGcode[i].StartsWith(";Z:"))
                {
                    string[] temp = actualSelectedGcode[i].Split(':');
                    actualZheight = float.Parse(temp[1].Replace('.', ','));
                    if (debugPrints)
                    {
                        Debug.Log("GECODE Z HEIGT ACTUAL " + actualZheight);
                    }


                }
                // layer Height
                if (actualSelectedGcode[i].StartsWith(";HEIGHT"))
                {
                    string[] temp = actualSelectedGcode[i].Split(':');
                    actualLayerHeight = float.Parse(temp[1].Replace('.', ','));

                }
                if (actualSelectedGcode[i].StartsWith(";LAYER:0"))
                {
                    isPrinting = true;
                }
                if (actualSelectedGcode[i].StartsWith(";End"))
                {
                    isPrinting = false;
                }
                if (debugPrints)
                {
                    if (actualSelectedGcode[i].StartsWith(";TYPE")) // write all feature in file
                    {
                        adebugtext += actualSelectedGcode[i] + "\n";
                        System.IO.File.WriteAllText("clean.txt", adebugtext);
                    }
                }

            }


            // Soported feature types
            // ; TYPE: WALL - INNER
            // ; TYPE: WALL - OUTER
            // ; TYPE: SOLID - FILL
            // ; TYPE: SKIRT
            // ; TYPE: FILL
            if (actualSelectedGcode[i].StartsWith(";TYPE:SKIRT"))
            {
                GameObject meshtest = new GameObject();
                meshtest.tag = "extrusion";
                meshtest.name = actualSelectedGcode[i];

                meshtest.AddComponent<Spline>();
                meshtest.AddComponent<SplineMeshTiling>();

               



                Spline spline = meshtest.GetComponent<Spline>();
                spline.tag = "extrusion";
                SplineMeshTiling smt = meshtest.GetComponent<SplineMeshTiling>();
                smt.tag = "extrusion";

                smt.updateInPlayMode = true;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Filament: " + actualSelectedGcode[i];
                go.tag = "extrusion";

                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                smt.mesh = mesh; // test
                // Debug.Log(Skirt + "     - materials");
                // Debug.Log(smt.material + " -- -- --");
                smt.material = Skirt;
                // Debug.Log(smt.material + " -- -- --");
                smt.rotation = new Vector3(0.0f, 0.0f, 0.0f);
                smt.scale = new Vector3(0.2f * scale, 0.2f * scale, 0.45f * scale);

                 if (visibleLayers.value >= (actualZheight * scale))
                {
                    meshtest.SetActive(true);
                    go.SetActive(true);

                }
                else
                {
                    meshtest.SetActive(false);
                    go.SetActive(false);

                }


                string[] before = actualSelectedGcode[(i - 2)].Split(' ');
                float xx;
                float yy;
                xx = float.Parse(before[2].Replace('.', ',').Substring(1));
                yy = float.Parse(before[3].Replace('.', ',').Substring(1));

                if (debugPrints)
                {
                    Debug.Log("X :" + xx + "   Y: " + yy);
                }

                spline.AddNode(new SplineNode(new Vector3(xx * scale, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale), new Vector3(xx * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale - 0.000001f)));
                // spline.AddNode(new SplineNode(new Vector3(x * scale, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale), new Vector3(x * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale - 0.000001f)));

                if (debugPrints)
                {
                    Debug.Log("Aktuelle Zeile : " + actualSelectedGcode[i] + "  " + i);
                }

                int n = 1;
                while ((i + n) < actualSelectedGcode.Length)
                {
                    if (actualSelectedGcode[i + n].StartsWith(";LAYER")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";End GCode")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";TYPE"))
                    {
                        if (debugPrints)
                        {
                            Debug.Log("I: " + i + " n: " + n + "Aktuelle Zeile : " + i + "   " + actualSelectedGcode[i]);
                        }

                        break;
                    }
                    if (actualSelectedGcode[i + n].StartsWith("G1")) // extrusion moves
                    {
                        if (debugPrints)
                        {
                            Debug.Log("G1 ------------------------- ");
                        }


                        float posX;
                        float posY;

                        string[] cords = actualSelectedGcode[i + n].Split(' ');


                        if (cords[1].StartsWith("F"))
                        {
                            posX = float.Parse(cords[2].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[3].Replace('.', ',').Substring(1));
                        }
                        else
                        {
                            posX = float.Parse(cords[1].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[2].Replace('.', ',').Substring(1));
                        }
                        spline.AddNode(new SplineNode(new Vector3(posX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale), new Vector3(posX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale - 0.000001f)));
                    }
                    n++;
                }
            }

            // --------------------------------------------------------------------------------------

            if (actualSelectedGcode[i].StartsWith(";TYPE:WALL-OUTER"))
            {
                GameObject meshtest = new GameObject();
                meshtest.tag = "extrusion";
                meshtest.name = actualSelectedGcode[i];

                meshtest.AddComponent<Spline>();
                meshtest.AddComponent<SplineMeshTiling>();


                Spline spline = meshtest.GetComponent<Spline>();
                SplineMeshTiling smt = meshtest.GetComponent<SplineMeshTiling>();

                spline.tag = "extrusion";
                smt.tag = "extrusion";

                smt.updateInPlayMode = true;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Filament: " + actualSelectedGcode[i];
                go.tag = "extrusion";

                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                smt.mesh = mesh; // test
                // Debug.Log(Skirt + "     - materials");
                // Debug.Log(smt.material + " -- -- --");
                smt.material = OuterShell;
                // Debug.Log(smt.material + " -- -- --");
                smt.rotation = new Vector3(0.0f, 0.0f, 0.0f);
                smt.scale = new Vector3(0.19f * scale, 0.2f * scale, 0.4f * scale);

                 if (visibleLayers.value >= (actualZheight * scale))
                {
                    meshtest.SetActive(true);
                    go.SetActive(true);

                }
                else
                {
                    meshtest.SetActive(false);
                    go.SetActive(false);

                }

                string[] before = actualSelectedGcode[(i - 1)].Split(' ');
                float xx;
                float yy;
                if (before[1].StartsWith("F"))
                {
                    xx = float.Parse(before[2].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[3].Replace('.', ',').Substring(1));
                }
                else
                {
                    xx = float.Parse(before[1].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[2].Replace('.', ',').Substring(1));
                }

                if (debugPrints)
                {
                    Debug.Log("X :" + xx + "   Y: " + yy);
                }

                spline.AddNode(new SplineNode(new Vector3(xx * scale, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale), new Vector3(xx * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale - 0.000001f)));
                // spline.AddNode(new SplineNode(new Vector3(x * scale, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale), new Vector3(x * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale - 0.000001f)));

                if (debugPrints)
                {
                    Debug.Log("Aktuelle Zeile : " + actualSelectedGcode[i] + "  " + i);
                }

                int n = 1;
                while ((i + n) < actualSelectedGcode.Length)
                {
                    if (actualSelectedGcode[i + n].StartsWith(";LAYER")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";End GCode")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";TYPE"))
                    {
                        if (debugPrints)
                        {
                            Debug.Log("I: " + i + " n: " + n + "Aktuelle Zeile : " + i + "   " + actualSelectedGcode[i]);
                        }

                        break;
                    }
                    if (actualSelectedGcode[i + n].StartsWith("G1")) // extrusion moves
                    {
                        if (debugPrints)
                        {
                            Debug.Log("G1 ------------------------- ");
                        }


                        float posX;
                        float posY;

                        string[] cords = actualSelectedGcode[i + n].Split(' ');


                        if (cords[1].StartsWith("F"))
                        {
                            posX = float.Parse(cords[2].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[3].Replace('.', ',').Substring(1));
                        }
                        else
                        {
                            posX = float.Parse(cords[1].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[2].Replace('.', ',').Substring(1));
                        }
                        spline.AddNode(new SplineNode(new Vector3(posX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale), new Vector3(posX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale - 0.000001f)));
                    }
                    n++;
                }
            }

            // -----------------------------------------------

            if (actualSelectedGcode[i].StartsWith(";TYPE:WALL-INNER"))
            {
                GameObject meshtest = new GameObject();
                meshtest.tag = "extrusion";
                meshtest.name = actualSelectedGcode[i];

                meshtest.AddComponent<Spline>();
                meshtest.AddComponent<SplineMeshTiling>();


                Spline spline = meshtest.GetComponent<Spline>();
                SplineMeshTiling smt = meshtest.GetComponent<SplineMeshTiling>();

                spline.tag = "extrusion";
                smt.tag = "extrusion";

                smt.updateInPlayMode = true;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Filament: " + actualSelectedGcode[i];
                go.tag = "extrusion";

                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                smt.mesh = mesh; // test
                // Debug.Log(Skirt + "     - materials");
                // Debug.Log(smt.material + " -- -- --");
                smt.material = InnerShell;
                // Debug.Log(smt.material + " -- -- --");
                smt.rotation = new Vector3(0.0f, 0.0f, 0.0f);
                smt.scale = new Vector3(0.19f * scale, 0.2f * scale, 0.4f * scale);

                 if (visibleLayers.value >= (actualZheight * scale))
                {
                    meshtest.SetActive(true);
                    go.SetActive(true);

                }
                else
                {
                    meshtest.SetActive(false);
                    go.SetActive(false);

                }

                string[] before = actualSelectedGcode[(i - 1)].Split(' ');
                float xx;
                float yy;
                if (before[1].StartsWith("F"))
                {
                    xx = float.Parse(before[2].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[3].Replace('.', ',').Substring(1));
                }
                else
                {
                    xx = float.Parse(before[1].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[2].Replace('.', ',').Substring(1));
                }

                if (debugPrints)
                {
                    Debug.Log("X :" + xx + "   Y: " + yy);
                }

                spline.AddNode(new SplineNode(new Vector3(xx * scale, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale), new Vector3(xx * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale - 0.000001f)));
                // spline.AddNode(new SplineNode(new Vector3(x * scale, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale), new Vector3(x * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale - 0.000001f)));

                if (debugPrints)
                {
                    Debug.Log("Aktuelle Zeile : " + actualSelectedGcode[i] + "  " + i);
                }

                int n = 1;
                while ((i + n) < actualSelectedGcode.Length)
                {
                    if (actualSelectedGcode[i + n].StartsWith(";LAYER")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";End GCode")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";TYPE"))
                    {
                        if (debugPrints)
                        {
                            Debug.Log("I: " + i + " n: " + n + "Aktuelle Zeile : " + i + "   " + actualSelectedGcode[i]);
                        }

                        break;
                    }
                    if (actualSelectedGcode[i + n].StartsWith("G1")) // extrusion moves
                    {
                        if (debugPrints)
                        {
                            Debug.Log("G1 ------------------------- ");
                        }


                        float posX;
                        float posY;

                        string[] cords = actualSelectedGcode[i + n].Split(' ');


                        if (cords[1].StartsWith("F"))
                        {
                            posX = float.Parse(cords[2].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[3].Replace('.', ',').Substring(1));
                        }
                        else
                        {
                            posX = float.Parse(cords[1].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[2].Replace('.', ',').Substring(1));
                        }
                        spline.AddNode(new SplineNode(new Vector3(posX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale), new Vector3(posX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale - 0.000001f)));
                    }
                    n++;
                }
            }

            // -----------------------------------------------

            if (actualSelectedGcode[i].StartsWith(";TYPE:SOLID-FILL"))
            {
                GameObject meshtest = new GameObject();
                meshtest.tag = "extrusion";
                meshtest.name = actualSelectedGcode[i];

                meshtest.AddComponent<Spline>();
                meshtest.AddComponent<SplineMeshTiling>();


                Spline spline = meshtest.GetComponent<Spline>();
                SplineMeshTiling smt = meshtest.GetComponent<SplineMeshTiling>();

                spline.tag = "extrusion";
                smt.tag = "extrusion";

                smt.updateInPlayMode = true;
                smt.curveSpace = true;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Filament: " + actualSelectedGcode[i];
                go.tag = "extrusion";

                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                smt.mesh = mesh; // test
                // Debug.Log(Skirt + "     - materials");
                // Debug.Log(smt.material + " -- -- --");
                smt.material = SolidInfill;
                // Debug.Log(smt.material + " -- -- --");
                smt.rotation = new Vector3(0.0f, 0.0f, 0.0f);
                smt.scale = new Vector3(0.19f * scale, 0.2f * scale, 0.4f * scale);

                 if (visibleLayers.value >= (actualZheight * scale))
                {
                    meshtest.SetActive(true);
                    go.SetActive(true);

                }
                else
                {
                    meshtest.SetActive(false);
                    go.SetActive(false);

                }

                string[] before = actualSelectedGcode[(i - 1)].Split(' ');
                float xx;
                float yy;
                if (before[1].StartsWith("F"))
                {
                    xx = float.Parse(before[2].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[3].Replace('.', ',').Substring(1));
                }
                else
                {
                    xx = float.Parse(before[1].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[2].Replace('.', ',').Substring(1));
                }

                if (debugPrints)
                {
                    Debug.Log("X :" + xx + "   Y: " + yy);
                }

                spline.AddNode(new SplineNode(new Vector3(xx * scale, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale), new Vector3(xx * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale - 0.000001f)));
                // spline.AddNode(new SplineNode(new Vector3(x * scale, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale), new Vector3(x * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale - 0.000001f)));

                if (debugPrints)
                {
                    Debug.Log("Aktuelle Zeile : " + actualSelectedGcode[i] + "  " + i);
                }

                int n = 1;
                while ((i + n) < actualSelectedGcode.Length)
                {
                    if (actualSelectedGcode[i + n].StartsWith(";LAYER")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";End GCode")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";TYPE"))
                    {
                        if (debugPrints)
                        {
                            Debug.Log("I: " + i + " n: " + n + "Aktuelle Zeile : " + i + "   " + actualSelectedGcode[i]);
                        }

                        break;
                    }
                    if (actualSelectedGcode[i + n].StartsWith("G1")) // extrusion moves
                    {
                        if (debugPrints)
                        {
                            Debug.Log("G1 ------------------------- ");
                        }


                        float posX;
                        float posY;

                        string[] cords = actualSelectedGcode[i + n].Split(' ');


                        if (cords[1].StartsWith("F"))
                        {
                            posX = float.Parse(cords[2].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[3].Replace('.', ',').Substring(1));
                        }
                        else
                        {
                            posX = float.Parse(cords[1].Replace('.', ',').Substring(1));
                            posY = float.Parse(cords[2].Replace('.', ',').Substring(1));
                        }
                        spline.AddNode(new SplineNode(new Vector3(posX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale), new Vector3(posX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale - 0.000001f)));
                    }
                    n++;
                }
            }



            // -----------------------------------------------

            if (actualSelectedGcode[i].StartsWith(";TYPE:FILL"))
            {
                GameObject meshtest = new GameObject();
                meshtest.tag = "extrusion";
                meshtest.name = actualSelectedGcode[i];

                meshtest.AddComponent<Spline>();
                meshtest.AddComponent<SplineMeshTiling>();


                Spline spline = meshtest.GetComponent<Spline>();
                SplineMeshTiling smt = meshtest.GetComponent<SplineMeshTiling>();

                spline.tag = "extrusion";
                smt.tag = "extrusion";

                smt.updateInPlayMode = true;
                smt.curveSpace = true;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Filament: " + actualSelectedGcode[i];
                go.tag = "extrusion";

                Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
                smt.mesh = mesh; // test
                // Debug.Log(Skirt + "     - materials");
                // Debug.Log(smt.material + " -- -- --");
                smt.material = Infill;
                // Debug.Log(smt.material + " -- -- --");
                smt.rotation = new Vector3(0.0f, 0.0f, 0.0f);
                smt.scale = new Vector3(0.19f * scale, 0.2f * scale, 0.4f * scale);

                 if (visibleLayers.value >= (actualZheight * scale))
                {
                    meshtest.SetActive(true);
                    go.SetActive(true);

                }
                else
                {
                    meshtest.SetActive(false);
                    go.SetActive(false);

                }

                string[] before = actualSelectedGcode[(i - 1)].Split(' ');
                float xx;
                float yy;
                if (before[1].StartsWith("F"))
                {
                    xx = float.Parse(before[2].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[3].Replace('.', ',').Substring(1));
                }
                else
                {
                    xx = float.Parse(before[1].Replace('.', ',').Substring(1));
                    yy = float.Parse(before[2].Replace('.', ',').Substring(1));
                }

                if (debugPrints)
                {
                    Debug.Log("X :" + xx + "   Y: " + yy);
                }

                spline.AddNode(new SplineNode(new Vector3(xx * scale, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale), new Vector3(xx * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, yy * scale - 0.000001f)));
                // spline.AddNode(new SplineNode(new Vector3(x * scale, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale), new Vector3(x * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, y * scale - 0.000001f)));

                if (debugPrints)
                {
                    Debug.Log("Aktuelle Zeile : " + actualSelectedGcode[i] + "  " + i);
                }

                int n = 1;
                while ((i + n) < actualSelectedGcode.Length)
                {
                    if (actualSelectedGcode[i + n].StartsWith(";LAYER")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";End GCode")) { break; }
                    if (actualSelectedGcode[i + n].StartsWith(";TYPE"))
                    {
                        if (debugPrints)
                        {
                            Debug.Log("I: " + i + " n: " + n + "Aktuelle Zeile : " + i + "   " + actualSelectedGcode[i]);
                        }

                        break;
                    }
                    if (actualSelectedGcode[i + n].StartsWith("G1")) // extrusion moves
                    {
                        if (debugPrints)
                        {
                            Debug.Log("G1 ------------------------- ");
                        }

                        if (actualSelectedGcode[i + n - 1].StartsWith(";"))
                        {
                            Debug.Log("G1 -------first in infill ------------------ ");
                        }
                        else
                        {

                            string[] BeforeCords = actualSelectedGcode[i + n - 1].Split(' ');
                            float beforeX;
                            float beforeY;

                            float posX;
                            float posY;

                            string[] cords = actualSelectedGcode[i + n].Split(' ');



                            if (BeforeCords[1].StartsWith("F"))
                            {
                                beforeX = float.Parse(BeforeCords[2].Replace('.', ',').Substring(1));
                                beforeY = float.Parse(BeforeCords[3].Replace('.', ',').Substring(1));
                            }
                            else
                            {
                                beforeX = float.Parse(BeforeCords[1].Replace('.', ',').Substring(1));
                                beforeY = float.Parse(BeforeCords[2].Replace('.', ',').Substring(1));
                            }


                            if (cords[1].StartsWith("F"))
                            {
                                posX = float.Parse(cords[2].Replace('.', ',').Substring(1));
                                posY = float.Parse(cords[3].Replace('.', ',').Substring(1));
                            }
                            else
                            {
                                posX = float.Parse(cords[1].Replace('.', ',').Substring(1));
                                posY = float.Parse(cords[2].Replace('.', ',').Substring(1));
                            }
                            spline.AddNode(new SplineNode(new Vector3(beforeX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, beforeY * scale), new Vector3(beforeX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, beforeY * scale - 0.000001f)));
                            spline.AddNode(new SplineNode(new Vector3(posX * scale, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale), new Vector3(posX * scale - 0.000001f, (actualZheight - (actualLayerHeight / 2)) * scale, posY * scale - 0.000001f)));


                        }
                    }
                    n++;
                }
            }













        }
        visibleLayers.maxValue = (actualZheight  * scale);
    }



    void SetBuildplate(float x, float y)
    {
        if (debugPrints)
        {
            Debug.Log("setting Buildplate");
        }
        if (GameObject.Find("loadGcode").GetComponent<Dropdown>().value != 0)
        {
            GameObject.Find("buildplate").transform.position = new Vector3(x * scale / 2, -0.5f * scale, y * scale / 2);
            GameObject.Find("buildplate").transform.localScale = new Vector3(x * scale, 1 * scale, y * scale);
            GameObject.Find("Main Camera").transform.position = new Vector3(x * scale / 2, 200 * scale, -200 * scale);
        }
    }

}