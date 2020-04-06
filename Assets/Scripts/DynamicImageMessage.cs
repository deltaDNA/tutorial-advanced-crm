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
    public string alignment;
    public string color;
    public bool isCountdown = false;
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




public class DynamicImageMessage : MonoBehaviour
{

    private ImageMessage imageMessage;
    public DynamicButtonList dynamicButtonList;
    public DateTime expiresAt = DateTime.MinValue;
    private Text countDownTimer = null;

    int refereshInterval = 1;
    float nextTime = 0;

    public void SetImageMessage(ImageMessage imageMessage)
    {
        this.imageMessage = imageMessage;
    }
    public void SetExpiryTime(DateTime expiresAt)
    {
        this.expiresAt = expiresAt;
    }

    public void Start()
    {
        dynamicButtonList = new DynamicButtonList();

        dynamicButtonList.LoadConfig();
        Debug.Log(string.Format("Found {0} buttons in Dynamic Button Configuration", dynamicButtonList.buttons.Count));

        LoadImageMessageLayout();
    }

    public void Update()
    {
        // Update Countdown Timer Text every second, if there is one that hasn't expired
        if (expiresAt > DateTime.Now && Time.time > nextTime && countDownTimer != null)
        {
            countDownTimer.text = expiresAt.Subtract(DateTime.Now).ToString(@"hh\:mm\:ss");
            nextTime += refereshInterval;
        }
    }

    void LoadImageMessageLayout()
    {
        Debug.Log("Loading Image Message Layout - isShowing = " + imageMessage.IsShowing().ToString());
        GameObject o = GameObject.Find("DeltaDNA Image Message");

        if (o != null)
        {
            Debug.Log("Image Message Children");
            int counter = 0;
            foreach (Transform t in o.transform)
            {

                if (t.name == "Button")
                {
                    DynamicButton b = dynamicButtonList.buttons[counter];
                    if (!string.IsNullOrEmpty(b.text) || b.isCountdown)
                    {
                        GameObject g = new GameObject("DynamicButtonText");

                        Text txt = g.AddComponent<Text>();
                        txt.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

                        // Assign countdown text object
                        if (b.isCountdown)
                        {
                            countDownTimer = txt;
                        }

                        // Set Overlay Text
                        txt.text = b.text;

                        // Set Overlay Font Size                                                                                        
                        txt.fontSize = b.size != 0 ? b.size : 30;

                        // Set Overlay Alignment
                        if (b.alignment != null)
                        {
                            TextAnchor a = (TextAnchor)Enum.Parse(typeof(TextAnchor), b.alignment.ToString());
                            txt.alignment = a;
                        }
                        else
                        {
                            txt.alignment = TextAnchor.MiddleCenter;
                        }

                        // Set Overlay Color
                        Color col;
                        if (ColorUtility.TryParseHtmlString("#" + b.color, out col))
                        {
                            txt.color = col;
                        }

                        // Set Position and Size and Attach to Image Message                        
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