using Match3_Evo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DnsManager : MonoBehaviour
{
    public Image spiralImg;
    float currentDnsAmount;

    public bool DnsMeterFilled { get => 1f <= currentDnsAmount; }
    public void CollectDns()
	{
        currentDnsAmount += GM.boardMng.gameParameters.dnsAmountPerCollect;
        spiralImg.fillAmount = Mathf.Clamp01(currentDnsAmount);
	}
}
