using UnityEngine;
using TMPro;
using Ultrahaptics;
using Valve.VR;
using uVector3 = Ultrahaptics.Vector3;
using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Threading.Tasks;


public class ExperimentController : MonoBehaviour
{
    Stack<String[]> controlFlowData = new Stack<String[]>();
    Stack<String[]> currentObject = new Stack<String[]>();
    Stack<String[]> retryQueue = new Stack<String[]>();

    public SteamVR_ActionSet m_ActionSet;
    public SteamVR_Action_Boolean m_BooleanAction, touchpadtouch;
    public float frequency = 200f;

    TextMeshPro mText;
    AmplitudeModulationEmitter _emitter;
    private static bool Stop = false; //boolean to stop the emitter

    // Start is called before the first frame update
    void Start()
    {

        String[] arr1 = new String[3] { "ruler", "ruler.csv", "start"};
        String[] arr2 = new String[3] { "ruler", "curve-line.csv", "end"};
        String[] arr3 = new String[3] { "apple", "circle.csv", "start" };
        String[] arr4 = new String[3] { "apple", "cross.csv", "end" };
        String[] arr5 = new String[3] { "croissant", "half-circle.csv", "start" };
        String[] arr6 = new String[3] { "croissant", "circle.csv", "end" };
        String[] arr7 = new String[3] { "bagel", "wavy-circle.csv", "start" };
        String[] arr8 = new String[3] { "bagel", "ruler.csv", "end" };
        
        controlFlowData.Push(arr8);
        controlFlowData.Push(arr7);
        controlFlowData.Push(arr6);
        controlFlowData.Push(arr5);
        controlFlowData.Push(arr4);
        controlFlowData.Push(arr3);
        controlFlowData.Push(arr2);
        controlFlowData.Push(arr1);

        m_BooleanAction = SteamVR_Actions._default.GrabPinch;
        touchpadtouch = SteamVR_Actions._default.TouchpadTouch;

        mText = GameObject.Find("Text (TMP)").GetComponent<TextMeshPro>();
        mText.text = "Press trigger button to continue.";
    }

    // Update is called once per frame
    void Update()
    {      
        if (m_BooleanAction.stateDown)
        {
            ControlFlow();
        }
        
    }
   
    private void ControlFlow() {
        if (_emitter == null)
        {
            if(controlFlowData.Count == 0)
            {
                mText.text = "Thank you for taking part in the user study. You may now dismount the VR headset.";
            }
            String[] currentFlow = controlFlowData.Pop();

            
            String objectName = currentFlow[0];
            String coordinateFileName = currentFlow[1];

            print("Object: "+currentFlow[0]+"\nShape rendered: "+currentFlow[1]);

            currentObject.Push(currentFlow);

            GameObject objectInHand = GameObject.Find(objectName);
            objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 0.80f, 2.44f);
            Task.Factory.StartNew(() => Render(coordinateFileName));
            mText.text = "";
        }
        else 
        {
            String[] currentObjectInGame = currentObject.Pop();
            GameObject objectInHand = GameObject.Find(currentObjectInGame[0]);
            objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 3.69f, 2.44f);

            if (currentObjectInGame[2] == "end" && controlFlowData.Count != 0)
            {
                mText.text = "Please dismount the VR headset and fillout the questionnaire.\nAfter questionnaire is completed, press the trigger button to continue.";
            }
            else if (currentObjectInGame[2] == "start")
            {
                mText.text = "Relax and recollect the experience.\nWhenever you are ready press the trigger button to continue.";
            }
            
            Stop_Emitter();
        }
    }

   
    private void Render(String fileName)
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
