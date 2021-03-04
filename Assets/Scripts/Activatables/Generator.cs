using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : Activatable
{
    
    public Text generatorText;

    public override void Activate()
    {
        if(!isActivated)
        {
            isActivated = true;
            generatorText.text += "(Activated)";
            Objectives.Instance.SendCompletedMessage(Condition.FindGenerator);
        }
        
    }
}
