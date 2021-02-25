using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject playerObject;
    public Vector3 offset;
    public float offsetLimitX = 1f;
    public float offsetLimitZ = 1f;
    public float horizontalNeutralZone = 6f;
    public float verticalNeutralZone = 4f;
    public float panSpeed = 5f;
    private Vector3 baseOffset;

    

    // Start is called before the first frame update
    void Start()
    {
        if (playerObject == null)
        {
            playerObject = Player.Instance.gameObject;
        }

        offset = transform.position - playerObject.transform.position;
        baseOffset = offset;

        
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = playerObject.transform.position + offset;

        if(!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy)
        {
            
            panCamera();
        }
        
        clampOffset();

    }

    private void panCamera()
    {


        float xModifier = Mathf.Abs(Input.mousePosition.x - getScrnFrac(true, 2f)) / getScrnFrac(true, 2f);
        float zModifier = Mathf.Abs(Input.mousePosition.y - getScrnFrac(false, 2f)) / getScrnFrac(false, 2f);


        if (Input.mousePosition.x < getScrnFrac(true, 2f) - getScrnFrac(true, horizontalNeutralZone))
        {
            offset.x -= xModifier * panSpeed * Time.deltaTime;

        }
        else if (Input.mousePosition.x > getScrnFrac(true, 2f) + getScrnFrac(true, horizontalNeutralZone))
        {
            offset.x += xModifier * panSpeed * Time.deltaTime;

        }
        else
        {
            //offset.x = Mathf.SmoothStep(offset.x, baseOffset.x, panSpeed * 3f * Time.deltaTime);
            offset.x = Mathf.Lerp(offset.x, baseOffset.x, panSpeed * 0.5f * Time.deltaTime);
        }

        if (Input.mousePosition.y < getScrnFrac(false, 2f) - getScrnFrac(false, verticalNeutralZone))
        {
            offset.z -= zModifier * panSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y > getScrnFrac(false, 2f) + getScrnFrac(false, verticalNeutralZone))
        {
            offset.z += zModifier * panSpeed * Time.deltaTime;
        }
        else
        {
            //offset.z = Mathf.SmoothStep(offset.z, baseOffset.z, panSpeed * 3f * Time.deltaTime);
            offset.z = Mathf.Lerp(offset.z, baseOffset.z, panSpeed * 0.4f * Time.deltaTime);
        }
    }

    private void clampOffset()
    {
        offset.x = Mathf.Clamp(offset.x, baseOffset.x - offsetLimitX, baseOffset.x + offsetLimitX);
        offset.z = Mathf.Clamp(offset.z, baseOffset.z - offsetLimitZ, baseOffset.z + offsetLimitZ);
    }

    /// <summary>
    /// Returns either width or height divided by amountToDivide
    /// </summary>
    /// <param name="widthOrHeight">True to get width, false to get height</param>
    /// <param name="amountToDivide">Number to divide the width or height by</param>
    /// <returns></returns>
    private float getScrnFrac(bool widthOrHeight, float amountToDivide)
    {
        float result;
        if(widthOrHeight)
        {
            result = Screen.currentResolution.width / amountToDivide;
        }
        else
        {
            result = Screen.currentResolution.height / amountToDivide;
        }

        return result;
    }

    private void OnDrawGizmos()
    {
        Rect rect = new Rect(getScrnFrac(true, 2f) - getScrnFrac(true,horizontalNeutralZone), getScrnFrac(false,2f) - getScrnFrac(false, verticalNeutralZone), 
            (getScrnFrac(true, 2f) + getScrnFrac(true, horizontalNeutralZone)) - (getScrnFrac(true, 2f) - getScrnFrac(true, horizontalNeutralZone)), 
            (getScrnFrac(false, 2f) + getScrnFrac(false, verticalNeutralZone)) - (getScrnFrac(false, 2f) - getScrnFrac(false, verticalNeutralZone)));
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.red);
        UnityEditor.Handles.EndGUI();
    }
}
