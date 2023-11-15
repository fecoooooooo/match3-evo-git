using Match3_Evo;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvoBtn : MonoBehaviour
{
	public Image art;

	public int variantIndex = -1;
    public int evolutionIndex = -1;

	bool clickListenerEnabled = false;

	void Start()
	{
		if (variantIndex == -1)
			throw new Exception("Specify variant index");
		if (evolutionIndex == -1)
			throw new Exception("Specify evolution index");

		clickListenerEnabled = false;
	}

	public void OnClick()
	{
		if (clickListenerEnabled)
		{
			GM.boardMng.ChooseEvoltion(variantIndex, evolutionIndex);
			clickListenerEnabled = false;
		}
	}

	public void SetClickListenerEnabled(bool enabled)
	{
		clickListenerEnabled = enabled;
	}
}
