using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset;
    public float offsetLimitX = 1f;
    public float offsetLimitZ = 1f;
    public float panSpeed = 5f;
    private Vector3 baseOffset;

    // Start is called before the first frame update
    void Start()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        offset = transform.position - player.transform.position;
        baseOffset = offset;
       

    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = player.transform.position + offset;
        panCamera();
        clampOffset();
        
    }

    private void panCamera()
    {
        

        float xModifier = Mathf.Abs(Input.mousePosition.x - Screen.currentResolution.width / 2) / (Screen.currentResolution.width /2);
        float zModifier = Mathf.Abs(Input.mousePosition.y - Screen.currentResolution.height / 2 ) / (Screen.currentResolution.height / 2);
        

        if (Input.mousePosition.x < (Screen.currentResolution.width / 2f) - (Screen.currentResolution.width / 6f))
        {
            offset.x -= xModifier * panSpeed * Time.deltaTime;
            
        }
        else if (Input.mousePosition.x > (Screen.currentResolution.width / 2f) + (Screen.currentResolution.width / 6f))
        {
            offset.x += xModifier * panSpeed * Time.deltaTime;
            
        }
        else
        {
            offset.x = Mathf.SmoothStep(offset.x, baseOffset.x, 8f * Time.deltaTime);
        }

        if (Input.mousePosition.y < Screen.currentResolution.height / 2f - Screen.currentResolution.height / 4f)
        {
            offset.z -= zModifier * panSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y > Screen.currentResolution.height / 2f + Screen.currentResolution.height / 4f)
        {
            offset.z += zModifier * panSpeed * Time.deltaTime;
        }
        else
        {
            offset.z = Mathf.SmoothStep(offset.z, baseOffset.z, panSpeed * 1.2f * Time.deltaTime);
        }
    }

    private void clampOffset()
    {
        offset.x = Mathf.Clamp(offset.x, baseOffset.x - offsetLimitX, baseOffset.x + offsetLimitX);
        offset.z = Mathf.Clamp(offset.z, baseOffset.z - offsetLimitZ, baseOffset.z + offsetLimitZ);
    }

    private void OnDrawGizmos()
    {
        Rect rect = new Rect((Screen.currentResolution.width / 2f - Screen.currentResolution.width / 6f), Screen.currentResolution.height / 2f - Screen.currentResolution.height / 4f, (Screen.currentResolution.width / 2f + Screen.currentResolution.width / 6f) - (Screen.currentResolution.width / 2f - Screen.currentResolution.width / 6f), (Screen.currentResolution.height / 2f + Screen.currentResolution.height / 4f) - (Screen.currentResolution.height / 2f - Screen.currentResolution.height / 4f));
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.red);
        UnityEditor.Handles.EndGUI();
    }
}
