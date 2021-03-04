using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    public bool isActivated = false;
    public Text generatorText;

    public void Activate()
    {
        if(!isActivated)
        {
            isActivated = true;
            generatorText.text += "(Activated)";
            Objectives.Instance.SendCompletedMessage(Condition.FindGenerator);
        }
        
    }
}
