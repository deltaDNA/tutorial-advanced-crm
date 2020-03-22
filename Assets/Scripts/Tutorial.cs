using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeltaDNA; 

public class Tutorial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
        DDNA.Instance.StartSDK();

    }


}
