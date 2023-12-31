﻿using Match3_Evo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DnsManager : MonoBehaviour
{
    public Image spiralImg;
    float currentDnsAmount = 0f;
	public bool DnsMeterFilled { get => 1f <= currentDnsAmount; }

	private void Awake()
	{
		GM.dnsManager = this;
	}

	private void Start()
	{
		UpdateUI();
	}

    public void CollectDns()
	{
        currentDnsAmount += GM.boardMng.gameParameters.dnsAmountPerCollect;
        UpdateUI();

    }

    void UpdateUI()
	{
        spiralImg.fillAmount = Mathf.Clamp01(currentDnsAmount);
	}

	internal void UseDns()
	{
		currentDnsAmount -= 1f;
		UpdateUI();
	}
}
