using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Ultrahaptics;
using System.Timers;
using Valve.VR;

public class Apple_touch : MonoBehaviour
{
    public SteamVR_ActionSet m_ActionSet;
    public SteamVR_Action_Boolean m_BooleanAction;
    public float frequency = 200f;

    TextMeshPro mText;
    AmplitudeModulationEmitter _emitter;

    List<string> currentCollisions = new List<string>();
    bool ExpStarted = false;


    void Start()
    {

        m_BooleanAction = SteamVR_Actions._default.GrabPinch;

        GameObject sphere = GameObject.Find("Apple");
        sphere.GetComponent<Rigidbody>().useGravity = false;

        //ultrahaptics
        // Initialize the emitter
        _emitter = new AmplitudeModulationEmitter();
        _emitter.initialize();


        mText = GameObject.Find("Text (TMP)").GetComponent<TextMeshPro>();
        mText.text = "Place your hand below the apple";
    }



    // Update is called once per frame
    private void Update()
    {
        GameObject leftHand = GameObject.Find("LoPoly Rigged Hand Left");
        GameObject sphere = GameObject.Find("Apple");

        if (leftHand != null && !ExpStarted)
        {
            mText.text = "Now press the trigger button, first ball will fall on your hand.";
        }
        if (m_BooleanAction.stateDown)
        {
            /*if left hand is null and left hand is active and sphere gravity is disabled then enable gravity*/
            if (leftHand != null && leftHand.activeSelf && !sphere.GetComponent<Rigidbody>().useGravity && (sphere.transform.position.y < 4))
            {
                ExpStarted = true;
                sphere.GetComponent<Rigidbody>().useGravity = true;
                StartCoroutine(DisableSphereCoroutine(sphere));

            }
            else if (leftHand == null)
            {
                mText.text = "Please place your left hand on white paper, and make sure it is detected correctly in the scene. If right hand is detected, then remove and keep again.";
            }
        }
    }

    IEnumerator DisableSphereCoroutine(GameObject sphere)
    {
        mText.text = "This ball will be on your hand for 30 seconds, and then it will disappear.";
        yield return new WaitForSeconds(30);
        /**/

        sphere.transform.position = new UnityEngine.Vector3(-1.51f, 9.09f, 2.298f);
        sphere.GetComponent<Rigidbody>().useGravity = false;

        GameObject sphere1 = GameObject.Find("Sphere1");
        sphere1.GetComponent<MotionWP_FR2>().StartScene();

        yield return new WaitForSeconds(1);
        _emitter.Dispose();
        _emitter = null;

    }



    void OnCollisionEnter(Collision collision)
    {

        currentCollisions.Add(collision.gameObject.name);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == "Contact Fingerbone" || collision.gameObject.name == "Contact Palm Bone")
        {

            Ultrahaptics.Vector3 position = new Ultrahaptics.Vector3(0.0f, 0.0f, 0.2f);
            // Create a control point object using this position, with full intensity, at 200Hz


            AmplitudeModulationControlPoint point = new AmplitudeModulationControlPoint(position, 0.9f, 200f);
            // Output this point; technically we don't need to do this every update since nothing is changing.
            _emitter.update(new List<AmplitudeModulationControlPoint> { point });

        }

    }

    private void OnCollisionExit(Collision collision)
    {


        // Remove the GameObject collided with from the list.
        currentCollisions.Remove(collision.gameObject.name);
        // Print the entire list to the console.

        if (!IsobjectStillTouchingHand())
        {
            /*print("on collision exit hand");*/
            _emitter.stop();
        }




    }


    private bool IsobjectStillTouchingHand()
    {
        if (currentCollisions.Contains("Contact Fingerbone") || currentCollisions.Contains("Contact Palm Bone"))
        {
            return true;
        }
        return false;

    }

    /*void OnDisable()
    {
        _emitter.stop();
    }

    // Ensure the emitter is immediately disposed when destroyed
    void OnDestroy()
    {
        _emitter.Dispose();
        _emitter = null;
    }*/

}
