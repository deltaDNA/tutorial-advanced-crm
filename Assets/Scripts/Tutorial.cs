using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DeltaDNA;
using System;



public class Tutorial : MonoBehaviour
{
    int userLevel = 1;
    public Text txtUserLevel ;

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

            DateTime dt = DateTime.Now.AddSeconds(720);
            dyn.SetExpiryTime(dt);
        }

    }




    //  Generic Method to make an Engage In-Game Decision Point campaign request for an Image Message
    private void DecisionPointImageMessageCampaignRequest(string decisionPointName, Params decisionPointParmeters)
    {

        DDNA.Instance.EngageFactory.RequestImageMessage(decisionPointName, decisionPointParmeters, (imageMessage) => {

            // Check we got an engagement with a valid image message.
            if (imageMessage != null)
            {
                Debug.Log("Engage Decision Point returned a valid image message.");
                myImageMessageHandler(imageMessage);

                // Process Multiple PopUps from decision point campaign
                ProcessMultiplePopups(imageMessage.Parameters);
            }
            else
            {
                Debug.Log("Engage Decision Point didn't return an image message.");
            }
        });
    }

    private void ProcessMultiplePopups(Dictionary<string, object> gameParameters)
    {       
        // Check for "campaignList" game Parameter specifying follow on campaigns. 
        if (gameParameters.ContainsKey("campaignList"))
        {
            CampaignList campaignList = new CampaignList();
            campaignList.LoadCampaigns(gameParameters["campaignList"].ToString());

            // Make additional Decision Poing Campaign requests for each campaign in campaignList
            foreach(Campaign campaign in campaignList.campaigns)
            {
                Params dpRequestParams = new Params().AddParam("campaign", campaign.campaign);
                DecisionPointImageMessageCampaignRequest(campaign.decisionPoint, dpRequestParams);
            }
        }
    }



    // UI Button Handlers
    public void BttnLevelUp_Clicked()
    {
        userLevel++;
        txtUserLevel.text = string.Format("Level : {0}", userLevel);


        GameEvent myEvent = new GameEvent("levelUp")
            .AddParam("userLevel", userLevel)
            .AddParam("levelUpName", string.Format("Level {0}", userLevel));

        DDNA.Instance.RecordEvent(myEvent).Run();
    }



    public void BttnDecisionPointTest_Clicked()
    {
        DecisionPointImageMessageCampaignRequest("test", null);
    }

}

