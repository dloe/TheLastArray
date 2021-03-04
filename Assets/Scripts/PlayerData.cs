using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable][CreateAssetMenu(fileName = "PlayerData", menuName = "ScritableObjects/PlayerData", order = 2)]
public class PlayerData : ScriptableObject
{

    public bool levelLoaded;
    [Header("Previously completed objective")]
    public int previouslyCompletedObj;

    [Header("Current Objective Info")]
    public string currentObjString;
    public int objectiveCount;
    public bool objectiveComplete;

    [Header("Current Level")]
    //starts at 1 -> 4
    public int currentLevelNumber = 1;
    
    [Space(10)]
    [Header("Player Stats")]
    //player stats
    //placeholders for right now
    public int maxHealth = 15;
    public int health = 15;
    public int dmgResist = 1;
    public int speedStat = 5;
    public int scrap, cloth, meds;
    public int skillPoints = 0;







}
