using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScritableObjects/PlayerData", order = 2)]
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

    //player stats
    //placeholders for right now
    public int health = 15;
    public int speed = 5;
}
