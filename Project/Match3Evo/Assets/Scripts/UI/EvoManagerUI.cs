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
    bool decidingEvo;

    private void Start()
    {
        evoImgs = GetComponentsInChildren<EvoImg>().ToList();
        evoBtns = GetComponentsInChildren<EvoBtn>().ToList();

        SetupGraphicsAndCounterLabels();

        UpdateUI();

        GM.boardMng.mergeEvent.AddListener(OnMergeEvent);
        GM.boardMng.promptDecideEvolutionEvent.AddListener(OnPromptDecideEvolutionEvent);
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
            evoImgs[i].counterTxt.gameObject.SetActive(i == evolutionLvlForVariant && !decidingEvo);

            if(i == evolutionLvlForVariant)
                evoImgs[i].counterTxt.text = GM.boardMng.currentMergeCountToNextEvolvePerVariant[variantIndex].ToString();
		}

        for (int i = 0; i < evoBtns.Count; ++i)
	    {
            bool currentEnabled = evolutionLvlForVariant == i + BoardManager.DECIDE_EVOLUTION_LEVEL;

            evoBtns[i].SetClickListenerEnabled(decidingEvo || currentEnabled);
            evoBtns[i].gameObject.SetActive(decidingEvo || currentEnabled);
        }
	}

    void OnMergeEvent(int variant)
	{
        if (variantIndex == variant)
            UpdateUI();
	}

    void OnPromptDecideEvolutionEvent(int variant)
    {
        if (variantIndex == variant)
		{
            decidingEvo = true;
            UpdateUI();
        }
    }

    void OnEvolutionDecidedEvent(int variant)
	{
        if (variantIndex == variant)
        {
            decidingEvo = false;
            UpdateUI();
        }
	}
}

