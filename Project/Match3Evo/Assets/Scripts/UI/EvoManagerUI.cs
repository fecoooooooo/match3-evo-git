using Match3_Evo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvoManagerUI : MonoBehaviour
{
    public int variantIndex = -1;
    List<EvoBtn> evoBtns;

    private void Start()
    {
        evoBtns = GetComponentsInChildren<EvoBtn>().ToList();

        SetupGraphicsAndCounterLabels();

        UpdateUI();

        GM.boardMng.mergeEvent.AddListener(OnMergeEvent);
    }

	private void SetupGraphicsAndCounterLabels()
	{
		FieldDataEvo fieldData = GM.boardMng.FieldData[variantIndex];
        for(int i = 0; i < fieldData.fieldDataTiers.Length; ++i)
            evoBtns[i].art.sprite = fieldData.fieldDataTiers[i].basic;

        for(int i = 0; i < GM.boardMng.gameParameters.mergesToNextEvolution.Length; ++i)
            evoBtns[i].counterTxt.text = GM.boardMng.gameParameters.mergesToNextEvolution[i].ToString();
	}

	void UpdateUI()
	{
        int evolutionLvlForVariant = GM.boardMng.currentEvolutionStates[variantIndex];
        for(int i = 0; i < evoBtns.Count; ++i)
		{
            evoBtns[i].gameObject.SetActive(i <= evolutionLvlForVariant);
            evoBtns[i].counterTxt.gameObject.SetActive(i == evolutionLvlForVariant);

            if(i == evolutionLvlForVariant)
                evoBtns[i].counterTxt.text = GM.boardMng.currentMergeToEvolve[variantIndex].ToString();
		}
	}

    void OnMergeEvent(int variant)
	{
        if (variantIndex == variant)
            UpdateUI();
	}
}

