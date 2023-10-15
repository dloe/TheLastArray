using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Upgrades : MonoBehaviour
{
    /// <summary>
    /// Upgrades
    /// Jeremy Castada
    /// 
    /// Last Updated: 5/15/22
    /// 
    /// 
    /// </summary>
    public enum StatType
    {
        health,
        dmgResist,
        speed

    }
    public static Upgrades Instance;
    Player _player;
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
            return _player.skillPoints;
        }
        set
        {
            _player.skillPoints = value;
            skillPointText.text = _player.skillPoints.ToString();
            UpdateExchangeButton();
        }
    }

    public int MaxHealth
    {
        get
        {
            return _player.maxHealth;
        }
        set
        {
            _player.maxHealth = value;
            maxHealthText.text = _player.maxHealth.ToString();
            _player.Health = _player.Health;
        }
    }

    public int DmgResistance
    {
        get
        {
            return _player.dmgResist;
        }
        set
        {
            _player.dmgResist = value;
            dmgResistanceText.text = _player.dmgResist.ToString();
        }
    }

    public float Speed
    {
        get
        {
            return _player.speedStat;
        }
        set
        {
            _player.speedStat = value;
            speedText.text = _player.speedStat.ToString("#0.0");
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
        _player = Player.Instance;


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
            UpdateDropDowns(_player.ScrapCount, _player.ClothCount, _player.MedsCount);
            
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
        if(_player.ScrapCount != 0)
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

        if (_player.ClothCount != 0)
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

        if (_player.MedsCount != 0)
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
        if (SceneManager.GetActiveScene().name == _player.baseData.trainSceneName)
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
        _player.ScrapCount -= scrapDropDown.value;
        _player.ClothCount -= clothDropDown.value;
        _player.MedsCount -= medsDropDown.value;

        UpdateDropDowns(_player.ScrapCount, _player.ClothCount, _player.MedsCount);
        UpdateExchangeButton();
    }

    public void ExchangeForStat(Button button)
    {
        if(button.name == maxHealthExchangeButton.name)
        {
            MaxHealth += 5;
            _player.Health += 5;
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
