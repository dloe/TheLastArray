using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretPresetTileInfo : PresetTileInfo
{
    public KeyHole keyHole;
    public GameObject interior;

    private void Update()
    {
        //if the door is opened, the inside is rendered
        if(keyHole.isActivated)
        {
            interior.layer = 0;
        }
    }
}
