using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerData baseData;
    
    [Header("Prefabs for Bullet")]
    public GameObject pistolBulletPrefab;
    public GameObject rifleBulletPrefab;
    
    //inventory of the player
    public Inventory inventory = new Inventory();
    
    //Item that Player is currently able to grab
    [Header("Current Grabbable Item")]
    public WorldItem itemToGrab;
    [Header("Current Grabbable Resource")]
    public Resource resourceToGrab;
    [Header("Current Usable Crafting Table")]
    public CraftingTable craftingTableToUse;
    public Activatable thingToActivate;

    public int speedStat = 5;
    public int dmgResist;
    public int skillPoints = 0;

    //public int healthUpgradesLeft;
    //public int dmgResistUpgradesLeft;
    //public int speedUpgradesLeft;

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
            healthText.text = health + "/" + maxHealth;
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
    public Transform playerHolderTransform;

    Vector3 moveDir;
    Vector3 lookDir;
    Plane rayPlane = new Plane(Vector3.up, 0);

    private bool doSetSpawn = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        Debug.Log("Player Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Start");
        _mainTransform = transform;
        playerHolderTransform = transform.parent;
        if(SceneManager.GetActiveScene().name == baseData.levelOneName )
        {
            SetStatsToBase();
        }
        else
        {
            LoadPlayer();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!UI.Instance.PausedStatus && (!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy))
        {
            doMovement();
            mouseLook();
            Debug.DrawRay(_mainTransform.position, lookDir, Color.green);

            //if there is a grabbable item and the inventory is not full, then E picks up item
            if (Input.GetKeyDown(KeyCode.E) && !Upgrades.Instance.upgradeMenu.activeInHierarchy)
            {
                if (itemToGrab && !inventory.IsFull())
                {
                    if (itemToGrab.worldItemData.itemType == ItemType.Gasoline)
                    {
                        Objectives.Instance.SendCompletedMessage(Condition.GetGasCan);
                    }
                    else
                    {
                        inventory.AddItem(new Item(itemToGrab.worldItemData));
                    }
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
                else if(thingToActivate)
                {
                    thingToActivate.Activate();
                    
                }
                else if(craftingTableToUse)
                {
                    craftingTableToUse.ActivateMenu();
                }
            }

            //uses currently selected item
            if (Input.GetMouseButtonDown(0) && inventory.selectedItem != null && !Upgrades.Instance.upgradeMenu.activeInHierarchy)
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

           //if (Input.GetKey(KeyCode.RightShift))
           //{
           //    if (Input.GetKeyDown(KeyCode.Equals))
           //    {
           //        Debug.Log("Trying To Save...");
           //        SavePlayer();
           //    }
           //}
           //else if (Input.GetKeyDown(KeyCode.Minus))
           //{
           //    Debug.Log("Trying To Load...");
           //    LoadPlayer();
           //}
            
            


        }
        
    }



    /// <summary>
    /// Moves Player Based On WASD
    /// </summary>
    private void doMovement()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moveDir.Normalize();
        moveDir *= Time.deltaTime * speedStat;

       // playerHolderTransform.TransformDirection(moveDir)

        _mainTransform.Translate(playerHolderTransform.TransformDirection(moveDir), Space.World);

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
            craftingTableToUse = other.GetComponent<CraftingTable>();
        }
        else if(other.tag == "Activatable")
        {
            Debug.Log("cock");
            thingToActivate = other.GetComponent<Activatable>();
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
        else if (other.tag == "Activatable" && thingToActivate && other.gameObject == thingToActivate.gameObject)
        {
            thingToActivate = null;
        }
        else if(other.tag == "Crafting")
        {
            craftingTableToUse.DeactivateMenu();
            craftingTableToUse = null;
        }
    }

    

    

    public void SetStatsToBase()
    {
        maxHealth = baseData.maxHealth;
        Health = baseData.health;
        dmgResist = baseData.dmgResist;

       
        
        ScrapCount = baseData.scrap;
        ClothCount = baseData.cloth;
        MedsCount = baseData.meds;
        skillPoints = baseData.skillPoints;
        inventory.selectedItem = null;
        inventory.AddItem(new Item(baseData.initialItem));
    }

    #region Saving / Loading
    public void SavePlayer()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/player_save.dat");

        PlayerSave playerSave = new PlayerSave();

        playerSave.maxHealth = maxHealth;
        playerSave.health = Health;
        playerSave.dmgResist = dmgResist;
        playerSave.speedStat = speedStat;
        playerSave.scrap = ScrapCount;
        playerSave.cloth = ClothCount;
        playerSave.meds = MedsCount;
        playerSave.skillPoints = skillPoints;
        //playerSave.healthUpgradesLeft = healthUpgradesLeft;
        //playerSave.dmgResistUpgradesLeft = dmgResistUpgradesLeft;
        //playerSave.speedUpgradesLeft = speedUpgradesLeft;

        playerSave.invJsonList = inventory.SaveToJsonList();

        bf.Serialize(file, playerSave);
        file.Close();
    }

    public void LoadPlayer()
    {
        if (SaveExists())
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player_save.dat", FileMode.Open);
            PlayerSave playerSave = (PlayerSave)bf.Deserialize(file);
            file.Close();

            maxHealth = playerSave.maxHealth;
            Health = playerSave.health;
            dmgResist = playerSave.dmgResist;
            speedStat = playerSave.speedStat;
            ScrapCount = playerSave.scrap;
            ClothCount = playerSave.cloth;
            MedsCount = playerSave.meds;
            skillPoints = playerSave.skillPoints;
            //healthUpgradesLeft = playerSave.healthUpgradesLeft;
            //dmgResistUpgradesLeft = playerSave.dmgResistUpgradesLeft;
            //speedUpgradesLeft = playerSave.speedUpgradesLeft;

            inventory.LoadFromJsonList(playerSave.invJsonList);
        }
    }

    public bool SaveExists()
    {
        return File.Exists(Application.persistentDataPath + "/player_save.dat");
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (Input.GetKey(KeyCode.Semicolon) && Application.isPlaying && inventory.selectedItem != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawCube(_mainTransform.position + _mainTransform.forward * inventory.selectedItem.itemData.meleeRange, meleeExtents);
        }


    }
}

[System.Serializable]
public class PlayerSave
{
    public int maxHealth;
    public int health;
    public int dmgResist;
    public int speedStat;
    public int scrap, cloth, meds;
    public int skillPoints;

   // public int healthUpgradesLeft;
   // public int dmgResistUpgradesLeft;
   // public int speedUpgradesLeft;

    public List<string> invJsonList;
}


