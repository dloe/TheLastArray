using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Player Script
    /// Jeremy Castada
    /// 
    /// Last Updated: 4/27/21
    /// 
    /// Notes:
    ///  - handles player crafting and player movement
    /// </summary>
    public static Player Instance;
    public float PlayerCamRot;

    int layerMask = ~(1 <<18);

    public PlayerData baseData;

    public GameObject[] meleeVisual;

    [Header("Prefabs for Bullet")]
    public GameObject pistolBulletPrefab;
    public GameObject rifleBulletPrefab;

    //inventory of the player
    public Inventory inventory = new Inventory();

    [Header("Activatable / Intractable To Use")]
    public List<Activatable> thingsToActivate = new List<Activatable>();
    public Activatable thingToActivate;
    //public CraftingTable craftTable;

    [Header("Player Stats")]
    public float speedStat = 5f;
    public int dmgResist;
    public int skillPoints = 0;
    public bool hasBackPack = false;
    public bool hasArmorPlate = false;

    //public int healthUpgradesLeft;
    //public int dmgResistUpgradesLeft;
    //public int speedUpgradesLeft;
    public bool[] levelsBeaten;

    [Header("Used to adjust look Direction to better align to mouse")]
    public float xLookOffset = 3f;
    public float zLookOffset = 3f;

    [Header("Extents for Melee BoxCast")]
    public Vector3 meleeExtents = new Vector3();
    [Header("Melee Forward Detection Distance")]
    public float meleeDist = 1f;
    public SpriteRenderer[] MeleeReticle;

    #region Audio
    [Space(20)]
    AudioSource _audioSource;
    [Header("Weapon Audio")]
    public AudioClip rifleFire_clip;
    public AudioClip rifleReload_clip;
    public AudioClip rifleEmpty_clip;
    public AudioClip pistolFire_clip;
    public AudioClip pistolEmpty_clip;
    public AudioClip pistolReload_clip;
    public AudioClip melee_clip;
    [Header("Damage Audio")]
    public AudioClip[] takeDamage_clip;
    public AudioClip[] armorDamage_clip;
    #endregion

    #region Ammo
    public int currentLightAmmo = 0;
    public int currentHeavyAmmo = 0;
    public int maxLightAmmo = 26;
    public int maxHeavyAmmo = 14;

    #endregion

    public LineRenderer laserLine;

    #region UI Variables
    public GameObject endScreen;
    public Text endScreenText;
    public Text levelText;
    public Image ArmorPlateImage;
    public GameObject stimMessage;
    public Image playerImage;
    public List<Sprite> playerSprites = new List<Sprite>(8);
    #endregion

    private int damageModifier = 0;
    private bool usingStimmy = false;

    #region Health Variables
    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            if (value > maxHealth)
            {
                health = maxHealth;
            }
            else if (value < 0)
            {
                health = 0;
            }
            else
            {
                health = value;
            }
            healthText.text = health + "/" + maxHealth;
            if (health == 0)
            {
                endScreen.SetActive(true);
                Time.timeScale = 0;
            }
            healthBar.fillAmount = health * 1f / maxHealth;
        }
    }
    private int health;
    [Header("Health Variables")]
    public int maxHealth = 10;
    public int medKitHealAmount = 6;
    public Text healthText;
    public Image healthBar;
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

    public Material damaged, norm;

    #region PlayerMovement
    //transform of the player
    Transform _mainTransform;
    public Transform playerHolderTransform;
    public Rigidbody rb;

    public Vector3 spawnPoint;

    Vector3 moveDir;
    Vector3 lookDir;
    Plane rayPlane = new Plane(Vector3.up, 0.5f);

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        rb = GetComponent<Rigidbody>();
        //Debug.Log("Player Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Player Start");
        
        _audioSource = GetComponent<AudioSource>();
        _mainTransform = transform;
        playerHolderTransform = transform.parent;
        if (SceneManager.GetActiveScene().name == baseData.levelOneName)
        {
            SetStatsToBase();
        }
        else
        {
            LoadPlayer();
        }

        //Updates the level text string to show which level is active
        levelText.text = SceneManager.GetActiveScene().name;

        if (hasArmorPlate)
        {
            ArmorPlateImage.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!UI.Instance.PausedStatus && (!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy) && !endScreen.activeInHierarchy)
        {
            mouseLook();
            doMovement();
            
            //if there is a grabbable item and the inventory is not full, then E picks up item
            if (Input.GetKeyDown(KeyCode.E) && !Upgrades.Instance.upgradeMenu.activeInHierarchy)
            {
                if (thingsToActivate.Count > 0)
                {
                    thingToActivate = thingsToActivate[0];
                    
                   
                    if(thingToActivate is WorldItem && inventory.IsFull())
                    {
                        thingsToActivate.Remove(thingToActivate);
                        thingsToActivate.Add(thingToActivate);
                    }
                    else if(!(thingToActivate is CraftingTable))
                    {
                        //Debug.Log(thingToActivate.name + "activated, removing from reachable activatables");
                        thingsToActivate.Remove(thingToActivate);
                    }
                    
                    
                    thingToActivate.Activate();
                    thingToActivate = null;

                    //if(thingToActivateTwo)
                    //{
                    //    thingToActivate = thingToActivateTwo;
                    //    thingToActivateTwo = null;
                    //}
                    //thingToActivate = null;
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
                    case ItemType.RangedWeapon:
                        rangedAttack();
                        break;
                    case ItemType.Heal:
                        if (Health < maxHealth)
                        {
                            heal();
                            inventory.RemoveItem(inventory.selectedItem);
                        }
                        break;
                    case ItemType.UnstableStim:
                        if (!usingStimmy)
                        {
                            StartCoroutine(UnstableStimmy());
                            inventory.RemoveItem(inventory.selectedItem);
                        }
                        break;
                    default:
                        break;
                }

            }

            //drops currently selected item on the ground at the player's feet
            if (Input.GetKeyDown(KeyCode.Q) && inventory.selectedItem != null && !inventory.selectedItem.itemData.reloading)
            {

                inventory.DropItem();
            }

            if (Input.GetKeyDown(KeyCode.R) && inventory.selectedItem != null && inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon && !inventory.selectedItem.itemData.reloading && inventory.selectedItem.itemData.loadedAmmo < inventory.selectedItem.itemData.magSize)
            {

                reload();
            }

#if UNITY_EDITOR
            //for testing damage and healing
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                TakeDamage(1, -1f, null);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Reseting Player Save...");
                inventory.Clear();
                SetStatsToBase();
                InventoryUI.Instance.ResetSlots();
                SavePlayer();
                InventoryUI.Instance.RefreshUI();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {


                Debug.Log("Trying To Save...");
                SavePlayer();

            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("Trying To Load...");
                LoadPlayer();
            }
            else if (Input.GetKeyDown(KeyCode.Comma) && inventory.selectedItem != null && inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon)
            {
                inventory.selectedItem.itemData.hasLaserSight = true;
                laserLine.gameObject.SetActive(true);
            }

#endif

        }

        //if(meleeVisual.activeInHierarchy)
        //{
        //    meleeVisual.transform.position = _mainTransform.position + (_mainTransform.forward * inventory.selectedItem.itemData.meleeRange);
        //}

    }

    /// <summary>
    /// Sets the Melee Visual to the Current Selected Item's meleeRange if true, hides it if false
    /// </summary>
    /// <param name="active"></param>
    public void SetMeleeVisualActive(bool active)
    {
        int meleeType = 0;
        if (active)
        {
            //set meleeVisual depending on type of melee
            if (Player.Instance.inventory.selectedItem.itemData.itemType == ItemType.MeleeWeapon && inventory.selectedItem.itemData.itemName == "Knife")
            {
                //use small melee reticle aka index 0
                meleeType = 0;
                //can set the reticle based on ItemData
                //meleeVisual[0].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = inventory.selectedItem.itemData.UIReticle;
                MeleeReticle[0].sprite = inventory.selectedItem.itemData.UIReticle;

            }
            else if (Player.Instance.inventory.selectedItem.itemData.itemType == ItemType.MeleeWeapon && inventory.selectedItem.itemData.itemName == "Metal Spear")
            {
                //use bigger melee reticle aka index 1
                meleeType = 1;
                //meleeVisual[1].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = inventory.selectedItem.itemData.UIReticle;
                MeleeReticle[1].sprite = inventory.selectedItem.itemData.UIReticle;
            }
            meleeVisual[meleeType].transform.position = _mainTransform.position + (_mainTransform.forward * inventory.selectedItem.itemData.meleeRange);
        }
        meleeVisual[meleeType].SetActive(active);
    }

    /// <summary>
    /// Moves Player Based On WASD
    /// </summary>
    private void doMovement()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moveDir.Normalize();
        moveDir = playerHolderTransform.TransformDirection(moveDir);
        moveDir *= Time.deltaTime * speedStat;

        //rb.MovePosition(transform.position + moveDir);

        if(transform.position.y < -5)
        {
            transform.position = spawnPoint;
        }

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
            lookDir = new Vector3(ray.GetPoint(dist - xLookOffset).x, _mainTransform.position.y, ray.GetPoint(dist - zLookOffset).z);
            
            //lookDir = new Vector3(ray.GetPoint(dist - xLookOffset).x , _mainTransform.position.y, ray.GetPoint(dist - zLookOffset).z );
            lookDir -= _mainTransform.position;
            Debug.DrawLine(_mainTransform.position, _mainTransform.position + lookDir);
            _mainTransform.rotation = Quaternion.Slerp(_mainTransform.rotation, Quaternion.LookRotation(lookDir), 35f * Time.deltaTime);


        }
        //Debug.Log(lookDir);

        Vector3 localLook = Quaternion.AngleAxis(-playerHolderTransform.rotation.eulerAngles.y, Vector3.up) * lookDir;
        int zFloor = Mathf.FloorToInt(Mathf.Abs(localLook.z));

        if (inventory.selectedItem != null && inventory.selectedItem.itemData.hasLaserSight)
        {
            Vector3 test = new Vector3(transform.localPosition.x, transform.localPosition.y - 1f, transform.localPosition.z);
            laserLine.SetPosition(0, test);
            
            laserLine.SetPosition(1, test + localLook);
        }

        int xFloor = Mathf.FloorToInt(Mathf.Abs(localLook.x));
        
        if (zFloor == 0)
        {
            if (localLook.x < 0)
            {
                playerImage.sprite = playerSprites[4];
            }
            else if(localLook.x > 0)
            {
                playerImage.sprite = playerSprites[5];
            }
        }
        else if (xFloor == 0)
        {
            if (localLook.z < 0)
            {
                playerImage.sprite = playerSprites[3];
            }
            else if (localLook.z > 0)
            {
                playerImage.sprite = playerSprites[0];
            }
        }
        else if(localLook.x < 0)
        {
            if (localLook.z < 0)
            {
                playerImage.sprite = playerSprites[1];
            }
            else if (localLook.z > 0)
            {
                playerImage.sprite = playerSprites[6];
            }
        }
        else if (localLook.x > 0)
        {
            if (localLook.z < 0)
            {
                playerImage.sprite = playerSprites[2];
            }
            else if (localLook.z > 0)
            {
                playerImage.sprite = playerSprites[7];
            }
        }
    }


    /// <summary>
    /// Performs Ranged Attack based on currently selected item
    /// </summary>
    private void rangedAttack()
    {
        if (inventory.selectedItem.itemData.canAttack && !inventory.selectedItem.itemData.reloading)
        {
            //Debug.Log(inventory.selectedItem.itemData.loadedAmmo);
            if (inventory.selectedItem.itemData.ammoType == AmmoType.LightAmmo)
            {
                if (inventory.selectedItem.itemData.loadedAmmo > 0)
                {
                    Bullet bullet;
                    bullet = Instantiate(pistolBulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
                    bullet.damageToDeal = inventory.selectedItem.itemData.damage + damageModifier;
                    bullet.isFireBullet = inventory.selectedItem.itemData.usingFireBullets;
                    StartCoroutine(inventory.selectedItem.itemData.CoolDown());

                    if(inventory.selectedItem.itemData.usingFireBullets)
                    {
                        inventory.selectedItem.itemData.fireLoadedAmmo--;
                    }
                    else
                    {
                        inventory.selectedItem.itemData.loadedAmmo--;
                    }

                    // Debug.Log("Fire Weapon: " + inventory.selectedItem.itemData.itemName);
                    WeaponFireAudio(4);
                }
                else
                {
                    WeaponFireAudio(6);
                    reload();
                }
            }
            else if (inventory.selectedItem.itemData.ammoType == AmmoType.HeavyAmmo)
            {
                if (inventory.selectedItem.itemData.loadedAmmo > 0 )
                {
                    Bullet bullet;
                    bullet = Instantiate(rifleBulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
                    bullet.damageToDeal = inventory.selectedItem.itemData.damage + damageModifier;
                    bullet.isFireBullet = inventory.selectedItem.itemData.usingFireBullets;
                    StartCoroutine(inventory.selectedItem.itemData.CoolDown());
                    if (inventory.selectedItem.itemData.usingFireBullets)
                    {
                        inventory.selectedItem.itemData.fireLoadedAmmo--;
                    }
                    else
                    {
                        inventory.selectedItem.itemData.loadedAmmo--;
                    }
                    
                    // Debug.Log("Fire Weapon: " + inventory.selectedItem.itemData.itemName);
                    WeaponFireAudio(1);
                }
                else
                {
                    WeaponFireAudio(3);
                    reload();
                }
            }
            InventoryUI.Instance.RefreshUI();
        }
    }

    /// <summary>
    /// Reloads the currently selected ranged weapon
    /// </summary>
    private void reload()
    {
        if (inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon)
        {
            switch (inventory.selectedItem.itemData.ammoType)
            {
                case AmmoType.LightAmmo:
                    if (currentLightAmmo != 0)
                    {
                        WeaponFireAudio(5);
                        if (currentLightAmmo < (inventory.selectedItem.itemData.magSize - inventory.selectedItem.itemData.loadedAmmo))
                        {
                            StartCoroutine(inventory.selectedItem.itemData.Reload(currentLightAmmo));
                        }
                        else
                        {
                            StartCoroutine(inventory.selectedItem.itemData.Reload(inventory.selectedItem.itemData.magSize - inventory.selectedItem.itemData.loadedAmmo));
                        }
                    }
                    else
                    {
                        Debug.Log("Can't reload, no light ammo");
                    }

                    break;
                case AmmoType.HeavyAmmo:
                    if (currentHeavyAmmo != 0)
                    {
                        WeaponFireAudio(2);
                        if (currentHeavyAmmo < (inventory.selectedItem.itemData.magSize - inventory.selectedItem.itemData.loadedAmmo))
                        {
                            StartCoroutine(inventory.selectedItem.itemData.Reload(currentHeavyAmmo));
                        }
                        else
                        {
                            StartCoroutine(inventory.selectedItem.itemData.Reload(inventory.selectedItem.itemData.magSize - inventory.selectedItem.itemData.loadedAmmo));
                        }
                    }
                    else
                    {
                        Debug.Log("Can't reload, no heavy ammo");
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Performs Melee Attack based on currently selected item
    /// </summary>
    private void meleeAttack()
    {
        RaycastHit hit;
        if (inventory.selectedItem.itemData.canAttack)
        {
            WeaponFireAudio(7);
           // Debug.Log("check");
            //Debug.Log("Melee Attack");
            if (Physics.BoxCast(_mainTransform.position, meleeExtents, _mainTransform.forward, out hit, _mainTransform.rotation, inventory.selectedItem.itemData.meleeRange, layerMask))
            {
                StartCoroutine(inventory.selectedItem.itemData.CoolDown());

                if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "Enemy" && hit.transform.TryGetComponent<BossEnemy>(out BossEnemy mBossEnemy))
                {
                    mBossEnemy.TakeDamage(inventory.selectedItem.itemData.damage + damageModifier);
                    //Debug.Log("yep boss hit");
                }
                else if (LayerMask.LayerToName(hit.transform.gameObject.layer) == "Enemy")
                {
                    //Debug.Log("Durability Before: " + inventory.selectedItem.itemData.durability);

                    hit.transform.GetComponent<BaseEnemy>().TakeDamage(inventory.selectedItem.itemData.damage + damageModifier, _mainTransform, inventory.selectedItem.itemData.meleeKnockback);

                    if (inventory.selectedItem.itemData.hasDurability)
                    {
                        inventory.selectedItem.itemData.durability--;
                        Debug.Log("Durability After: " + inventory.selectedItem.itemData.durability);
                        if (inventory.selectedItem.itemData.durability <= 0)
                        {
                            if (inventory.selectedItem.itemData.name.Contains("Instance"))
                            {
                                Destroy(inventory.selectedItem.itemData);
                            }
                            inventory.RemoveItem(inventory.selectedItem);
                        }
                        InventoryUI.Instance.RefreshUI();
                    }
                    //Debug.Log("yep enemy hit");
                }
            }
        }
    }

    /// <summary>
    /// Heals Player Based On Amount to Heal from Selected Item
    /// </summary>
    private void heal()
    {
        Health += inventory.selectedItem.itemData.amountToHeal;
    }

    /// <summary>
    /// Damages Player Based On Amount to Heal from Selected Item
    /// </summary>
    public void TakeDamage(int damage, float knockBack, Transform instigator)
    {
        TakeDamageAudio(hasArmorPlate);
        if (hasArmorPlate)
        {
            hasArmorPlate = false;
            ArmorPlateImage.gameObject.SetActive(false);
        }
        else
        {
            double levelMod;
            if(levelText.text == baseData.levelOneName)
            {
                levelMod = 3.0;
            }
            else if (levelText.text == baseData.levelTwoName)
            {
                levelMod = 2.0;
            }
            else if (levelText.text == baseData.levelThreeName)
            {
                levelMod = 3.0;
            }
            else if (levelText.text == baseData.levelFourName)
            {
                levelMod = 3.0;
            }
            else
            {
                levelMod = 1.0;
            }

            if(instigator != null)
            {
                //apply knockback 
                float kbMultiplier = 0.0f;
                if(knockBack > 0)
                {
                    kbMultiplier = knockBack;
                }

                gameObject.GetComponent<Rigidbody>().AddForce(instigator.forward * kbMultiplier);
            }

            //Debug.Log("Damage Before Resistance: " + damage);
            double resistance = dmgResist / (2 * levelMod + dmgResist);
            //Debug.Log("Damage After Resistance: " + (damage - (int)(damage * resistance)));
            Health -= (damage - (int)(damage * resistance));
            StartCoroutine(Damaged());
        }
    }

    bool ow = false;
    public void poisned(int damage)
    {
        if(!ow)
        {
            ow = true;
            StartCoroutine(pois(damage));
        }
    }
   
    IEnumerator pois(int damage)
    {
        TakeDamage(damage, -1.0f, null);
        yield return new WaitForSeconds(2);//shawn here 
        ow = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Activatable>(out Activatable thing))
        {
            thingsToActivate.Add(thing);
            //Debug.Log(thing.name + " trigger entered, added to reachable activatables");
        }

        //if (other.TryGetComponent<Activatable>(out Activatable thing))
        //{
        //    if(thingToActivate)
        //    {
        //        if (thing != thingToActivate)
        //        {
        //            
        //            thingToActivateTwo = thing;
        //        }
        //        
        //    }
        //    else
        //    {
        //        
        //        
        //        
        //        thingToActivate = thing;
        //
        //    }
        //    
        //    
        //    
        //}

        if (other.TryGetComponent<PlayerDetection>(out PlayerDetection tile))
        {
            if (!tile.hasBeenVisited)
            {
                tile.hasBeenVisited = true;
                tile.fogofwar.layer = 22;

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
       // if (thingToActivate && other.gameObject == thingToActivate.gameObject)
       // {
       //     thingToActivate = null;
       //     if(thingToActivateTwo)
       //     {
       //         thingToActivate = thingToActivateTwo;
       //         thingToActivateTwo = null;
       //     }
       // }
       //
       // if(thingToActivateTwo && other.gameObject == thingToActivateTwo.gameObject)
       // {
       //     thingToActivateTwo = null;
       // }


        if(other.TryGetComponent<Activatable>(out Activatable thing))
        {
            thingsToActivate.Remove(thing);
            //Debug.Log(thing.name + " trigger left, removed from reachable activatables");
        }
    }
    IEnumerator Damaged()
    {
        playerImage.color = Color.red;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.white;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.red;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.white;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.red;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.white;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.red;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.white;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.red;
        yield return new WaitForSeconds(.1f);
        playerImage.color = Color.white;
    }

    int originalMax;
    IEnumerator UnstableStimmy()
    {
        usingStimmy = true;
        stimMessage.SetActive(true);
         originalMax = maxHealth;
        if (Health == maxHealth)
        {
            Health -= inventory.selectedItem.itemData.healthDecrease;
        }

        maxHealth -= inventory.selectedItem.itemData.healthDecrease;
        Health -= 0;

        damageModifier = inventory.selectedItem.itemData.damageModifier;

        yield return new WaitForSeconds(5f);
        maxHealth = originalMax;
        damageModifier = 0;
        Health -= 0;
        usingStimmy = false;
        stimMessage.SetActive(false);
    }

    public void SetStatsToBase()
    {
        Debug.Log("Player: Setting stats to base.");
        maxHealth = baseData.maxHealth;
        Health = baseData.health;
        dmgResist = baseData.dmgResist;
        speedStat = baseData.speedStat;
        levelsBeaten = new bool[4];

        ScrapCount = baseData.scrap;
        ClothCount = baseData.cloth;
        MedsCount = baseData.meds;
        skillPoints = baseData.skillPoints;
        currentLightAmmo = 0;
        currentHeavyAmmo = 0;

        hasBackPack = false;
        hasArmorPlate = false;
        ArmorPlateImage.gameObject.SetActive(false);

        inventory.selectedItem = null;
        inventory.numInvSlots = 4;
        inventory.AddItemNoUI(baseData.initialItem);
    }

    #region Audio

    void TakeDamageAudio(bool armor)
    {
        if (!armor)
        {
            int aIndex = Random.Range(0, takeDamage_clip.Length);
            _audioSource.clip = takeDamage_clip[aIndex];
        }
        else
        {
            int aIndex = Random.Range(0, armorDamage_clip.Length);
            _audioSource.clip = armorDamage_clip[aIndex];
        }

        _audioSource.Play();
    }
    void WeaponFireAudio(int audio)
    {
        switch (audio)
        {
            case 1:
                _audioSource.clip = rifleFire_clip;
                break;
            case 2:
                _audioSource.clip = rifleReload_clip;
                break;
            case 3:
                _audioSource.clip = rifleEmpty_clip;
                break;
            case 4:
                _audioSource.clip = pistolFire_clip;
                break;
            case 5:
                _audioSource.clip = pistolReload_clip;
                break;
            case 6:
                _audioSource.clip = pistolEmpty_clip;
                break;
            case 7:
                _audioSource.clip = melee_clip;
                break;
            default:
                break;
        }
        _audioSource.Play();
    }

    #endregion

    #region Saving / Loading
    public void SavePlayer()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/player_save.dat");

        PlayerSave playerSave = new PlayerSave();
        if(usingStimmy)
        {
            maxHealth = originalMax;
            usingStimmy = false;
        }
        playerSave.maxHealth = maxHealth;
        playerSave.health = Health;
        playerSave.dmgResist = dmgResist;
        playerSave.speedStat = speedStat;
        playerSave.scrap = ScrapCount;
        playerSave.cloth = ClothCount;
        playerSave.meds = MedsCount;
        playerSave.skillPoints = skillPoints;
        playerSave.hasBackPack = hasBackPack;
        playerSave.hasArmorPlate = hasArmorPlate;
        playerSave.lightAmmo = currentLightAmmo;
        playerSave.heavyAmmo = currentHeavyAmmo;
        playerSave.levelsBeaten = levelsBeaten;

        playerSave.invJsonList = inventory.SaveToJsonList();
        playerSave.numInvSlots = InventoryUI.Instance.slotList.Count;

        bf.Serialize(file, playerSave);
        file.Close();
    }

    public void LoadPlayer()
    {
        if (SaveExists())
        {
            //Debug.Log("save exists");
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
            hasBackPack = playerSave.hasBackPack;
            hasArmorPlate = playerSave.hasArmorPlate;

            currentLightAmmo = playerSave.lightAmmo;
            currentHeavyAmmo = playerSave.heavyAmmo;
            levelsBeaten = playerSave.levelsBeaten;

            inventory.numInvSlots = playerSave.numInvSlots;
            inventory.LoadFromJsonList(playerSave.invJsonList);

        }
        else
        {
            SetStatsToBase();
            Debug.LogWarning("The Save Data Could not be Found, so base Stats were loaded instead");
        }
    }

    public bool SaveExists()
    {
        return File.Exists(Application.persistentDataPath + "/player_save.dat");
    }
    #endregion

    //private void OnDrawGizmos()
    //{
    //    if (Input.GetKey(KeyCode.Semicolon) && Application.isPlaying && inventory.selectedItem != null)
    //    {
    //        Gizmos.color = Color.red;
    //
    //        Gizmos.DrawCube(_mainTransform.position + _mainTransform.forward * inventory.selectedItem.itemData.meleeRange, meleeExtents);
    //    }
    //
    //
    //}
}

[System.Serializable]
public class PlayerSave
{
    public int maxHealth;
    public int health;
    public int dmgResist;
    public float speedStat;
    public int scrap, cloth, meds;
    public int skillPoints;
    public bool hasBackPack;
    public bool hasArmorPlate;
    public bool[] levelsBeaten;
    public int lightAmmo;
    public int heavyAmmo;

   // public int healthUpgradesLeft;
   // public int dmgResistUpgradesLeft;
   // public int speedUpgradesLeft;

    public List<string> invJsonList;
    public int numInvSlots;
}
