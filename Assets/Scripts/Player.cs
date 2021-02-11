using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    [Header("Prefabs for Bullet")]
    public GameObject pistolBulletPrefab;
    public GameObject rifleBulletPrefab;
    public Inventory inventory;
    
    //Item that Player is currently able to grab
    [Header("Current Grabbable Item")]
    public WorldItem itemToGrab;

    public float moveSpeed = 5f;

    [Header("Used to adjust look Direction to better align to mouse")]
    public float xLookOffset = 3f;
    public float zLookOffset = 3f;

    
    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            if(value > maxHealth)
            {
                health = maxHealth;
            }
            else if(value < 0)
            {
                health = 0;
            }
            else
            {
                health = value;
            }
            healthText.text = health.ToString();
        }
    }
    private int health;
    [Header("Health Variables")]
    public int maxHealth = 10;
    public int medKitHealAmount = 6;
    public Text healthText;


    //transform of the player
    Transform _mainTransform;

    Vector3 moveDir;
    Vector3 lookDir;
    Plane rayPlane = new Plane(Vector3.up, 0);


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
        Health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        doMovement();
        mouseLook();
        Debug.DrawRay(_mainTransform.position, lookDir, Color.green);

        //if there is a grabbable item and the inventory is not full, then E picks up item
        if(Input.GetKeyDown(KeyCode.E) && itemToGrab && !inventory.IsFull())
        {
            inventory.AddItem(new Item { itemType = itemToGrab.itemType });
            Destroy(itemToGrab.gameObject);
            itemToGrab = null;
        }

        //uses currently selected item
        if(Input.GetMouseButtonDown(0) && inventory.selectedItem != null)
        {
            switch (inventory.selectedItem.itemType)
            {
                case Item.ItemType.MeleeWeapon:
                    meleeAttack();
                    break;
                case Item.ItemType.Pistol:
                case Item.ItemType.Rifle:
                    rangedAttack(inventory.selectedItem.itemType);
                    break;
                case Item.ItemType.MedKit:
                    if (Health < maxHealth)
                    {
                        heal();
                        inventory.RemoveItem(inventory.selectedItem);
                    }
                    break;
                default:
                    break;
            }
            
        }

        //drops currently selected item on the ground at the player's feet
        if(Input.GetKeyDown(KeyCode.Q) && inventory.selectedItem != null && !itemToGrab)
        {
            inventory.DropItem();
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(1);
        }
        
    }

    /// <summary>
    /// Moves Player Based On WASD
    /// </summary>
    private void doMovement()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDir.Normalize();
        moveDir *= Time.deltaTime * moveSpeed;
        _mainTransform.Translate(moveDir, Space.World);

    }

    /// <summary>
    /// Makes Player Look in Direction of Mouse
    /// </summary>
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

    private void rangedAttack(Item.ItemType itemType)
    {
        if (itemType == Item.ItemType.Pistol)
        {
            Instantiate(pistolBulletPrefab, transform.position, transform.rotation);
        }
        else
        {
            Instantiate(rifleBulletPrefab, transform.position, transform.rotation);
        }
        Debug.Log("Fire Weapon: " + itemType);
    }

    private void meleeAttack()
    {
        Debug.Log("Melee Attack");
    }

    private void heal()
    {
        Health += medKitHealAmount;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
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
        if(other.tag == "Item" && itemToGrab && other.gameObject == itemToGrab.gameObject)
        {
            itemToGrab = null;
        }
    }
}
