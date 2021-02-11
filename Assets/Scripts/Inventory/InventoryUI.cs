using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    public Player player;
    public Inventory inventory;
    public int selectedItemIndex = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis(_zoomAxis) > 0)
        {
            SetIndex(selectedItemIndex + 1);
        }
        else if(Input.GetAxis(_zoomAxis) < 0)
        {
            SetIndex(selectedItemIndex - 1);
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetIndex(0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
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

    }
}
