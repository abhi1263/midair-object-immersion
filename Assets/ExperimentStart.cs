using UnityEngine;
using TMPro;
using Ultrahaptics;
using Valve.VR;
using uVector3 = Ultrahaptics.Vector3;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Timers;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Threading.Tasks;


public class ExperimentStart : MonoBehaviour
{
    List<string[]> virtualObjectsList = new List<string[]> { }; 
    Stack<string[]> currentObject = new Stack<string[]>();

    public SteamVR_ActionSet m_ActionSet;
    public SteamVR_Action_Boolean m_BooleanAction, touchpadtouch;
    public float frequency = 200f; // frequency for UltraHaptics has been set to 200Hz
    TextMeshPro mText;
    AmplitudeModulationEmitter _emitter; // emiiter for UltraHaptics
    private static bool Stop = false; //boolean to stop the emitter

    //Stack<String[]> virtualObjectsControlFlow = new Stack<String[]>();

    private bool ExperimentStarted = false;
    private bool PreExperimentStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        //Initializing a stack with virtual object and their haptic sensation csv file containing the coordinates for UltraHaptics
        string[] arr1 = new string[2] { "ruler", "circle.csv"};
        string[] arr2 = new string[2] { "ruler", "half-circle.csv"};
        string[] arr3 = new string[2] { "ruler", "curve-line.csv"};
        string[] arr4 = new string[2] { "apple", "circle.csv"};
        string[] arr5 = new string[2] { "apple", "half-circle.csv"};
        string[] arr6 = new string[2] { "apple", "curve-line.csv"};
        string[] arr7 = new string[2] { "croissant", "circle.csv"};
        string[] arr8 = new string[2] { "croissant", "half-circle.csv"};
        string[] arr9 = new string[2] { "croissant", "curve-line.csv"};
               
        virtualObjectsList.Add(arr1);
        virtualObjectsList.Add(arr2);
        virtualObjectsList.Add(arr3);
        virtualObjectsList.Add(arr4);
        virtualObjectsList.Add(arr5);
        virtualObjectsList.Add(arr6);
        virtualObjectsList.Add(arr7);
        virtualObjectsList.Add(arr8);
        virtualObjectsList.Add(arr9);        

