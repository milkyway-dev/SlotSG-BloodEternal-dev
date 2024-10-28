using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotIconView : MonoBehaviour
{
    [Header("required fields")]
    [SerializeField] internal Image iconImage;
    [SerializeField] internal Transform parent;
    [SerializeField] internal GameObject frontBorder;

    [SerializeField] internal GameObject blastAnim;
    [SerializeField] internal GameObject bloodSplatter;
    [SerializeField] internal GameObject wildObject;

    void Start(){

        parent=transform.parent;
    }

}
