using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Condition
{
    KillEnemy,
    GetKeyItem,
    FindGenerator
}
public class Objectives : MonoBehaviour
{
    public static Objectives Instance;
    public GameObject emptyWorldItem, generatorObject;
    public List<ItemData> possibleItems;
    public List<Objective> objectives;

    public int enemyCount;

    //public Objective mainObjective;

    [System.Serializable]
    public class Objective
    {
        public Condition condition;
        public string objectiveMessage = "blank";
        public ItemData itemData;
        public Generator generator;
        public bool complete;
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

    public void AddObjective(int objectiveInt, GameObject spot)
    {
        Objective objective = new Objective();
        switch (objectiveInt)
        {
            case 1:
                objective.condition = Condition.KillEnemy;
                break;
            case 2:
                objective.condition = Condition.GetKeyItem;
                objective.itemData = possibleItems[Random.Range(0, possibleItems.Count)];
                WorldItem objItem = Instantiate(emptyWorldItem, spot.transform.position, emptyWorldItem.transform.rotation).GetComponent<WorldItem>();
                objItem.worldItemData = objective.itemData;
                break;
            case 3:
                objective.condition = Condition.FindGenerator;
                objective.generator = Instantiate(generatorObject, spot.transform.position, generatorObject.transform.rotation).GetComponent<Generator>();
                break;
            default:
                break;
        }
        objective.objectiveMessage = objective.condition.ToString();

        objectives.Add(objective);
        //mainObjective = objectives[0];
    }

    public GameObject AddObjectiveRef(int objectiveInt, GameObject spot)
    {
        GameObject returnObj = null;
        WorldItem objItem = null;
        Objective objective = new Objective();
        switch (objectiveInt)
        {
            case 1:
                objective.condition = Condition.KillEnemy;
                break;
            case 2:
                objective.condition = Condition.GetKeyItem;
                objective.itemData = possibleItems[Random.Range(0, possibleItems.Count)];
                objItem = Instantiate(emptyWorldItem, spot.transform.position, emptyWorldItem.transform.rotation).GetComponent<WorldItem>();
                objItem.worldItemData = objective.itemData;
                returnObj = objItem.gameObject;
                break;
            case 3:
                objective.condition = Condition.FindGenerator;
                objective.generator = Instantiate(generatorObject, spot.transform.position, generatorObject.transform.rotation).GetComponent<Generator>();
                returnObj = objective.generator.gameObject;
                break;
            default:
                break;
        }
        objective.objectiveMessage = objective.condition.ToString();

        objectives.Add(objective);
        //mainObjective = objectives[0];

        return returnObj;
    }

    public bool CheckWinCondition(Condition condition)
    {
        if (condition == Condition.KillEnemy)
        {
            return true;
        }
        if (objectives.Count == 0)
        {
            return false;
        }
        bool objectivesComplete = true;
        foreach (Objective objective in objectives)
        {
            if (objective.condition == condition && !objective.complete)
            {
                switch (objective.condition)
                {
                    case Condition.KillEnemy:
                        objective.complete = enemyCount == 0;
                        break;
                    case Condition.GetKeyItem:
                        objective.complete = Player.Instance.inventory.Contains(objective.itemData);
                        break;
                    case Condition.FindGenerator:
                        objective.complete = objective.generator.isActivated;
                        break;
                    default:
                        break;
                }
            }
            if (!objective.complete)
            {
                objectivesComplete = false;
                
            }
        }
        return objectivesComplete;
    }

}
