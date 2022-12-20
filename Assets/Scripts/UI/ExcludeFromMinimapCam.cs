using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcludeFromMinimapCam : MonoBehaviour
{
    /// <summary>
    /// Exclude From Minimap Cam
    /// Dylan Loe
    /// 
    /// Last Updated: 4/27/21
    /// 
    /// Notes:
    ///  - Basic behavior on start for minimap
    /// </summary>
    private Camera cam;
    [Header("FOR NOW ON: Please enclude layers you dont want cam to see here instead of handling it in multiple areas of project")]
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
