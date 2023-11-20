using Match3_Evo;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvoManagerUI : MonoBehaviour
{
    public int variantIndex = -1;
    List<EvoImg> evoImgs;
    List<EvoBtn> evoBtns;

    private void Start()
    {
        evoImgs = GetComponentsInChildren<EvoImg>().ToList();
        evoBtns = GetComponentsInChildren<EvoBtn>().ToList();

        SetupGraphicsAndCounterLabels();

        UpdateUI();

        GM.boardMng.mergeEvent.AddListener(OnMergeEvent);
        GM.boardMng.evolutionDecidedEvent.AddListener(OnEvolutionDecidedEvent);
    }

	void SetupGraphicsAndCounterLabels()
	{
		FieldDataEvo fieldData = GM.boardMng.FieldData[variantIndex];
        
        for(int i = 0; i < evoImgs.Count; ++i)
		{
            evoImgs[i].art.sprite = fieldData.fieldDataTiers[i].basic;
            evoImgs[i].counterTxt.text = GM.boardMng.gameParameters.mergesToNextEvolution[i].ToString();
        }

        for(int i = 0; i < evoBtns.Count; ++i)
		{
            int fieldDataIndex = BoardManager.DECIDE_EVOLUTION_LEVEL + i;
            evoBtns[i].art.sprite = fieldData.fieldDataTiers[fieldDataIndex].basic;
		}
    }

    void UpdateUI()
	{
        int evolutionLvlForVariant = GM.boardMng.currentEvolutionLvlPerVariant[variantIndex];

        for(int i = 0; i < evoImgs.Count; ++i)
		{
            evoImgs[i].gameObject.SetActive(i <= evolutionLvlForVariant);
            evoImgs[i].counterTxt.gameObject.SetActive(i == evolutionLvlForVariant);

            if(i == evolutionLvlForVariant)
                evoImgs[i].counterTxt.text = GM.boardMng.currentMergeCountToNextEvolvePerVariant[variantIndex].ToString();
		}

        for (int i = 0; i < evoBtns.Count; ++i)
	    {
            evoBtns[i].gameObject.SetActive(evolutionLvlForVariant == i + BoardManager.DECIDE_EVOLUTION_LEVEL);
        }
	}

    void OnMergeEvent(int variant)
	{
        if (variantIndex == variant)
            UpdateUI();
	}

    void OnEvolutionDecidedEvent(int variant)
	{
        if (variantIndex == variant)
            UpdateUI();
	}
}

