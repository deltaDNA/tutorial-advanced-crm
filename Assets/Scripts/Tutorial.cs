using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DeltaDNA;
using System;

[System.Serializable]
public class DynamicButton
{

    public string id;
    public string text;
    public int size;
    public string countdown;
    public string alignment;
}


[System.Serializable]
public class DynamicButtonList
{
    public List<DynamicButton> buttons;
    public DynamicButtonList()
    {
        buttons = new List<DynamicButton>();
    }
    public void LoadConfig()
    {
        // Load dynamic buttons array with the JSON configuration from gameParameters sent with Image Message
        var extensions = Resources.Load<TextAsset>("Buttons");
        //var extensions = im.Parameters["actionExtension"];
        buttons = JsonUtility.FromJson<DynamicButtonList>(extensions.ToString()).buttons;
    }
}

public class OfferTimer
{
    public DateTime offerExpityTime;
    public OfferTimer(int duration)
    {
        offerExpityTime = DateTime.Now.AddSeconds(duration);
    }
    public OfferTimer(DateTime expiryDateTime)
    {
        offerExpityTime = expiryDateTime;
    }

}


public class DynamicImageMessage  : MonoBehaviour 
{
   
    private ImageMessage imageMessage;
    public DynamicButtonList dynamicButtonList;
   
    public void SetImageMessage(ImageMessage imageMessage)
    {
        this.imageMessage = imageMessage; 
    }

    public void Start()
    {
        dynamicButtonList = new DynamicButtonList();

        dynamicButtonList.LoadConfig();
        Debug.Log(string.Format("Found {0} buttons in Dynamic Button Configuration", dynamicButtonList.buttons.Count));

        LoadImageMessageLayout();

    }

    void LoadImageMessageLayout()
    {
        Debug.Log("Loading Image Message Layout - isShowing = " + imageMessage.IsShowing().ToString());
        GameObject o = GameObject.Find("DeltaDNA Image Message");

        if (o != null)
        {
            Debug.Log("Image Message Children");
            int counter = 0;
            foreach(Transform t in o.transform)
            {
               
                if(t.name == "Button")
                {
                    DynamicButton b = dynamicButtonList.buttons[counter];
                    if (!string.IsNullOrEmpty(b.text) || !string.IsNullOrEmpty(b.countdown))
                    {
                        GameObject g = new GameObject("DynamicButtonText");
                        

                        Text txt = g.AddComponent<Text>();
                        txt.text = b.text;
                        txt.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                        txt.fontSize = b.size != 0 ? b.size:30;

                        if (b.alignment != null)
                        { 
                            TextAnchor a = (TextAnchor)Enum.Parse(typeof(TextAnchor), b.alignment.ToString());
                            txt.alignment = a; 
                        }
                        else
                        {
                            txt.alignment = TextAnchor.MiddleCenter;
                        }


                        RectTransform r = g.GetComponent<RectTransform>();
                        r.sizeDelta = new Vector2(txt.fontSize * 10, 100);


                        g.transform.SetPositionAndRotation(t.transform.position, t.transform.rotation);
                        g.transform.SetParent(o.transform);
                    }
                    counter++;
                }
                
               
            }
        }



    }




}







public class Tutorial : MonoBehaviour
{
    int userLevel = 1;
    public Text txtUserLevel ;
    public Text overlayTextPrefab;

   // public List<Level> levels; 

    // Start is called before the first frame update
    void Start()
    {
        txtUserLevel.text = string.Format("Level : {0}", userLevel);

        // Hook up callback to fire when DDNA SDK has received session config info, including Event Triggered campaigns.
        DDNA.Instance.NotifyOnSessionConfigured(true);
        DDNA.Instance.OnSessionConfigured += (bool cachedConfig) => GetGameConfig(cachedConfig);

        // Allow multiple game parameter actions callbacks from a single event trigger        
        DDNA.Instance.Settings.MultipleActionsForEventTriggerEnabled = true;

        //Register default handlers for event triggered campaigns. These will be candidates for handling ANY Event-Triggered Campaigns. 
        //Any handlers added to RegisterEvent() calls with the .Add method will be evaluated before these default handlers. 
        DDNA.Instance.Settings.DefaultImageMessageHandler =
            new ImageMessageHandler(DDNA.Instance, imageMessage => {
                // do something with the image message
                myImageMessageHandler(imageMessage);
            });
        DDNA.Instance.Settings.DefaultGameParameterHandler = new GameParametersHandler(gameParameters => {
            // do something with the game parameters
            myGameParameterHandler(gameParameters);
        });

        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
        DDNA.Instance.StartSDK();
    }


    // The callback indicating that the deltaDNA has downloaded its session configuration, including 
    // Event Triggered Campaign actions and logic, is used to record a "sdkConfigured" event 
    // that can be used provision remotely configured parameters. 
    // i.e. deferring the game session config until it knows it has received any info it might need
    public void GetGameConfig(bool cachedConfig)
    {
        Debug.Log("Configuration Loaded, Cached =  " + cachedConfig.ToString());
        Debug.Log("Recording a sdkConfigured event for Event Triggered Campaign to react to");

        // Create an sdkConfigured event object
        var gameEvent = new GameEvent("sdkConfigured")
            .AddParam("clientVersion", DDNA.Instance.ClientVersion)
            .AddParam("userLevel", userLevel);

        // Record sdkConfigured event and run default response hander
        DDNA.Instance.RecordEvent(gameEvent).Run();
    }



    private void myGameParameterHandler(Dictionary<string, object> gameParameters)
    {
        // Parameters Received      
        Debug.Log("Received game parameters from event trigger: " + DeltaDNA.MiniJSON.Json.Serialize(gameParameters));
    }


    private void myImageMessageHandler(ImageMessage imageMessage)
    {
        // Add a handler for the 'dismiss' action.
        imageMessage.OnDismiss += (ImageMessage.EventArgs obj) => {

            Debug.Log("Image Message dismissed by " + obj.ID);
            // NB : parameters not processed if player dismisses action
        };

        // Add a handler for the 'action' action.
        imageMessage.OnAction += (ImageMessage.EventArgs obj) => {
            Debug.Log("Image Message actioned by " + obj.ID + " with command " + obj.ActionValue);

            // Process parameters on image message if player triggers image message action
            if (imageMessage.Parameters != null) myGameParameterHandler(imageMessage.Parameters);
        };

        imageMessage.OnDidReceiveResources += () =>
        {
            Debug.Log("Received Image Message Assets");
        };


        // the image message is already cached and prepared so it will show instantly
        imageMessage.Show();

        // This Image Message Contains contains custom JSON to extend capabilities of contents        
        if (imageMessage.Parameters.ContainsKey("actionExtension"))
        {
            // DynamicImageMessage dm = new DynamicImageMessage(imageMessage, this.transform);           
            DynamicImageMessage dyn = gameObject.AddComponent<DynamicImageMessage>();
            dyn.SetImageMessage(imageMessage);
        }





    }
















    public void BttnLevelUp_Clicked()
    {
        userLevel++;
        txtUserLevel.text = string.Format("Level : {0}", userLevel);


        GameEvent myEvent = new GameEvent("levelUp")
            .AddParam("userLevel", userLevel)
            .AddParam("levelUpName", string.Format("Level {0}", userLevel));

        DDNA.Instance.RecordEvent(myEvent).Run();
    }

}
