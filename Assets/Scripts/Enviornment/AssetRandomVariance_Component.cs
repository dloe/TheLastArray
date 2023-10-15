using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRandomVariance_Component : MonoBehaviour
{
    /// <summary>
    /// Asset Random Variance Component
    /// Dylan Loe
    /// 
    /// Last Updated: 9/11/23
    /// 
    /// - having configurable, adjustable params that can be chosen at random for things like lights on cars, lights, particle effects, etc
    /// </summary>

    [Header("Set Light Components to randomize")]
    public bool SetLightsToRandomize = false;
    public Light[] assetLights;
    public int mandatoryLightOverride = 1;
    int _lightsToActivate = 0;
    public float intensityOverride = -1;

    [Space(10)]
    [Header("Set Light Components to randomize")]
    public bool SetMaterialToRandomize = false;
    public Material[] possibleMatColors;
    public MeshRenderer[] meshesToColor;


    // Start is called before the first frame update
    void Start()
    {
        if(SetMaterialToRandomize)
            RandomizeBasesMaterial();

        if(SetLightsToRandomize)
            RandomizeLights();
    }

    void RandomizeBasesMaterial()
    {
        
        Material choosenMat = possibleMatColors[Random.Range(0, possibleMatColors.Length - 1)];

        for(int index = 0; index < meshesToColor.Length; index++)
        {
            meshesToColor[index].material = choosenMat;
        }
    }

    void RandomizeLights()
    {
        if (assetLights.Length > 1)
        {
            assetLights = reshuffle(assetLights);
        }
        //mix up array of lights

        if (mandatoryLightOverride > 1)
            _lightsToActivate = Random.Range(mandatoryLightOverride, assetLights.Length - 1);
        else if (mandatoryLightOverride == 1)
        {
            _lightsToActivate = 0;
            SetupRandomLight(assetLights[_lightsToActivate]);
            return;
        }
        else
        {
            //if neg 1, then any of them are fair game
            _lightsToActivate = Random.Range(0, assetLights.Length - 1);
        }

        for (int index = 0; index < assetLights.Length; index++)
        {
            if (index < _lightsToActivate)
                SetupRandomLight(assetLights[index]);
            else
                assetLights[index].intensity = 0;
        }
    }

    //set up slight variance in color
    //set up slight variance in range
    //set up slight variance in intensity
    void SetupRandomLight(Light light)
    {
        if(intensityOverride > 0)
            light.intensity += Random.Range(-intensityOverride, 0);
        else
            light.intensity += Random.Range(-20, 0);
        light.range += Random.Range(-6, 0);
        light.spotAngle += Random.Range(-2, 5);
        light.color = new Color(light.color.r + Random.Range(-0.06f, 0.06f), light.color.g + Random.Range(-0.06f, 0.06f), 
            light.color.b + Random.Range(-0.06f, 0.06f), light.color.a + Random.Range(-6, 2)); //Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }


    //reshuffle list
    Light[] reshuffle(Light[] ar)
    {
        for (int t = 0; t < ar.Length; t++)
        {
            Light tmp = ar[t];
            int r = Random.Range(t, ar.Length);
            ar[t] = ar[r];
            ar[r] = tmp;
        }
        return ar;
    }
}
