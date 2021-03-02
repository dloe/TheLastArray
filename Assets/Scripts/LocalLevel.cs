﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
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
    [HideInInspector]
    public List<int> _posObjectives;

    //interpolation
    bool fadeIn = false;
    bool fadeOut = false;
    float timeStart;
    float u;
    bool ctc = false;
    float a0, a1, a01;
    bool fading = false;
    [SerializeField]
    public Image transBar;

    public int objective;

    private void Awake()
    {
        //assign tileGen obj
        _myTileGen = FindObjectOfType<TileGeneration>();
        //number of objectives
        _posObjectives = new List<int> { 1, 2, 3 };
        //LevelFadeIn();
        StartFadeIn();

        ChooseObjective();
    }

    



    private void Update()
    {
        if(fadeIn)
        {
            LevelFadeIn();
        }
        else if(fadeOut)
        {
            LevelFadeOut();
        }
    }

    /// <summary>
    /// - list of objectives
    /// - remove previous obj from list
    /// - pick randomly from updated list
    /// </summary>
    public void ChooseObjective()
    {
        Debug.Log("picking obj");


        objective = 3;
        return;

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
            //Debug.Log(_posObjectives.Count);
            //_posObjectives.RemoveAt(myPlayerData.previouslyCompletedObj);
            _posObjectives.Remove(myPlayerData.previouslyCompletedObj);
            _posObjectives = reshuffle(_posObjectives);
            //Debug.Log(_posObjectives.Count);
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

    #region Scene Transitions
    public void StartFadeIn()
    {
        // Debug.Log("on");
        fadeIn = true;
        ctc = true;
        u = 1.0f;
    }

    public void StartFadeOut()
    {
        fadeOut = true;
        ctc = true;
        u = 0.0f;
    }
    /// <summary>
    /// Scene Transitions, will incorperate a fade in and out
    /// - will not use animator on canvas to avoid stuff being in update
    /// - will use interpolation on panels alpha
    /// </summary>
    public void LevelFadeOut()
    {
        if(ctc)
        {
            a0 = 0f;
            a1 = 1.0f;
            ctc = false;
            fading = true;
            timeStart = Time.time;
        }
        if(fading)
        {
            u = (Time.time - timeStart);
            u = 1 - u;
            if (u >= 1.0)
            {
                u = 1;
                fading = false;
                fadeIn = false;
                Debug.Log("off");
            }

            a01 = (1 - u) * a0 + u * a1;

            Color temp = transBar.color;
            temp.a = 1 - a01;
            transBar.color = temp;
        }
    }


    public void LevelFadeIn()
    {
        if (ctc)
        {
            a0 = 1.0f;
            a1 = 0f;
            ctc = false;
            fading = true;
            timeStart = Time.time;
        }
        if (fading)
        {
            u = (Time.time - timeStart) / 1.0f;
            u = 1 - u;
            if (u <= 0.0f)
            {
                u = 0;
                fading = false;
                fadeIn = false;
               // Debug.Log("off");
            }

            a01 = (1 - u) * a0 + u * a1;

            Color temp = transBar.color;
            temp.a = 1 - a01;
            transBar.color = temp;
        }
    }
    #endregion
}
