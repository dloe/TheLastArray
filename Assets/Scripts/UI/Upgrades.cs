using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Upgrades : MonoBehaviour
{
    public enum StatType
    {
        health,
        dmgResist,
        speed

    }
    public static Upgrades Instance;
    Player player;
    public GameObject upgradeMenu;

    public Text skillPointText, maxHealthText, dmgResistanceText, speedText;
    public Dropdown scrapDropDown;
    public Dropdown clothDropDown;
    public Dropdown medsDropDown;
    public Button resourceExchangeButton, maxHealthExchangeButton, dmgResistExchangeButton, speedExchangeButton;

    #region Stat Properties
    public int SkillPoints
    {
        get
        {
            return player.skillPoints;
        }
        set
        {
            player.skillPoints = value;
            skillPointText.text = player.skillPoints.ToString();
            UpdateExchangeButton();
        }
    }

    public int MaxHealth
    {
        get
        {
            return player.maxHealth;
        }
        set
        {
            player.maxHealth = value;
            maxHealthText.text = player.maxHealth.ToString();
            player.Health = player.Health;
        }
    }

    public int DmgResistance
    {
        get
        {
            return player.dmgResist;
        }
        set
        {
            player.dmgResist = value;
            dmgResistanceText.text = player.dmgResist.ToString();
        }
    }

    public float Speed
    {
        get
        {
            return player.speedStat;
        }
        set
        {
            player.speedStat = value;
            speedText.text = player.speedStat.ToString("#0.0");
        }
    }
    #endregion

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


    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && !UI.Instance.PausedStatus && (!CraftingTable.Instance || !CraftingTable.Instance.Menu.activeInHierarchy) && !Player.Instance.endScreen.activeInHierarchy)
        {
            ToggleUpgrades();
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            upgradeMenu.SetActive(false);
        }
    }

    public void ToggleUpgrades()
    {
        if(upgradeMenu.activeInHierarchy)
        {
            upgradeMenu.SetActive(false);
        }
        else
        {
            SkillPoints = SkillPoints;
            MaxHealth = MaxHealth;
            DmgResistance = DmgResistance;
            Speed = Speed;
            UpdateDropDowns(player.ScrapCount, player.ClothCount, player.MedsCount);
            
            upgradeMenu.SetActive(true);
            UpdateExchangeButton();
        }
    }

    public void UpdateDropDowns(int scrap, int cloth, int meds)
    {
        scrapDropDown.ClearOptions();
        clothDropDown.ClearOptions();
        medsDropDown.ClearOptions();
        List<string> optionNums = new List<string>();
        if(player.ScrapCount != 0)
        {
            for (int index = 0; index < scrap + 1 && index < 11; index++)
            {
                optionNums.Add(index.ToString());
            }
            scrapDropDown.AddOptions(optionNums);
        }
        else
        {
            optionNums.Add("0");
            scrapDropDown.AddOptions(optionNums);
            scrapDropDown.value = 0;
        }

        optionNums.Clear();

        if (player.ClothCount != 0)
        {
            
            for (int index = 0; index < cloth +1 && index < 11 ; index++)
            {
                optionNums.Add(index.ToString());
            }
            clothDropDown.AddOptions(optionNums);
        }
        else
        {
            optionNums.Add("0");
            clothDropDown.AddOptions(optionNums);
            clothDropDown.value = 0;
        }

        optionNums.Clear();

        if (player.MedsCount != 0)
        {
            
            for (int index = 0; index < meds + 1 && index < 11; index++)
            {
                optionNums.Add(index.ToString());
            }
            medsDropDown.AddOptions(optionNums);
            //Debug.Log(medsDropDown.options);
        }
        else
        {
            optionNums.Add("0");
            medsDropDown.AddOptions(optionNums);
            medsDropDown.value = 0;
        }
    }

    public void OnDropDownChoice(Dropdown dropDown)
    {
        if(scrapDropDown.value == 10)
        {
            clothDropDown.value = 0;
            medsDropDown.value = 0;
        }
        else if(clothDropDown.value == 10)
        {
            scrapDropDown.value = 0;
            medsDropDown.value = 0;
        }
        else if(medsDropDown.value == 10)
        {
            scrapDropDown.value = 0;
            clothDropDown.value = 0;
        }
        else if((scrapDropDown.value + clothDropDown.value + medsDropDown.value) > 10)
        {
            dropDown.value -= (scrapDropDown.value + clothDropDown.value + medsDropDown.value) - 10;
        }

        UpdateExchangeButton();
    }

    public void UpdateExchangeButton()
    {
        if (SceneManager.GetActiveScene().name == player.baseData.trainSceneName)
        {
            if ((scrapDropDown.value + clothDropDown.value + medsDropDown.value) == 10)
            {
                resourceExchangeButton.interactable = true;
            }
            else
            {
                resourceExchangeButton.interactable = false;
            }

            if (SkillPoints >= 1)
            {
                maxHealthExchangeButton.interactable = true;
                speedExchangeButton.interactable = true;
                dmgResistExchangeButton.interactable = true;
            }
            else
            {
                maxHealthExchangeButton.interactable = false;
                speedExchangeButton.interactable = false;
                dmgResistExchangeButton.interactable = false;
            }
        }
        else
        {
            resourceExchangeButton.interactable = false;
            maxHealthExchangeButton.interactable = false;
            speedExchangeButton.interactable = false;
            dmgResistExchangeButton.interactable = false;
            scrapDropDown.interactable = false;
            medsDropDown.interactable = false;
            clothDropDown.interactable = false;
        }
        
    }

    public void ExhangeForSkillPoint()
    {
        SkillPoints++;
        player.ScrapCount -= scrapDropDown.value;
        player.ClothCount -= clothDropDown.value;
        player.MedsCount -= medsDropDown.value;

        UpdateDropDowns(player.ScrapCount, player.ClothCount, player.MedsCount);
        UpdateExchangeButton();
    }

    public void ExchangeForStat(Button button)
    {
        if(button.name == maxHealthExchangeButton.name)
        {
            MaxHealth += 5;
            player.Health += 5;
        }
        else if(button.name == speedExchangeButton.name)
        {
            Speed += .3f;
        }
        else if (button.name == dmgResistExchangeButton.name)
        {
            DmgResistance += 1;
        }
        else
        {
            Debug.LogError("Incorrect Button Input into Function");
        }

        SkillPoints--;
    }
}
