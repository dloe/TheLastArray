using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : Activatable
{
    /// <summary>
    /// Generator Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    ///  - Child of activatable
    ///  - Generators are one of the objects players may need to find throughout the level
    ///  - multiple generators are needed in order to beat the level
    /// 
    /// </summary>

    public Text generatorText;
    public GameObject objectiveMarker;

    public override void Activate()
    {
        if(!isActivated)
        {
            isActivated = true;
            generatorText.text += "(Activated)";
            Objectives.Instance.SendCompletedMessage(Condition.FindGenerator);
            objectiveMarker.SetActive(false);
        }
    }
}
