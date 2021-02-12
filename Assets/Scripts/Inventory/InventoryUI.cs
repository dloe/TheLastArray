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

    public GameObject pistol, rifle, melee, medkit;

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
        //selector.parent = slotParent;
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(!UI.Instance.PausedStatus)
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
       

        
    }

    void SetIndex(int index)
    {
        

        if(index > 3)
        {
            selectedItemIndex = 0;
        }
        else if(index < 0)
        {
            selectedItemIndex = 3;
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
                slotList[index].GetComponentInChildren<Text>().text = inventory.ItemAtIndex(index).itemType.ToString();
                
            }
            else
            {
                slotList[index].GetComponentInChildren<Text>().text = "none";
            }
        }
        SelectSlot(selectedItemIndex);
        inventory.Equip(selectedItemIndex);
    }

    public void SpawnItem(Item item)
    {
        Vector3 dropPos = new Vector3(player.transform.position.x, 1, player.transform.position.z);
        switch (item.itemType)
        {
            case Item.ItemType.MeleeWeapon:
                Instantiate(melee, dropPos, melee.transform.rotation);
                break;
            case Item.ItemType.Pistol:
                Instantiate(pistol, dropPos, melee.transform.rotation);
                break;
            case Item.ItemType.Rifle:
                Instantiate(rifle, dropPos, melee.transform.rotation);
                break;
            case Item.ItemType.MedKit:
                Instantiate(medkit, dropPos, melee.transform.rotation);
                break;
            default:
                break;
        }
        
    }

    private void SelectSlot(int slotIndex)
    {
        for(int index = 0; index < slotList.Count; index ++)
        {
            if(slotIndex == index)
            {
                slotList[index].GetComponent<Image>().color = Color.blue;
            }
            else
            {
                slotList[index].GetComponent<Image>().color = Color.white;
            }
        }

    }
}
