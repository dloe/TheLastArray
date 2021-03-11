using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    public Transform slotParent;
    //public Transform selector;
    private Player player;
    public Inventory inventory;
    public int selectedItemIndex = 0;
    public List<Transform> slotList = new List<Transform>();

    public GameObject emptyWorldItem;
    public GameObject slotPrefab;
    public Text equipedItemLabelText;
    public Text equipedWeaponAmmoText;
    public Text equipedWeaponReservesText;
    public Text equipedWeaponDashText;
    public Text currentAmmoName;

    private readonly string _zoomAxis = "Mouse ScrollWheel";

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
        player = Player.Instance;
        inventory = player.inventory;
        foreach(Transform slot in slotParent)
        {
            slotList.Add(slot);
        }

        ResetSlots();

        //selector.parent = slotParent;
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(!UI.Instance.PausedStatus && (inventory.selectedItem == null || !inventory.selectedItem.itemData.reloading) && (!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy))
        {
            if (Input.GetAxis(_zoomAxis) < 0)
            {
                SetIndex(selectedItemIndex + 1);
            }
            else if (Input.GetAxis(_zoomAxis) > 0)
            {
                SetIndex(selectedItemIndex - 1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetIndex(3);
            }
        }
       
        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    AddSlot();
        //}
        //else if(Input.GetKeyDown(KeyCode.O))
        //{
        //    Debug.Log("yesir");
        //    ResetSlots();
        //}
        
    }

    void SetIndex(int index)
    {
        

        if(index > slotList.Count-1)
        {
            selectedItemIndex = 0;
        }
        else if(index < 0)
        {
            selectedItemIndex = slotList.Count - 1;
        }
        else
        {
            selectedItemIndex = index;
        }

        
        RefreshUI();

        
    }

    public void RefreshUI()
    {
        for (int index = 0; index < slotList.Count; index++)
        {
            if(inventory.ItemAtIndex(index) != null)
            {
                slotList[index].GetComponentInChildren<Text>().text = inventory.ItemAtIndex(index).itemData.itemName;
                if(inventory.ItemAtIndex(index).itemData.itemSprite != null)
                {
                    slotList[index].GetChild(0).gameObject.SetActive(true);
                    slotList[index].GetChild(0).GetComponent<Image>().sprite = inventory.ItemAtIndex(index).itemData.itemSprite;
                }
                else
                { 
                    slotList[index].GetChild(0).gameObject.SetActive(false); 
                }
                
            }
            else
            {
                slotList[index].GetComponentInChildren<Text>().text = "none";
                slotList[index].GetChild(0).GetComponent<Image>().sprite = null;
                slotList[index].GetChild(0).gameObject.SetActive(false);
            }
        }
        SelectSlot(selectedItemIndex);
        inventory.Equip(selectedItemIndex);

        


        //DYLAN WAS HERE
        //for currently equiped item text
        if (inventory.selectedItem != null)
        {
            equipedItemLabelText.text = inventory.selectedItem.itemData.itemName.ToString();
            switch (inventory.selectedItem.itemData.itemType)
            {
                case ItemType.MeleeWeapon:
                    //equipedItemLabelText.text = inventory.selectedItem.itemType.ToString();
                    equipedWeaponAmmoText.text = "";
                    equipedWeaponDashText.gameObject.SetActive(false);
                    equipedWeaponReservesText.gameObject.SetActive(true);
                    currentAmmoName.gameObject.SetActive(true);
                    if(inventory.selectedItem.itemData.hasDurability)
                    {
                        equipedWeaponReservesText.text = inventory.selectedItem.itemData.durability.ToString();
                    }
                    else
                    {
                        equipedWeaponReservesText.text = "∞";
                    }
                    currentAmmoName.text = "Durability Left";
                    
                    Player.Instance.SetMeleeVisualActive(true);
                    
                    break;
                case ItemType.Pistol:
                    //equipedItemLabelText.text = inventory.selectedItem.itemType.ToString();
                    equipedWeaponDashText.gameObject.SetActive(true);

                    currentAmmoName.gameObject.SetActive(true);
                    equipedWeaponReservesText.gameObject.SetActive(true);
                    equipedWeaponAmmoText.text = inventory.selectedItem.itemData.loadedAmmo.ToString();
                    equipedWeaponReservesText.text = Player.Instance.currentLightAmmo.ToString();
                    currentAmmoName.text = inventory.selectedItem.itemData.ammoType.ToString();

                    Player.Instance.SetMeleeVisualActive(false);
                    break;
                case ItemType.Rifle:
                    //equipedItemLabelText.text = inventory.selectedItem.itemType.ToString();
                    equipedWeaponDashText.gameObject.SetActive(true);
                    currentAmmoName.gameObject.SetActive(true);
                    equipedWeaponReservesText.gameObject.SetActive(true);

                    equipedWeaponAmmoText.text = inventory.selectedItem.itemData.loadedAmmo.ToString();
                    equipedWeaponReservesText.text = Player.Instance.currentHeavyAmmo.ToString();
                    currentAmmoName.text = inventory.selectedItem.itemData.ammoType.ToString();

                    Player.Instance.SetMeleeVisualActive(false);

                    break;
                case ItemType.Heal:
                    equipedWeaponAmmoText.text = "";
                   // equipedWeaponReservesText.text = "";
                    equipedWeaponDashText.gameObject.SetActive(false);
                    Player.Instance.SetMeleeVisualActive(false);

                    equipedWeaponReservesText.gameObject.SetActive(true);
                    currentAmmoName.gameObject.SetActive(true);

                    currentAmmoName.text = "Heals";
                    equipedWeaponReservesText.text = inventory.selectedItem.itemData.amountToHeal.ToString();

                    break;
                default:
                    break;
            }
        }
        else
        {
            equipedItemLabelText.text = "None";
            equipedWeaponAmmoText.text = "";
            equipedWeaponReservesText.text = "";
            equipedWeaponDashText.gameObject.SetActive(false);
            currentAmmoName.gameObject.SetActive(false);
            Player.Instance.SetMeleeVisualActive(false);
        }
    }

    public void SpawnItem(Item item)
    {
        Vector3 dropPos = new Vector3(player.transform.position.x, 0.5f, player.transform.position.z);

        WorldItem worldItem = Instantiate(emptyWorldItem, dropPos, Player.Instance.playerHolderTransform.rotation).GetComponent<WorldItem>();

        
        worldItem.worldItemData = item.itemData;
        
    }

    public void AddSlot()
    {
        Transform slot = Instantiate(slotPrefab, slotParent).transform;
        slotList.Add(slot);
    }

    public void ResetSlots()
    {
        while (inventory.numInvSlots > slotList.Count)
        {
            AddSlot();
        }

        while (slotList.Count > inventory.numInvSlots)
        {
            //Debug.Log(slotList.Count);
            Destroy(slotList[slotList.Count - 1].gameObject);
            slotList[slotList.Count - 1] = null;
            slotList.RemoveAt(slotList.Count - 1);
            //Debug.Log(slotList.Count);
        }
    }

    private void SelectSlot(int slotIndex)
    {
        for(int index = 0; index < slotList.Count; index ++)
        {
            if(slotIndex == index)
            {
                slotList[index].GetComponentInChildren<Text>().color = Color.blue;
            }
            else
            {
                slotList[index].GetComponentInChildren<Text>().color = Color.white;
            }
        }

    }
}
