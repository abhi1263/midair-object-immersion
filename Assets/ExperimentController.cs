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
    Stack<String> currentObject = new Stack<String>();
    public SteamVR_ActionSet m_ActionSet;
    public SteamVR_Action_Boolean m_BooleanAction;
    public float frequency = 200f;
    TextMeshPro mText;
    AmplitudeModulationEmitter _emitter;
    private static bool Stop = false; //boolean to stop the emitter

    // Start is called before the first frame update
    void Start()
    {

        String[] arr1 = new String[2] { "ruler", "ruler.csv" };
        String[] arr2 = new String[2] { "ruler", "ruler2.csv" };
        String[] arr3 = new String[2] { "apple", "circle.csv" };
        String[] arr4 = new String[2] { "apple", "cross.csv" };
        controlFlowData.Push(arr4);
        controlFlowData.Push(arr3);
        controlFlowData.Push(arr2);
        controlFlowData.Push(arr1);

        m_BooleanAction = SteamVR_Actions._default.GrabPinch;

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
            String[] currentFlow = controlFlowData.Pop();
            String objectName = currentFlow[0];
            String coordinateFileName = currentFlow[1];

            currentObject.Push(objectName);

            GameObject objectInHand = GameObject.Find(objectName);
            objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 0.80f, 2.44f);
            Task.Factory.StartNew(() => Render(coordinateFileName));
        }
        else {
            String objectName = currentObject.Pop();
            GameObject objectInHand = GameObject.Find(objectName);
            objectInHand.transform.position = new UnityEngine.Vector3(-1.20f, 3.69f, 2.44f);

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
