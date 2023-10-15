using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Transitions : MonoBehaviour
{
    /// <summary>
    /// Transition Level Start behaviors
    /// Dylan Loe
    /// 
    /// Last Updated: 4/27/21
    /// 
    /// Notes:
    ///  - Scene transitions on scene startups
    ///     - using interpolation formula
    /// </summary>
    
    bool _fadeIn = false;
    bool _fadeOut = false;
    float _timeStart;
    float _u;
    bool _ctc = false;
    float _a0, _a1, _a01;
    bool _fading = false;
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
        if (_fadeIn)
        {
            LevelFadeIn();
        }
        else if (_fadeOut)
        {
            LevelFadeOut();
        }

    }

    #region Scene Transitions
    void StartFadeIn()
    {
        _fadeIn = true;
        _ctc = true;
        _u = 1.0f;
    }

    public void StartFadeOut()
    {
        _fadeOut = true;
        _ctc = true;
        _u = 0.0f;
    }
    /// <summary>
    /// Scene Transitions, will incorporate a fade in and out
    /// - will not use animator on canvas to avoid stuff being in update
    /// - will use interpolation on panels alpha
    /// </summary>
    void LevelFadeOut()
    {
        if (_ctc)
        {
            _a0 = 0f;
            _a1 = 1.0f;
            _ctc = false;
            _fading = true;
            _timeStart = Time.time;
        }
        if (_fading)
        {
            _u = (Time.time - _timeStart);
            //u = 1 - u;
            if (_u >= 1.0)
            {
                _u = 1;
                _fading = false;
                _fadeOut = false;
            }

            _a01 = (1 - _u) * _a0 + _u * _a1;

            Color temp = transBar.color;
            temp.a = _a01;
            transBar.color = temp;
        }
    }


    void LevelFadeIn()
    {
        if (_ctc)
        {
            _a0 = 1.0f;
            _a1 = 0f;
            _ctc = false;
            _fading = true;
            _timeStart = Time.time;
        }
        if (_fading)
        {
            _u = (Time.time - _timeStart) / 1.0f;
            _u = 1 - _u;
            if (_u <= 0.0f)
            {
                _u = 0;
                _fading = false;
                _fadeIn = false;
            }

            _a01 = (1 - _u) * _a0 + _u * _a1;

            Color temp = transBar.color;
            temp.a = 1 - _a01;
            transBar.color = temp;
        }
    }
    #endregion
}
