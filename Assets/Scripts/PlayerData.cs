using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScritableObjects/PlayerData", order = 2)]
public class PlayerData : ScriptableObject
{
    public bool levelLoaded;

    public int previouslyCompletedObj;

    public string currentObjString;
    public int objectiveCount;
    public bool objectiveComplete;
}
