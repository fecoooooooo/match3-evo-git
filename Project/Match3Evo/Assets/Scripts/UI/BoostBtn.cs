using Match3_Evo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoostBtn : MonoBehaviour
{
    readonly float COOLDOWN_TIME = 30f;

    public RectTransform gameRoot;
    public Image cooldownImg;
    public Image draggableImg;
    Image ownImg;

    float currentCooldownTime = 0f;
    bool onCooldown;
    bool dragging;


    void Start()
    {
        ownImg = GetComponent<Image>();
    }

    void Update()
    {
		if (onCooldown)
		{
            currentCooldownTime -= Time.deltaTime;
            cooldownImg.fillAmount = currentCooldownTime / COOLDOWN_TIME;
            if(currentCooldownTime < 0)
			{
                onCooldown = false;
                currentCooldownTime = 0;
                cooldownImg.fillAmount = 0;
			}
		}
        
        if(dragging)
		{
            if (Input.GetMouseButtonUp(0))
                OnReleaseMouse();

            draggableImg.rectTransform.anchoredPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition) * gameRoot.sizeDelta;
		}
    }

    public void BeginDrag()
	{
        if (!onCooldown)
        {
            draggableImg.enabled = true;
            draggableImg.sprite = ownImg.sprite;
            dragging = true;
        }
	}

    void OnReleaseMouse()
	{
        dragging = false;
        draggableImg.enabled = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Perform the raycast into the UI
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        FieldUI targetField = null;
        foreach(var r in results)
		{
            targetField = r.gameObject.transform.parent?.GetComponent<FieldUI>();
            if (targetField != null)
                break;
        }
        Debug.Log(GM.Instance.transform.parent?.transform);


        bool overField = targetField != null;
        if (overField) 
        {
            onCooldown = true;
            currentCooldownTime = COOLDOWN_TIME;
        }
    }
}
