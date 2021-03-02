using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalLevel : MonoBehaviour
{
    /// <summary>
    /// REQURIES:
    ///     - a TileGen Prefab
    /// </summary>


    //holds objective info and game loop info
    //consider putting ui here maybe
    TileGeneration _myTileGen;

    public PlayerData myPlayerData;
    public List<int> _posObjectives;

    private void Awake()
    {
        //assign tileGen obj
        _myTileGen = FindObjectOfType<TileGeneration>();
        _posObjectives = new List<int> { 1, 2, 3 };
    }

    /// <summary>
    /// - list of objectives
    /// - remove previous obj from list
    /// - pick randomly from updated list
    /// </summary>
    public void ChooseObjective()
    {
        Debug.Log("picking obj");
        int objective;
        //picks objective - cant be the previous objective
        if(myPlayerData.previouslyCompletedObj == -1)
        {
            //randomly pick any
            int rand = Random.Range(0, _posObjectives.Count);
            //Debug.Log(_posObjectives.Count);
            objective = _posObjectives[rand];
        }
        else
        {
            //exclude previous obj index from choice
            _posObjectives.RemoveAt(myPlayerData.previouslyCompletedObj);
            _posObjectives = reshuffle(_posObjectives);
            objective = _posObjectives[Random.Range(0, _posObjectives.Count)];
        }



        Debug.Log(objective);
    }

    //reshuffle list
    List<int> reshuffle(List<int> ar)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < ar.Count; t++)
        {
            int tmp = ar[t];
            int r = Random.Range(t, ar.Count);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }
}
