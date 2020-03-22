using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DeltaDNA; 

public class Tutorial : MonoBehaviour
{
    int userLevel = 1;
    public Text txtUserLevel ; 

    // Start is called before the first frame update
    void Start()
    {
        txtUserLevel.text = string.Format("Level : {0}", userLevel);


        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
        DDNA.Instance.StartSDK();

    }

    public void BttnLevelUp_Clicked()
    {
        userLevel++;
        txtUserLevel.text = string.Format("Level : {0}" , userLevel);


        GameEvent myEvent = new GameEvent("levelUp")
            .AddParam("userLevel", userLevel)
            .AddParam("levelUpName", string.Format("Level {0}", userLevel));

        DDNA.Instance.RecordEvent(myEvent).Run();
    }

}
