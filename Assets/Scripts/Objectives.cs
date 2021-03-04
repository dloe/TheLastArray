using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum Condition
{
    KillEnemy,
    GetGasCan,
    FindGenerator
}
public class Objectives : MonoBehaviour
{
    public static Objectives Instance;
    public GameObject emptyWorldItem, generatorObject;
    public string killMessage, gasMessage, generatorMessage;
    public ItemData gasolineData;
    public Objective mainObjective;
    public int objectiveCount = 0;

    public Text objectiveText;

    public int enemyCount;

    [System.Serializable]
    public class Objective
    {
        public Condition condition;
        public string objectiveMessage = "blank";
        public ItemData itemData;
        public bool complete;

        public int numTimes = 0;

    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        enemyCount = FindObjectsOfType<BaseEnemy>().Length;
    }


    public void UpdateObjectiveText()
    {
        if(mainObjective.objectiveMessage == gasMessage || mainObjective.objectiveMessage == generatorMessage)
        {
            objectiveText.text = mainObjective.objectiveMessage + " " + (objectiveCount- mainObjective.numTimes) + "/" + objectiveCount;
        }
        else
        {
            objectiveText.text = mainObjective.objectiveMessage;
        }
    }
    

    public GameObject SetObjectiveRef(int objectiveInt, GameObject spot)
    {
        GameObject returnObj = null;
        WorldItem objItem = null;
        Generator objGenerator = null;
        Objective objective = new Objective();
        switch (objectiveInt)
        {
            case 1:
                objective.condition = Condition.KillEnemy;
                objective.objectiveMessage = killMessage;
                break;
            case 2:
                objective.condition = Condition.GetGasCan;
                objective.itemData = gasolineData;
                objItem = Instantiate(emptyWorldItem, spot.transform.position, emptyWorldItem.transform.rotation).GetComponent<WorldItem>();
                objItem.worldItemData = objective.itemData;
                returnObj = objItem.gameObject;
                objective.objectiveMessage = gasMessage;
                
                break;
            case 3:
                objective.condition = Condition.FindGenerator;
                objGenerator = Instantiate(generatorObject, spot.transform.position, generatorObject.transform.rotation).GetComponent<Generator>();
                returnObj = objGenerator.gameObject;
                objective.objectiveMessage = generatorMessage;
                break;
            default:
                break;
        }
        
        if(objective.objectiveMessage == mainObjective.objectiveMessage)
        {
            mainObjective.numTimes++;
            objectiveCount++;
        }
        else
        {
            objective.numTimes = 1;
            objectiveCount = 1;
            mainObjective = objective;
        }

        UpdateObjectiveText();

        return returnObj;
    }

    public bool CheckWinCondition()
    {
        
        
        bool objectivesComplete = true;
        
        if (!mainObjective.complete)
        {
            switch (mainObjective.condition)
            {
                case Condition.KillEnemy:
                    mainObjective.complete = enemyCount == 0;
                    break;
                case Condition.GetGasCan:
                    if(Player.Instance.inventory.Contains(mainObjective.itemData))
                    {
                        mainObjective.numTimes--;
                    }
                    mainObjective.complete = mainObjective.numTimes == 0;
                    break;
                case Condition.FindGenerator:
                    break;
                default:
                    break;
            }
        }
        if (!mainObjective.complete)
        {
            objectivesComplete = false;
            
        }
        
        return objectivesComplete;
    }



    public void SendCompletedMessage(Condition condition)
    {
        switch (condition)
        {
            case Condition.KillEnemy:
                Debug.Log("Enemy Killed");
                enemyCount--;
                mainObjective.complete = enemyCount <= 0;
                break;
            case Condition.GetGasCan:
                mainObjective.numTimes--;
                mainObjective.complete = mainObjective.numTimes == 0;
                break;
            case Condition.FindGenerator:
                mainObjective.numTimes--;
                mainObjective.complete = mainObjective.numTimes == 0;
                break;
            default:
                break;
        }
        UpdateObjectiveText();

        if (mainObjective.complete)
        {
            objectiveText.text = "COMPLETE, Return to Train";
        }
        
    }

}
