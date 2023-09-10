using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    /// <summary>
    /// Level Audio Manager
    /// Dylan Loe
    /// 
    /// Updated: 9/9/23
    /// 
    /// 
    /// Notes:
    /// - For establishing what audio is used in the level
    /// - locally configure array of what ambient audio, then randomly selects one
    /// - locally can configure array of what background music, then randomly selects one
    /// 
    /// TO DO:
    /// - make sure mixing is decent on ambient noises for level 1
    /// 
    /// </summary>

    public AudioSource audioSourceComponent;

    public AudioClip[] ambientAudioClips;
    public AudioClip[] backgroundMusicAudioClips;

    public AudioClip overrideChoice;

    // Start is called before the first frame update
    void Start()
    {
        if(overrideChoice == null && ambientAudioClips.Length != 0)
        {
            audioSourceComponent.clip = ambientAudioClips[Random.Range(0, ambientAudioClips.Length)];
        }
        else
        {
            if(overrideChoice != null)
                audioSourceComponent.clip = overrideChoice;
        }
    }


}
