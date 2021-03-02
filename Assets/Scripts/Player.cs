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
    
    //inventory of the player
    public Inventory inventory;
    
    //Item that Player is currently able to grab
    [Header("Current Grabbable Item")]
    public WorldItem itemToGrab;
    [Header("Current Grabbable Resource")]
    public Resource resourceToGrab;
    [Header("Current Usable Crafting Table")]
    public CraftingTable craftingTable;

    public float moveSpeed = 5f;

    [Header("Used to adjust look Direction to better align to mouse")]
    public float xLookOffset = 3f;
    public float zLookOffset = 3f;

    [Header("Extents for Melee BoxCast")]
    public Vector3 meleeExtents = new Vector3();
    [Header("Melee Forward Detection Distance")]
    public float meleeDist = 1f;

    #region UI Variables
    public GameObject loseScreen;
    #endregion

    #region Health Variables
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
            if(health == 0)
            {
                loseScreen.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
    private int health;
    [Header("Health Variables")]
    public int maxHealth = 10;
    public int medKitHealAmount = 6;
    public Text healthText;
    #endregion

    #region Resource Variables
    public int ScrapCount
    {
        get
        {
            return scrapCount;
        }
        set
        {
            scrapCount = value;
            scrapText.text = scrapCount.ToString();
        }
    }
    private int scrapCount = 0;

    public int MedsCount
    {
        get
        {
            return medsCount;
        }
        set
        {
            medsCount = value;
            medsText.text = medsCount.ToString();
        }
    }
    private int medsCount = 0;

    public int ClothCount
    {
        get
        {
            return clothCount;
        }
        set
        {
            clothCount = value;
            clothText.text = clothCount.ToString();
        }
    }
    private int clothCount = 0;

    public Text scrapText, medsText, clothText;
    #endregion


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
        ScrapCount = scrapCount;
        MedsCount = medsCount;
        ClothCount = clothCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (!UI.Instance.PausedStatus)
        {
            doMovement();
            mouseLook();
            Debug.DrawRay(_mainTransform.position, lookDir, Color.green);

            //if there is a grabbable item and the inventory is not full, then E picks up item
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (itemToGrab && !inventory.IsFull())
                {
                    inventory.AddItem(new Item(itemToGrab.worldItemData));
                    Destroy(itemToGrab.gameObject);
                    itemToGrab = null;
                }
                else if (resourceToGrab)
                {
                    switch (resourceToGrab.resourceType)
                    {
                        case Resource.ResourceType.scrap:
                            ScrapCount += resourceToGrab.amountToAdd;
                            break;
                        case Resource.ResourceType.meds:
                            MedsCount += resourceToGrab.amountToAdd;
                            break;
                        case Resource.ResourceType.cloth:
                            ClothCount += resourceToGrab.amountToAdd;
                            break;
                        default:
                            break;
                    }
                    Debug.Log("Resource Picked Up: " + resourceToGrab.amountToAdd + " " + resourceToGrab.resourceType);
                    Destroy(resourceToGrab.gameObject);
                    resourceToGrab = null;
                }
                else if(craftingTable)
                {
                    craftingTable.ActivateMenu();
                }
            }

            //uses currently selected item
            if (Input.GetMouseButtonDown(0) && inventory.selectedItem != null)
            {
                switch (inventory.selectedItem.itemData.itemType)
                {
                    case ItemType.MeleeWeapon:
                        meleeAttack();
                        break;
                    case ItemType.Pistol:
                    case ItemType.Rifle:
                        rangedAttack(inventory.selectedItem.itemData.itemType);
                        break;
                    case ItemType.Heal:
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
            if (Input.GetKeyDown(KeyCode.Q) && inventory.selectedItem != null && !itemToGrab)
            {
                inventory.DropItem();
            }

            //for testing damage and healing
            if (Input.GetKeyDown(KeyCode.F))
            {
                TakeDamage(1);
            }

            
            
        }
        
    }

    /// <summary>
    /// Moves Player Based On WASD
    /// </summary>
    private void doMovement()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
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
        if (rayPlane.Raycast(ray, out dist) && Time.timeScale != 0)
        {
            //if(Input.mousePosition.x)
            lookDir = new Vector3(ray.GetPoint(dist - xLookOffset).x , _mainTransform.position.y, ray.GetPoint(dist - zLookOffset).z );
            lookDir -= _mainTransform.position;

            _mainTransform.rotation = Quaternion.Slerp(_mainTransform.rotation, Quaternion.LookRotation(lookDir), 0.15F);
            
           
        }
    }

    private void rangedAttack(ItemType itemType)
    {
        
        if (itemType == ItemType.Pistol)
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
        RaycastHit hit;
        if(Physics.BoxCast(_mainTransform.position, meleeExtents, _mainTransform.forward,out hit, _mainTransform.rotation, inventory.selectedItem.itemData.meleeRange))
        {
            if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "Enemy")
            {
                Debug.Log("Durability Before: " + inventory.selectedItem.itemData.durability);
                if(inventory.selectedItem.itemData.hasDurability)
                {
                    inventory.selectedItem.itemData.durability--;
                    Debug.Log("Durability After: " + inventory.selectedItem.itemData.durability);
                    if (inventory.selectedItem.itemData.durability <= 0)
                    {
                        if(inventory.selectedItem.itemData.name.Contains("Instance"))
                        {
                            Destroy(inventory.selectedItem.itemData);
                            inventory.RemoveItem(inventory.selectedItem);
                            
                        }
                        
                    }
                }

                Debug.Log("yep enemy hit");
            }
        }
        
    }

    private void heal()
    {
        Health += inventory.selectedItem.itemData.amountToHeal;
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
        else if(other.tag == "Resource")
        {
            resourceToGrab = other.GetComponent<Resource>();
        }
        else if(other.tag == "Crafting")
        {
            craftingTable = other.GetComponent<CraftingTable>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Item" && itemToGrab && other.gameObject == itemToGrab.gameObject)
        {
            itemToGrab = null;
        }
        else if(other.tag == "Resource" && resourceToGrab && other.gameObject == resourceToGrab.gameObject)
        {
            resourceToGrab = null;
        }
        else if(other.tag == "Crafting")
        {
            craftingTable.DeactivateMenu();
            craftingTable = null;
        }
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying && inventory.selectedItem != null)
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawCube(_mainTransform.position + _mainTransform.forward * inventory.selectedItem.itemData.meleeRange, meleeExtents);
        }
        
        
    }
}
