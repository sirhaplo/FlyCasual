﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SquadBuilderNS;

public class PilotPanelSquadBuilder : MonoBehaviour {

    private SquadBuilderShip Ship;
    private string ImageUrl;
    private string ShipName;
    private string PilotName;
    private Action<SquadBuilderShip, string, string> OnClick;

    public void Initialize(string pilotName, string shipName, string imageUrl = null, Action<SquadBuilderShip, string, string> onClick = null)
    {
        PilotName = pilotName;
        ShipName = shipName;
        ImageUrl = imageUrl;
        OnClick = onClick;
    }

    public void Initialize(SquadBuilderShip ship, Action<SquadBuilderShip, string, string> onClick = null)
    {
        Ship = ship;
        PilotName = ship.Instance.PilotName;
        ShipName = ship.Instance.Type;
        ImageUrl = ship.Instance.ImageUrl;
        OnClick = onClick;
    }

    void Start()
    {
        LoadImage();
        SetOnClickHandler();
    }

    private void LoadImage()
    {
        StartCoroutine(LoadTooltipImage(this.gameObject, ImageUrl));
    }

    private IEnumerator LoadTooltipImage(GameObject thisGameObject, string url)
    {
        if (url == null)
        {
            Debug.Log(PilotName);
            Debug.Log(ShipName);
            url = SquadBuilder.AllPilots.Find(n => n.PilotName == PilotName && n.PilotShip.ShipName == ShipName).Instance.ImageUrl;
        }

        WWW www = new WWW(url);
        yield return www;

        if (www.texture != null)
        {
            SetImageFromWeb(thisGameObject.transform.Find("PilotImage").gameObject, www);
        }
    }

    private void SetImageFromWeb(GameObject targetObject, WWW www)
    {
        Texture2D newTexture = new Texture2D(www.texture.height, www.texture.width);
        www.LoadImageIntoTexture(newTexture);
        Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), Vector2.zero);
        targetObject.transform.GetComponent<Image>().sprite = newSprite;
    }

    private void SetOnClickHandler()
    {
        if (OnClick != null)
        {
            EventTrigger trigger = this.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(delegate { OnClick(Ship, PilotName, ShipName); });
            trigger.triggers.Add(entry);
        }
    }

}