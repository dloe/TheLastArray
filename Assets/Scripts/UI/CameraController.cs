using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private GameObject playerObject;
    public Vector3 offset;
    public float offsetLimitX = 1f;
    public float offsetLimitZ = 1f;
    public float horizontalNeutralZone = 6f;
    public float verticalNeutralZone = 4f;
    public float panSpeed = 5f;
    private Vector3 baseOffset;

    public float originalOffsetLimitX;
    public float originalOffsetLimitZ;

    private float xModifier, zModifier;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        originalOffsetLimitX = offsetLimitX;
        originalOffsetLimitZ = offsetLimitZ;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerObject == null)
        {
            playerObject = Player.Instance.gameObject;
        }

        offset = transform.localPosition - Player.Instance.playerHolderTransform.TransformPoint(playerObject.transform.position);
        baseOffset = offset;

        //originalOffsetLimitX = offsetLimitX;
        //originalOffsetLimitZ = offsetLimitZ;

    }

    // Update is called once per frame
    void Update()
    {

        //transform.localPosition = playerObject.transform.localPosition + offset;

        //transform.Translate((playerObject.transform.position + offset).normalized);
        transform.position = playerObject.transform.position + Player.Instance.playerHolderTransform.TransformPoint(offset);

        if((!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy) && !Upgrades.Instance.upgradeMenu.activeInHierarchy )
        {
            
            panCamera();
        }
        
        clampOffset();


        //Dylan added this to exclude layer of secret room interior
        Camera cam = GetComponent<Camera>();
        cam.cullingMask = ~(1 << LayerMask.NameToLayer("HiddenSecretInterior"));

    }

    private void panCamera()
    {

        xModifier = Mathf.Abs(Input.mousePosition.x - getScrnFrac(true, 2f)) / getScrnFrac(true, 2f);
        zModifier = Mathf.Abs(Input.mousePosition.y - getScrnFrac(false, 2f)) / getScrnFrac(false, 2f);

        if (Input.mousePosition.x < getScrnFrac(true, 2f) - getScrnFrac(true, horizontalNeutralZone))
        {
            if(offset.x > baseOffset.x)
            {
                offset.x -= xModifier * panSpeed * 1.2f * Time.deltaTime;
            }
            else
            {
                offset.x -= xModifier * panSpeed * Time.deltaTime;
            }
            
            //offset.x = -Mathf.MoveTowards(offset.x,offsetLimitX - xModifier*-offsetLimitX , 0.1f);
        }
        else if (Input.mousePosition.x > getScrnFrac(true, 2f) + getScrnFrac(true, horizontalNeutralZone))
        {
            if (offset.x < baseOffset.x)
            {
                offset.x += xModifier * panSpeed * 1.2f * Time.deltaTime;
            }
            else
            {
                offset.x += xModifier * panSpeed * Time.deltaTime;
            }

        }
        else
        {
            //offset.x = Mathf.Lerp(offset.x, baseOffset.x, panSpeed * 0.5f * Time.deltaTime);
            offset.x = Mathf.MoveTowards(offset.x, baseOffset.x, panSpeed * Time.deltaTime);
        }

        if (Input.mousePosition.y < getScrnFrac(false, 2f) - getScrnFrac(false, verticalNeutralZone))
        {
            if(offset.z > baseOffset.z)
            {
                offset.z -= zModifier * panSpeed * 1.2f* Time.deltaTime;
            }
            else
            {
                offset.z -= zModifier * panSpeed * Time.deltaTime;
            }
            
        }
        else if (Input.mousePosition.y > getScrnFrac(false, 2f) + getScrnFrac(false, verticalNeutralZone))
        {
            if (offset.z < baseOffset.z)
            {
                offset.z += zModifier * panSpeed * 1.2f * Time.deltaTime;
            }
            else
            {
                offset.z += zModifier * panSpeed * Time.deltaTime;
            }
        }
        else
        {
            //offset.z = Mathf.Lerp(offset.z, baseOffset.z, panSpeed * 0.5f * Time.deltaTime);
            offset.z = Mathf.MoveTowards(offset.z, baseOffset.z, panSpeed * Time.deltaTime);
        }
    }

    private void clampOffset()
    {
        offset.x = Mathf.Clamp(offset.x, baseOffset.x - offsetLimitX, baseOffset.x + offsetLimitX);
        offset.z = Mathf.Clamp(offset.z, baseOffset.z - offsetLimitZ, baseOffset.z + offsetLimitZ);
    }

    public void ToggleBinocularMode(bool state)
    {
        if(state && offsetLimitX == originalOffsetLimitX && offsetLimitZ == originalOffsetLimitZ)
        {
            offsetLimitX += 6;
            offsetLimitZ += 1.7f;
        }
        else
        {
            offsetLimitX = originalOffsetLimitX;
            offsetLimitZ = originalOffsetLimitZ;
        }
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
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Rect rect = new Rect(getScrnFrac(true, 2f) - getScrnFrac(true,horizontalNeutralZone), getScrnFrac(false,2f) - getScrnFrac(false, verticalNeutralZone), 
            (getScrnFrac(true, 2f) + getScrnFrac(true, horizontalNeutralZone)) - (getScrnFrac(true, 2f) - getScrnFrac(true, horizontalNeutralZone)), 
            (getScrnFrac(false, 2f) + getScrnFrac(false, verticalNeutralZone)) - (getScrnFrac(false, 2f) - getScrnFrac(false, verticalNeutralZone)));
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.red);
        UnityEditor.Handles.EndGUI();
    }
#endif
}
