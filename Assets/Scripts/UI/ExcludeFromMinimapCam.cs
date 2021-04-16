using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcludeFromMinimapCam : MonoBehaviour
{
    private Camera cam;
    public LayerMask toExclude;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();

        cam.cullingMask = ~toExclude;
       // foreach (LayerMask layer in toExclude)
        //{
       //     cam.cullingMask += ~layer;
       // }
    }

}
