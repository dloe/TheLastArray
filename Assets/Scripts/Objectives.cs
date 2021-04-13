using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public enum Condition
{
    KillEnemy,
    GetGasCan,
    FindGenerator
}
public class Objectives : MonoBehaviour
{
    public static Objectives Instance;
    public GameObject gasolineObject, generatorObject;
    public string killMessage, gasMessage, generatorMessage, finalMessage;
    public ItemData gasolineData;
    public Objective mainObjective;
    public int objectiveCount = 0;

    public Text objectiveText;


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
        if (SceneManager.GetActiveScene().name == Player.Instance.baseData.trainSceneName)
        {
            gameObject.SetActive(false);
        }
    }


    public void UpdateObjectiveText()
    {
        Debug.Log("check");
        if (mainObjective.objectiveMessage == gasMessage || mainObjective.objectiveMessage == generatorMessage)
        {
            objectiveText.text = mainObjective.objectiveMessage + " " + (objectiveCount - mainObjective.numTimes) + "/" + objectiveCount;
        }
        else
        {
            objectiveText.text = mainObjective.objectiveMessage;
        }
    }


    public GameObject SetObjectiveRef(int objectiveInt, GameObject spot)
    {
        GameObject returnObj = null;
        Gasoline objGasoline = null;
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
                objGasoline = Instantiate(gasolineObject, spot.transform.position, gasolineObject.transform.rotation).GetComponent<Gasoline>();
                returnObj = objGasoline.gameObject;
                objective.objectiveMessage = gasMessage;

                break;
            case 3:
                objective.condition = Condition.FindGenerator;
                objGenerator = Instantiate(generatorObject, spot.transform.position, generatorObject.transform.rotation).GetComponent<Generator>();
                returnObj = objGenerator.gameObject;
                objective.objectiveMessage = generatorMessage;
                break;
            case 4:
                objective.condition = Condition.KillEnemy;
                objective.objectiveMessage = finalMessage;
                break;
            default:
                break;
        }

        if (objective.objectiveMessage == mainObjective.objectiveMessage)
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

    /// <summary>
    /// Dylan
    /// 
    /// - temporary objective tracker for last level
    /// </summary>
    /// <param name="phase"> to show what part of objective player is on </param>
    public void UpdateFinalObjective(int phase)
    {
        if(phase == 0)
        {
            Debug.Log("Spawned boss");
            finalMessage = "Survive";
            mainObjective.objectiveMessage = finalMessage;
            UpdateObjectiveText();
        }
        else if(phase == 1)
        {
            Debug.Log("Activate array");
            finalMessage = "Activate Last Array";
            mainObjective.objectiveMessage = finalMessage;
            UpdateObjectiveText();
        }
        else if(phase == 2)
        {
            Debug.Log("objective complete");
            finalMessage = "Return to Train for Extraction";
            mainObjective.objectiveMessage = finalMessage;
            UpdateObjectiveText();
        }
        else if(phase == 3)
        {
            Debug.Log("last objective");
            finalMessage = "Locate the Last Array";
            UpdateObjectiveText();
        }
    }
    


    public void SendCompletedMessage(Condition condition)
    {
        switch (condition)
        {
            case Condition.KillEnemy:
                Debug.Log("Enemy Killed");
                
                mainObjective.complete = true;
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
            if(SceneManager.GetActiveScene().name == Player.Instance.baseData.levelFourName)
            {
                objectiveText.text = "YOU WIN!";
                Player.Instance.endScreenText.text = "You Win!";
                Time.timeScale = 0;
                Player.Instance.endScreen.SetActive(true);
            }
            else
            {
                objectiveText.text = "Return to Train";
            }
            
        }
        
    }

}
