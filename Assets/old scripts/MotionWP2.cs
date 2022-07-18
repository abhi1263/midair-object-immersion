using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Ultrahaptics;
using TMPro;
using Valve.VR;


public class MotionWP2 : MonoBehaviour
{
    public SteamVR_ActionSet m_ActionSet;
    public SteamVR_Action_Boolean m_BooleanAction;
    public float intensity = 0.7f;

    AmplitudeModulationEmitter _emitter;
    bool sphereEnabled = false;
    List<string> currentCollisions = new List<string>();

    TextMeshPro mText;

    // Start is called before the first frame update
    void Start()
    {
         m_BooleanAction = SteamVR_Actions._default.GrabPinch;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject sphere = GameObject.Find("Sphere1");
        if (m_BooleanAction.stateDown && sphereEnabled)
        {
            print("state down");
            if (_emitter == null) {
                print("emitter null");
                _emitter = new AmplitudeModulationEmitter();
                _emitter.initialize();
            }
            
            GameObject leftHand = GameObject.Find("LoPoly Rigged Hand Left");
            /*if left hand is null and left hand is active and sphere gravity is disabled then enable gravity*/
            if (leftHand != null && leftHand.activeSelf && !sphere.GetComponent<Rigidbody>().useGravity && (sphere.transform.position.y < 4))
            {
                print("use gravity true");
                sphere.GetComponent<Rigidbody>().useGravity = true;
                StartCoroutine(DisableSphereCoroutine(sphere));
            }
        }
    }

    public void CalledFromMotionWP() {
        print("going inside update");
    }

    public void StartScene() {
        mText = GameObject.Find("Text (TMP)").GetComponent<TextMeshPro>();
        print("motion WP functionh called");
        mText.text = "This is the 2nd ball in front of you. Press the trigger button again to make it fall on your hand.";
        sphereEnabled = true;
        GameObject sphere = GameObject.Find("Sphere1");
        sphere.transform.position = new UnityEngine.Vector3(-1.51647103f, 1.30900002f, 2.29871488f);

    }


    IEnumerator DisableSphereCoroutine(GameObject sphere)
    {
        mText.text = "This ball will be on your hand for 30 seconds too, and then it will disappear.";
        yield return new WaitForSeconds(30);
        /**/
        sphere.GetComponent<Rigidbody>().useGravity = false;
        sphere.transform.position = new UnityEngine.Vector3(-1.51647103f, 11.30900002f, 2.29871488f);

        mText.text = "Thank you, the study is over please answer the questions provided by us about this scene";
        yield return new WaitForSeconds(40);
        Destroy(sphere);

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

            AmplitudeModulationControlPoint point = new AmplitudeModulationControlPoint(position, intensity, 200.0f);
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

    void OnDisable()
    {
        print("disable called");
        _emitter.stop();
        
    }

    // Ensure the emitter is immediately disposed when destroyed
    void OnDestroy()
    {
        print("destroy called");
        _emitter.Dispose();
        _emitter = null;
    }

}
