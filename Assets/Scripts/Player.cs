using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //transform of the player
    Transform _mainTransform;

    Vector3 moveDir;
    Vector3 lookDir;
    Plane rayPlane = new Plane(Vector3.up, 0);

    public List<GameObject> inventory;

    public float moveSpeed = 5f;
    public float xLookOffset = 3f;
    public float zLookOffset = 3f;


    

    // Start is called before the first frame update
    void Start()
    {
        _mainTransform = transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        doMovement();
        mouseLook();
        Debug.DrawRay(_mainTransform.position, lookDir, Color.green);
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
}
