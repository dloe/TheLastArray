using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LocalLevel : MonoBehaviour
{
    /// <summary>
    /// REQURIES:
    ///     - a TileGen Prefab
    /// </summary>
    public bool tempEndLevel = false;
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
    public Objectives _myObjs;

    public Text objectiveText;
    private int objectiveCountStart = 0;
    public static LocalLevel Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;


        //assign tileGen obj
        _myTileGen = FindObjectOfType<TileGeneration>();
        //number of objectives
        _posObjectives = new List<int> { 1, 2, 3 };
        //LevelFadeIn();
        StartFadeIn();

        //ChooseObjective();
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

        //temp test of level transition
        if(tempEndLevel)
        {
            LevelBeat();
            tempEndLevel = false;
        }
    }

    //PlayerUIHolder myUIVARs;
    /// <summary>
    /// - list of objectives
    /// - remove previous obj from list
    /// - pick randomly from updated list
    /// </summary>
    public void ChooseObjective()
    {
        //Debug.Log("picking obj");


       // objective = 3;
      //  return;

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
        _myObjs = FindObjectOfType<Objectives>();
         PlayerUIHolder myUIVARs = FindObjectOfType<PlayerUIHolder>();
        //player = GameObject.Find("PlayerHolder");
        objectiveText = myUIVARs.objectiveText;
        // objectiveText.text = objective.ToString();
        //  transBar = myUIVARs.panel;
        
        //set text for objectives
        switch (objective)
        {
            case 1:
               // objectiveCountStart = _myObjs.objectives.Count;
                objectiveText.text = "Kill the enemy";
                break;
            case 2:
                objectiveCountStart = _myObjs.objectives.Count;
                objectiveText.text = "Collect Gas: " + _myObjs.objectives.Count + "/" + objectiveCountStart;
                break;
            case 3:
                objectiveCountStart = _myObjs.objectives.Count;
                objectiveText.text = "Collect Generator: " + _myObjs.objectives.Count + "/" + objectiveCountStart;
                break;
            default:
                break;
        }


        Debug.Log("Picked Objective: " + objective);
    }

    public void LevelBeat()
    {
        StartCoroutine(LevelWonEvent());
    }

    //updates when objectives are choosen and when objectives are met (or steps of it are met)
    public void UpdateObjectiveUI()
    {

    }

    /// <summary>
    ///  - when player accomplishes object and gets to extraction
    ///     - saves data to scriptable obj
    ///     - fade out
    ///     - player leaves this scene, goes to crafting scene (for now next level)
    ///     - update levels beaten var
    /// </summary>
    IEnumerator LevelWonEvent()
    {
        StartFadeOut();
        //save player data to scriptable obj

        yield return new WaitForSeconds(1.0f);
        //transition scene
        SceneManager.LoadScene(1);
        Debug.Log("Loading next scene");
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
            //u = 1 - u;
            if (u >= 1.0)
            {
                u = 1;
                fading = false;
                fadeOut = false;
                Debug.Log("off");
            }

            a01 = (1 - u) * a0 + u * a1;

            Color temp = transBar.color;
            temp.a = a01;
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