        // Randomizing the order in which object and their haptic sensation occurs in the scene
        var count = 9;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = virtualObjectsList[i];
            virtualObjectsList[i] = virtualObjectsList[r];
            virtualObjectsList[r] = tmp;
        }
               

        //Initializing Ultrahaptics emitter
        _emitter = new AmplitudeModulationEmitter();
        _emitter.initialize();

        //Initializing HTC Vice controller action
        m_BooleanAction = SteamVR_Actions._default.GrabPinch;        

        mText = GameObject.Find("Text (TMP)").GetComponent<TextMeshPro>();
        mText.text = "Welcome to the user study.\nPlease place your left hand on white paper, and make sure it is detected correctly in the scene.\nPress the trigger button on the controller to begin.";
    }

    // Update is called once per frame
    void Update()
    {
        //GameObject leftHand = GameObject.Find("LoPoly Rigged Hand Left");
        bool leftHand = true;

        // When the trigger button is the pre experiment phase starts
        if (leftHand && m_BooleanAction.stateDown && !ExperimentStarted && !PreExperimentStarted)
        {
            //Before the experiment is started, user is made accustomed to the VR scene and UltraHaptics touch sensation
            
            PreExperimentStarted = true;
            mText.text = "Describe your experience.";
            StartCoroutine(PreExperimentAction());
            
        }
        // When left hand is not detected properly in VR
        else if(!leftHand && !ExperimentStarted)
        {
            mText.text = "The left hand is not detected properly. Please remove hand and place it again in the white paper.";
        }
        //Pre experiment phase is done. Starting the Experiment 1
        else if(leftHand && ExperimentStarted && m_BooleanAction.stateDown && !PreExperimentStarted)
        {
            StartExperiment1();
        }
    }

    // Function to render a single haptic point for 30 seconds || Part of pre experiment phase
    IEnumerator PreExperimentAction()
    {                         
        print("Pre Experiment started");

        for(; ; )
        {                               
             Ultrahaptics.Vector3 position = new Ultrahaptics.Vector3(0.0f, 0.0f, 0.2f);
             // Create a control point object using this position, with full intensity, at 200Hz

             AmplitudeModulationControlPoint point = new AmplitudeModulationControlPoint(position, 1.0f, 200f);
             // Output this point; technically we don't need to do this every update since nothing is changing.
             _emitter.update(new List<AmplitudeModulationControlPoint> { point });

             yield return new WaitForSeconds(30);
             _emitter.update(new List<AmplitudeModulationControlPoint> { });

            PreExperimentStarted = false;            
            print("Pre Experiment ended");     
            ExperimentStarted = true;
            
            yield return new WaitForSeconds(1);
            mText.text = "Relax!\nYou have successfully completed the demo level.\nWhenever you're ready press the trigger to begin the Level 1.";           
            _emitter.Dispose();
            _emitter = null;
        }
    }            

       
    // Experiment 1 renders different objects with same haptic sensation
    private void StartExperiment1()
    {
        if(!ExperimentStarted)
        {            
            ExperimentStarted = true;            
        }
        else
        {
            //Experiment 1 begins
            print("Experiment1 started.");
            
            if (_emitter == null)
            {
                if(virtualObjectsList.Count == 0)
                {
                    mText.text = "Thank you for taking part in the user study. You may now dismount the VR headset.";
                }

                string[] currentFlow = virtualObjectsList[0];
                virtualObjectsList.RemoveAt(0);
                string objectName = currentFlow[0];
                string coordinateFileName = currentFlow[1];

                print("Object: "+currentFlow[0]+"\nShape rendered: "+currentFlow[1]);

                currentObject.Push(currentFlow);

                GameObject objectInHand = GameObject.Find(objectName);
                objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 0.80f, 2.44f);
                Task.Factory.StartNew(() => Render(coordinateFileName));
                mText.text = "In this level, you will be able to see and feel different virtual objects. The trigger button is used to navigate through the experiment.\nPlease describe your experience.";
            }
            else 
            {
                string[] currentObjectInGame = currentObject.Pop();
                GameObject objectInHand = GameObject.Find(currentObjectInGame[0]);
                objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 3.69f, 2.44f);                             
            
                Stop_Emitter();
            }

        }
        
    }

    private void Render(string fileName)
    {
        
        Stop = false;
        string file_name = Path.Combine(Environment.CurrentDirectory, fileName);
        
        _emitter = new AmplitudeModulationEmitter();
        _emitter.initialize();

        for (; ; )
        {
            using (TextFieldParser parser = new TextFieldParser(file_name))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    double x = 1000, y = 1000;
                    //Processing row
                    string[] fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        //TODO: Process field
                        if (x == 1000)
                        {
                            x = double.Parse(field);
                        }
                        else if (y == 1000)
                        {
                            y = double.Parse(field);
                        }
                    }
                    uVector3 position = new uVector3((float)(x * Ultrahaptics.Units.metres), (float)(y * Ultrahaptics.Units.metres), (float)(0.20 * Ultrahaptics.Units.metres));
                    AmplitudeModulationControlPoint point = new AmplitudeModulationControlPoint(position, 1.0f, 200f);
                    var points = new List<AmplitudeModulationControlPoint> { point };
                    _emitter.update(points);

                    //this condition will stop emitter from processing further
                    if (Stop)
                    {
                        _emitter.update(new List<AmplitudeModulationControlPoint> { });

                        _emitter.Dispose();
                        _emitter = null;
                        Stop = false;
                        return;
                    }
                }
            }
        }

    }

     private void Stop_Emitter()
     {
        Stop = true;
     }

    private void OnDestroy()
    {
        _emitter.update(new List<AmplitudeModulationControlPoint> { });

        _emitter.Dispose();
        _emitter = null;

    }
 }

