using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    //Doors act as connectors, each door must have a reference to each room its connected to otherwise, its a no longer a door
    //The room that initially spawns in doors is A, the second one is always B. Works out from center
    public GameObject roomA;
    public GameObject roomB;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
