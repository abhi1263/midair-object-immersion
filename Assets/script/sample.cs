using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sample : MonoBehaviour

{
    public GameObject table;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        table.transform.position = this.transform.position;
        GameObject.Find("LoPoly Rigged Hand Left");

    }
}
