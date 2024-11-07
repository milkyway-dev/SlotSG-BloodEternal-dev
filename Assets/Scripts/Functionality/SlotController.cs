using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotController : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] iconImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField] private List<SlotImage> slotMatrix;
    [SerializeField] private GlowMatrix[] glowMatrix;

    [SerializeField] internal GameObject disableIconsPanel;


    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;

    [SerializeField] private ImageAnimation secondarySlotPrefab;
    private Dictionary<int, string> x_string = new Dictionary<int, string>();
    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Maxbet_button;



    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private Button MaxBet_Button;
    [SerializeField] private Button BetPlus_Button;
    [SerializeField] private Button BetMinus_Button;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text BetPerLine_text;

    [SerializeField] private TMP_Text Total_lines;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;


    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _spinSound;
    [SerializeField]
    private AudioClip _lossSound;
    [SerializeField]
    private AudioClip[] _winSounds;

    [Header("tween properties")]
    [SerializeField] private int tweenHeight = 0;
    [SerializeField] private float initialPos;

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PaylineController PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing

    private int numberOfSlots = 5;          //number of columns



    [SerializeField] private SocketController SocketManager;
    [SerializeField] private UIManager uiManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine = null;
    private bool IsAutoSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    [SerializeField]
    private int spacefactor;

    private int BetCounter = 0;
    private int LineCounter = 0;

    internal bool CheckPopups;
    private double currentBalance = 0;
    private double currentTotalBet = 0;


    [SerializeField] private ImageAnimation[] VHObjectsBlue;
    [SerializeField] private ImageAnimation[] VHObjectsRed;

    [SerializeField] private ImageAnimation[] reel_border;

    // [SerializeField] private ImageAnimation[] slotGlowRed;
    // [SerializeField] private ImageAnimation[] slotGlowBlue;
    internal List<SlotIconView> animatedIcons = new List<SlotIconView>();

    private void Start()
    {

        // shuffleInitialMatrix();
    }





    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> x_points = null;
        List<int> y_points = null;
        x_points = x_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }



    // [x]


    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;

        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;
        balance = balance - (bet);

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });

    }

    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }



    // current
    internal IEnumerator StartSpin()
    {

        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

    }

    internal void PopulateSLotMatrix(List<List<int>> resultData)
    {
        for (int j = 0; j < slotMatrix[0].slotImages.Count; j++)
        {
            for (int i = 0; i < slotMatrix.Count; i++)
            {
                slotMatrix[i].slotImages[j].iconImage.sprite = iconImages[resultData[j][i]];
                if (resultData[j][i] == 11 || resultData[j][i] == 12 || resultData[j][i] == 13 || resultData[j][i] == 14)
                {

                    slotMatrix[i].slotImages[j].bgGlow.gameObject.SetActive(true);
                    slotMatrix[i].slotImages[j].bgGlow.StartAnimation();
                }

            }
        }
    }
    internal IEnumerator StopSpin(float delay1 = 0, float delay2 = 0,bool isFreeSpin=false,Action<bool> playReelGlowSound=null)
    {

        DeActivateReelBorder();
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            if (delay1 > 0 && i == 2 && !isFreeSpin)
            {
                ActivateReelBorder("red");
                playReelGlowSound?.Invoke(true);
                yield return new WaitForSeconds(delay1);
            }

            if (delay2 > 0 && i == 4 && !isFreeSpin)
            {
                playReelGlowSound?.Invoke(false);
                DeActivateReelBorder();
                ActivateReelBorder("blue");
                playReelGlowSound?.Invoke(true);
                yield return new WaitForSeconds(delay2);
            }

            StopTweening(Slot_Transform[i], i);
            playReelGlowSound?.Invoke(false);

            yield return new WaitForSeconds(0.25f);
        }
            playReelGlowSound?.Invoke(false);

        KillAllTweens();
        DeActivateReelBorder();

    }

    internal float[] CalculateDelay(List<List<int>> resultData)
    {
        float[] delay = new float[] { 0, 0 };

        for (int i = 0; i < resultData.Count; i++)
        {
            if (resultData[i][1] == 11)
                delay[0] = 1f;
            else if (resultData[i][3] == 13)
                delay[1] = 1f;

        }

        return delay;
    }


    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 10);
                slotMatrix[i].slotImages[j].iconImage.sprite = iconImages[randomIndex];
            }
        }
    }



    internal void StartIconBlastAnimation(List<string> iconPos, bool opposite = false)
    {
        // IconController tempIcon; 
        for (int j = 0; j < iconPos.Count; j++)
        {
            SlotIconView tempIcon;
            int[] pos = iconPos[j].Split(',').Select(int.Parse).ToArray();
            if (opposite)
                tempIcon = slotMatrix[pos[1]].slotImages[pos[0]];
            else
                tempIcon = slotMatrix[pos[0]].slotImages[pos[1]];

            tempIcon.blastAnim.SetActive(true);
            tempIcon.blastAnim.transform.DOScale(new Vector2(1.1f, 1.1f), 0.35f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                tempIcon.blastAnim.SetActive(false);
                tempIcon.frontBorder.SetActive(true);
            });
            tempIcon.transform.SetParent(disableIconsPanel.transform.parent);
            animatedIcons.Add(tempIcon);
        }

    }

    internal void ShowOnlyIcons(List<string> iconPos,bool activateFrontBorder=false)
    {

        for (int j = 0; j < iconPos.Count; j++)
        {
            int[] pos = iconPos[j].Split(',').Select(int.Parse).ToArray();
            SlotIconView tempIcon = slotMatrix[pos[0]].slotImages[pos[1]];
            if(activateFrontBorder)
            tempIcon.frontBorder.SetActive(true);
            tempIcon.transform.SetParent(disableIconsPanel.transform.parent);
            if(animatedIcons.Contains(tempIcon))
            animatedIcons.Add(tempIcon);
        }
    }
    internal void StopIconBlastAnimation()
    {

        foreach (var item in animatedIcons)
        {
            item.blastAnim.SetActive(false);
            item.blastAnim.transform.localScale *= 0;
            // item.frontBorder.SetActive(false);
            item.transform.SetParent(item.parent);

        }
        animatedIcons.Clear();
    }
    internal void StartIconAnimation(List<string> iconPos)
    {
        SlotIconView tempIcon = null;
        for (int j = 0; j < iconPos.Count; j++)
        {
            int[] pos = iconPos[j].Split(',').Select(int.Parse).ToArray();
            tempIcon = slotMatrix[pos[0]].slotImages[pos[1]];
            tempIcon.frontBorder.SetActive(true);
            tempIcon.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 0, 0.3f);
            tempIcon.transform.SetParent(disableIconsPanel.transform.parent);
            if(!animatedIcons.Contains(tempIcon))
            animatedIcons.Add(tempIcon);
        }
        // getAnimatedIcons?.Invoke(animatedIcons);

    }

    internal void ShowWildAndBloodANimation(List<string> iconPos)
    {

        for (int j = 0; j < iconPos.Count; j++)
        {
            int[] pos = iconPos[j].Split(',').Select(int.Parse).ToArray();
            SlotIconView tempIcon = slotMatrix[pos[1]].slotImages[pos[0]];
            tempIcon.bloodSplatter.transform.localScale *= 0;
            tempIcon.bloodSplatter.gameObject.SetActive(true);
            tempIcon.bloodSplatter.transform.DOScale(Vector3.one, 0.5f).OnComplete(()=>{
                tempIcon.wildObject.SetActive(true);
                tempIcon.wildObject.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 0, 0.3f);
            }).SetEase(Ease.OutExpo);

            if(!animatedIcons.Contains(tempIcon))
            animatedIcons.Add(tempIcon);
        }

    }

    internal void StopIconAnimation()
    {

        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < slotMatrix[i].slotImages.Count; j++)
            {
                slotMatrix[i].slotImages[j].bloodSplatter.gameObject.SetActive(false);
                slotMatrix[i].slotImages[j].transform.localScale = Vector3.one;
                slotMatrix[i].slotImages[j].frontBorder.SetActive(false);
                slotMatrix[i].slotImages[j].transform.SetParent(slotMatrix[i].slotImages[j].parent);
                slotMatrix[i].slotImages[j].transform.localPosition = slotMatrix[i].slotImages[j].defaultPos;
                slotMatrix[i].slotImages[j].transform.SetSiblingIndex(slotMatrix[i].slotImages[j].siblingIndex);
                slotMatrix[i].slotImages[j].wildObject.SetActive(false);
            }
        }

        // foreach (var item in animatedIcons)
        // {
        //     item.frontBorder.SetActive(false);
        //     item.transform.localScale = Vector3.one;
        //     item.transform.SetParent(item.parent);
        //     item.transform.localPosition = item.defaultPos;
        //     item.transform.SetSiblingIndex(item.siblingIndex);
        //     // item.bloodSplatter.transform.localScale = Vector3.one; 
        //     item.bloodSplatter.gameObject.SetActive(false);
        //     item.wildObject.SetActive(false);

        // }
        animatedIcons.Clear();
    }


    void ActivateReelBorder(string type)
    {
        if (type == "red")
        {
            reel_border[0].gameObject.SetActive(true);
            reel_border[0].StartAnimation();


        }
        else if (type == "blue")
        {
            reel_border[1].gameObject.SetActive(true);
            reel_border[1].StartAnimation();
        }

    }

    internal void DeActivateReelBorder()
    {
        for (int i = 0; i < reel_border.Length; i++)
        {
            reel_border[i].gameObject.SetActive(false);
            reel_border[i].StopAnimation();
        }


    }
    internal void FreeSpinVHAnim(List<string> pos, ref List<ImageAnimation> VHcombo)
    {
        for (int i = 0; i < pos.Count; i++)
        {
            if (i % 2 != 0) continue;

            int[] iconPos = pos[i].Split(',').Select(int.Parse).ToArray();
            if (iconPos[1] == 1)
            {
                VHObjectsRed[iconPos[0]].gameObject.SetActive(true);
                VHObjectsRed[iconPos[0]].StartAnimation();
                VHObjectsRed[iconPos[0]].transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.3f, 0, 1.2f);
                VHcombo.Add(VHObjectsRed[iconPos[0]]);
            }
            else if (iconPos[1] == 3)
            {
                VHObjectsBlue[iconPos[0]].gameObject.SetActive(true);
                VHObjectsBlue[iconPos[0]].StartAnimation();
                VHObjectsBlue[iconPos[0]].transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.3f, 0, 1.2f);
                VHcombo.Add(VHObjectsBlue[iconPos[0]]);

            }

        }

    }

    internal void IconShakeAnim(List<string> vhPos)
    {
        int[] pos;
        for (int i = 0; i < vhPos.Count; i++)
        {
            pos = vhPos[i].Split(',').Select(int.Parse).ToArray();
            slotMatrix[pos[1]].slotImages[pos[0]].transform.DOShakePosition(1f, strength: new Vector3(25, 25, 0), vibrato: 20, randomness: 90, fadeOut: true);

        }
    }

    void EnableIconGlow(int[] pos)
    {
        slotMatrix[pos[0]].slotImages[pos[1]].bgGlow.gameObject.SetActive(true);
        slotMatrix[pos[0]].slotImages[pos[1]].bgGlow.StartAnimation();

    }

    internal void DisableGlow()
    {
        for (int i = 0; i < glowMatrix.Length; i++)
        {
            for (int j = 0; j < glowMatrix[i].row.Count; j++)
            {
                slotMatrix[i].slotImages[j].bgGlow.gameObject.SetActive(false);
                slotMatrix[i].slotImages[j].bgGlow.StopAnimation();
            }

        }
    }
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
            if (TempList[i].transform.childCount > 0)
                TempList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        // slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = null;
        tweener = slotTransform.DOLocalMoveY(-tweenHeight + 350, 0.35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            tweener.Pause();
            slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -835f);
            tweener.Kill();
            tweener = slotTransform.DOLocalMoveY(-tweenHeight + 350, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
            alltweens.Add(tweener);

        });
        // tweener.Play();
    }

    private void StopTweening(Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, initialPos + 500);
        alltweens[index] = slotTransform.DOLocalMoveY(initialPos, 0.25f).SetEase(Ease.OutQuad); // slot initial pos - iconsizefactor - spacing

    }


    private void KillAllTweens()
    {
        for (int i = 0; i < alltweens.Count; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<SlotIconView> slotImages = new List<SlotIconView>(10);
}

[Serializable]
public class GlowMatrix
{
    public List<ImageAnimation> row = new List<ImageAnimation>();
}

