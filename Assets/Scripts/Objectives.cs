using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Condition
{
    KillAllEnemies,
    GetKeyItem,
    ReachArea
}
public class Objectives : MonoBehaviour
{
    public static Objectives Instance;
    public GameObject emptyWorldItem;
    public List<ItemData> possibleItems;
    public List<Objective> objectives;

    public int enemyCount;

    [System.Serializable]
    public class Objective
    {
        public Condition condition;
        public string objectiveMessage = "blank";
        public ItemData itemData;
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
                objective.condition = Condition.KillAllEnemies;
                break;
            case 2:
            case 3:
                objective.condition = Condition.GetKeyItem;
                objective.itemData = possibleItems[Random.Range(0, possibleItems.Count)];
                WorldItem objItem = Instantiate(emptyWorldItem, spot.transform.position, emptyWorldItem.transform.rotation).GetComponent<WorldItem>();
                objItem.worldItemData = objective.itemData;
                break;
            //case 3:
            //    objective.condition = Condition.ReachArea;
            //    break;
            default:
                break;
        }
        objective.objectiveMessage = objective.condition.ToString();

        objectives.Add(objective);
    }

    public bool CheckWinCondition(Condition condition)
    {
        if (condition == Condition.KillAllEnemies)
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
                    case Condition.KillAllEnemies:
                        objective.complete = enemyCount == 0;
                        break;
                    case Condition.GetKeyItem:
                        objective.complete = Player.Instance.inventory.Contains(objective.itemData);
                        break;
                    case Condition.ReachArea:
                        objective.complete = false;
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
