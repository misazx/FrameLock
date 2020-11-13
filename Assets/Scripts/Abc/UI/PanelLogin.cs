using com.unity.mgobe.src.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityFrame;

public class PanelLogin : MonoBehaviour
{
    public InputField input;
    public Button btn;

    public bool randomOpenId;

    private void Start()
    {
        btn.onClick.AddListener(() => {
            OnLogin();
        });

        input.text = randomOpenId ? ("name" + Math.Floor(RandomUtil.Random() * 1000)) : "name";
    }

    void OnLogin()
    {
        StartCoroutine(GameMain.Instance.OnLogined(input.text));
    }
}
