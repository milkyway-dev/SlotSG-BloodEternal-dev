using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    [Header("required fields")]
    [SerializeField] internal Image iconImage;
    [SerializeField] internal Transform parent;
    [SerializeField] internal GameObject frontBorder;

    void Start(){

        parent=transform.parent;
    }

}
