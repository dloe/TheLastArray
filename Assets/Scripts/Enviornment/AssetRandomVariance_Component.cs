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
    /// - having configurable, adjustable params that can be choosen at random for things like lights on cars, lights, particle effects, etc
    /// </summary>

    [Header("Set Light Components to randomize")]
    public bool SetLightsToRandomize = false;
    public Light[] assetLights;
    public int mandatoryLightOverride = 1;
    int lightsToActivate = 0;

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

        if (mandatoryLightOverride != -1)
            lightsToActivate = Random.Range(mandatoryLightOverride, assetLights.Length - 1);

        for (int index = 0; index < assetLights.Length; index++)
        {
            if (index < lightsToActivate)
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
        light.intensity += Random.Range(-25, 4);
        light.range += Random.Range(-6, 4);
        light.spotAngle += Random.Range(-2, 5);
        light.color = new Color(light.color.r + Random.Range(-0.06f, 0.06f), light.color.g + Random.Range(-0.06f, 0.06f), 
            light.color.b + Random.Range(-0.06f, 0.06f), light.color.a + Random.Range(-6, 5)); //Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
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
