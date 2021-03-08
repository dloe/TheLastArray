using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Transitions : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Awake()
    {
        StartFadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeIn)
        {
            LevelFadeIn();
        }
        else if (fadeOut)
        {
            LevelFadeOut();
        }

    }

   

    #region Scene Transitions
    void StartFadeIn()
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
    void LevelFadeOut()
    {
        if (ctc)
        {
            a0 = 0f;
            a1 = 1.0f;
            ctc = false;
            fading = true;
            timeStart = Time.time;
        }
        if (fading)
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


    void LevelFadeIn()
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
