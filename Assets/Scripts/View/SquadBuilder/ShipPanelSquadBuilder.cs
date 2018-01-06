﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SquadBuilderNS;

public class ShipPanelSquadBuilder : MonoBehaviour {

    public string ImageUrl;
    public string ShipName;

    // Use this for initialization
    void Start ()
    {
        LoadImage();
        SetName();
        SetOnClickHandler();
    }

    private void LoadImage()
    {
        if (ImageUrl != null) StartCoroutine(LoadTooltipImage(this.gameObject, ImageUrl));
    }

    private void SetName()
    {
        transform.Find("ShipName").GetComponent<Text>().text = ShipName;
    }

    private IEnumerator LoadTooltipImage(GameObject thisGameObject, string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (www.texture != null)
        {
            SetImageFromWeb(thisGameObject.transform.Find("ShipImage").gameObject, www);
        }
    }

    private void SetImageFromWeb(GameObject targetObject, WWW www)
    {
        Texture2D newTexture = new Texture2D(www.texture.height, www.texture.width);
        www.LoadImageIntoTexture(newTexture);
        Sprite newSprite = Sprite.Create(newTexture, new Rect(0, newTexture.height-124, newTexture.width, 124), Vector2.zero);
        targetObject.transform.GetComponent<Image>().sprite = newSprite;
    }

    private void SetOnClickHandler()
    {
        EventTrigger trigger = this.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(delegate { SelectShip(ShipName); });
        trigger.triggers.Add(entry);
    }

    private void SelectShip(string shipName)
    {
        SquadBuilder.CurrentShip = shipName;
        MainMenu.CurrentMainMenu.ChangePanel("SelectPilotPanel");
    }
}