using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    //transform of the player
    Transform _mainTransform;

    Vector3 moveDir;
    Vector3 lookDir;
    Plane rayPlane = new Plane(Vector3.up, 0);

    public Inventory inventory;
    public WorldItem itemToGrab;

    public float moveSpeed = 5f;

    [Header("Used to adjust look Direction to better align to mouse")]
    public float xLookOffset = 3f;
    public float zLookOffset = 3f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _mainTransform = transform;
        inventory = new Inventory();
    }

    // Update is called once per frame
    void Update()
    {
        doMovement();
        mouseLook();
        Debug.DrawRay(_mainTransform.position, lookDir, Color.green);

        if(Input.GetKeyDown(KeyCode.E) && itemToGrab)
        {
            inventory.AddItem(new Item { itemType = itemToGrab.itemType });
            Destroy(itemToGrab.gameObject);
            itemToGrab = null;
        }
    }

    private void doMovement()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDir.Normalize();
        moveDir *= Time.deltaTime * moveSpeed;
        _mainTransform.Translate(moveDir, Space.World);




    }

    private void mouseLook()
    {
         

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        float dist;
        if (rayPlane.Raycast(ray, out dist))
        {
            //if(Input.mousePosition.x)
            lookDir = new Vector3(ray.GetPoint(dist - xLookOffset).x , _mainTransform.position.y, ray.GetPoint(dist - zLookOffset).z );
            lookDir -= _mainTransform.position;

            _mainTransform.rotation = Quaternion.Slerp(_mainTransform.rotation, Quaternion.LookRotation(lookDir), 0.15F);
            
           
        }
    }

    private void rangedAttack()
    {

    }

    private void meleeAttack()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item" )
        {
            itemToGrab = other.GetComponent<WorldItem>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Item")
        {
            itemToGrab = null;
        }
    }
}
