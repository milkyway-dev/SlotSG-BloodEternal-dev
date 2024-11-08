using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotIconView : MonoBehaviour
{
    [Header("required fields")]
    [SerializeField] internal int pos;
    [SerializeField] internal Image iconImage;
    [SerializeField] internal Transform parent;
    [SerializeField] internal GameObject frontBorder;
    [SerializeField] internal Vector3 defaultPos;
    [SerializeField] internal ImageAnimation bgGlow;
    [SerializeField] internal int siblingIndex;
    [SerializeField] internal GameObject blastAnim;
    [SerializeField] internal GameObject bloodSplatter;
    [SerializeField] internal GameObject wildObject;

    void Start(){

        parent=transform.parent;
        defaultPos=transform.localPosition;
        siblingIndex=transform.GetSiblingIndex();
    }

}
