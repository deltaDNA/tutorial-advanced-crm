using System;
using System.Collections.Generic;
using UnityEngine;


// Campaign List Class holds a list of decision point campaigns
// This can be used to make follow on decision point requests
// In order to display multiple image messages simultaneously

[System.Serializable]
public class CampaignList
{
    public List<Campaign> campaigns;
    public CampaignList()
    {
        campaigns = new List<Campaign>();
    }
    public void LoadCampaigns(string campaignsJSON)
    {
        campaigns = JsonUtility.FromJson<CampaignList>(campaignsJSON).campaigns;
    }
}

[System.Serializable]
public class Campaign
{
    public string decisionPoint;
    public string campaign;
}