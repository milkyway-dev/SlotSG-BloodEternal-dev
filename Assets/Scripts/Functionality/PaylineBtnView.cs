using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaylineBtnView : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [SerializeField] private int ID;
    [SerializeField] private TMP_Text id_text;
    [SerializeField] internal bool enabled=true;

    internal Action<int> OnHover;
    internal Action<bool> OnExit;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!enabled)
        return;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!enabled)
        return;
        OnHover?.Invoke(ID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!enabled)
        return;
        OnExit?.Invoke(false);
    }

    internal void SetIdAndText(int id){
        ID=id;
        id_text.text=(ID+1).ToString();

    }
}
