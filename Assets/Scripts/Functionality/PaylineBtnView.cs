using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaylineBtnView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int ID;
    [SerializeField] private TMP_Text id_text;
    [SerializeField] internal bool active = true;

    internal Action<int> OnHover;
    internal Action<bool> OnExit;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!active)
            return;
        OnHover?.Invoke(ID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!active)
            return;
        OnExit?.Invoke(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
        {
            if (!active)
                return;
            OnHover?.Invoke(ID);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
        {
            //Debug.Log("run on pointer up");
            if (!active)
                return;
            OnExit?.Invoke(false);
        }
    }
    internal void SetIdAndText(int id)
    {
        ID = id;
        id_text.text = (ID + 1).ToString();

    }
}
