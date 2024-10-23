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
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> slotMatrix;

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

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab

    [SerializeField]
    private PayoutCalculation PayCalculator;

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

    [SerializeField] private GlowMatrix[] glowMatrix;
    // [SerializeField] private ImageAnimation[] slotGlowRed;
    // [SerializeField] private ImageAnimation[] slotGlowBlue;
    private void Start()
    {

        shuffleInitialMatrix();

        // IsAutoSpin = false;
        // if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        // if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });


        // if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        // if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        // if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        // if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        // if (BetPlus_Button) BetPlus_Button.onClick.RemoveAllListeners();
        // if (BetPlus_Button) BetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });
        // if (BetMinus_Button) BetMinus_Button.onClick.RemoveAllListeners();
        // if (BetMinus_Button) BetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        // if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        // if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);
        tweenHeight = (16 * IconSizeFactor) - 280;
    }

    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    // [x]
    private void ChangeBet(bool IncDec)
    {
        // if (audioController) audioController.PlayButtonAudio();

        // if (IncDec)
        // {
        //     if (BetCounter < SocketManager.initialData.Bets.Count - 1)
        //     {
        //         BetCounter++;
        //     }
        // }
        // else
        // {
        //     if (BetCounter > 0)
        //     {
        //         BetCounter--;
        //     }
        // }

        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        // currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        // CompareBalance();
    }

    // [x]
    private void MaxBet()
    {
        // if (audioController) audioController.PlayButtonAudio();
        // BetCounter = SocketManager.initialData.Bets.Count - 1;
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        // currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        // CompareBalance();
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

    //just for testing purposes delete on production
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
        {
            StartSlots();
        }
    }

    // [x]
    internal void SetInitialUI()
    {
        // BetCounter = 0;
        // LineCounter = SocketManager.initialData.LinesCount.Count - 1;
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        // if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
        // if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        // if (Total_lines) Total_lines.text = SocketManager.initialData.Lines.Count.ToString();
        // currentBalance = SocketManager.playerdata.Balance;
        // currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        // CompareBalance();
        // uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    //function to populate animation sprites accordingly
    // private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    // {
    //     animScript.textureArray.Clear();
    //     animScript.textureArray.TrimExcess();
    //     switch (val)
    //     {
    //         case 8:
    //             for (int i = 0; i < Coin_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Coin_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;
    //         case 9:
    //             for (int i = 0; i < Frog_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Frog_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;
    //         case 6:
    //             for (int i = 0; i < Turtle_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Turtle_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;
    //         case 7:
    //             for (int i = 0; i < Cap_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Cap_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;
    //         case 5:
    //             for (int i = 0; i < Fish_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Fish_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;
    //         case 4:
    //             for (int i = 0; i < Ten_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Ten_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 29f;
    //             break;
    //         case 0:
    //             for (int i = 0; i < A_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(A_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 29f;
    //             break;
    //         case 3:
    //             for (int i = 0; i < J_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(J_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 30f;
    //             break;
    //         case 1:
    //             for (int i = 0; i < K_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(K_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 29f;
    //             break;
    //         case 2:
    //             for (int i = 0; i < Q_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Q_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 29f;
    //             break;

    //         case 10:
    //             for (int i = 0; i < Scatter_Sprite.Length; i++)
    //             {
    //                 animScript.textureArray.Add(Scatter_Sprite[i]);
    //             }
    //             animScript.AnimationSpeed = 25f;
    //             break;

    //     }
    // }

    //starts the spin process
    // [x]
    private void StartSlots(bool autoSpin = false)
    {
        WinningsAnim(false);
        if (!autoSpin)
        {
            if (audioController) audioController.PlayButtonAudio("spin");

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }

        }
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }


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

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {

        yield return new WaitForSeconds(0.1f);
        IsSpinning = true;
        // if (currentBalance < currentTotalBet)
        // {
        //     // CompareBalance();
        //     if (IsAutoSpin) {
        //         StopAutoSpin();
        //         yield return new WaitForSeconds(1f);
        //     }
        //     yield break;
        // }
        if (audioController) audioController.PlaySpinAudio();
        CheckSpinAudio = true;
        ToggleButtonGrp(false);

        // for (int i = 0; i < numberOfSlots; i++)
        // {
        //     InitializeTweening(Slot_Transform[i]);
        //     yield return new WaitForSeconds(0.1f);
        // }

        BalanceDeduction();

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitForSeconds(0.5f);


        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
                // PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // for (int i = 0; i < numberOfSlots; i++)
        // {
        //     yield return StopTweening(5, Slot_Transform[i], i);
        // }

        yield return new WaitForSeconds(0.5f);

        if (audioController) audioController.StopSpinAudio();

        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit);

        KillAllTweens();

        CheckPopups = true;

        currentBalance = SocketManager.playerdata.Balance;

        CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);

        //if (SocketManager.resultData.jackpot > 0)
        //{
        //    CheckPopups = true;
        //    uiManager.PopulateWin(4, SocketManager.resultData.jackpot);
        //}

        //yield return new WaitUntil(() => !CheckPopups);

        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.WinAmout.ToString("f2");
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        if (!IsAutoSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;

        }
        else
        {
            IsSpinning = false;
            yield return new WaitForSeconds(2f);
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

    internal IEnumerator StopSpin(float delay1 = 0, float delay2 = 0, List<int[]> vHPos=null)
    {

        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            if (delay1 > 0 && i == 2)
            {
                DeActivateReelBorder();
                ActivateReelBorder("red");
                yield return new WaitForSeconds(delay1);

            }

            if (delay2 > 0 && i == 4)
            {
                DeActivateReelBorder();
                ActivateReelBorder("blue");
                yield return new WaitForSeconds(delay2);
            }

            StopTweening(Slot_Transform[i], i);
            yield return new WaitForSeconds(0.25f);
            if (i > 0 && i < 5)
            {

                for (int k = 0; k < vHPos.Count; k++)
                {
                    if (vHPos[k][0] == i)
                    {
                        EnableIconGlow(vHPos[k]);
                    }

                }
            }

        }
        KillAllTweens();
        DeActivateReelBorder();

    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < slotMatrix.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                slotMatrix[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    // [x]
    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;

    }

    // [x]
    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        else
        {
            if (AutoSpin_Button) AutoSpin_Button.interactable = true;
            if (SlotStart_Button) SlotStart_Button.interactable = true;
        }
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        int i = animObjects.transform.childCount;

        if (i > 0)
        {
            ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
            animObjects.transform.GetChild(0).gameObject.SetActive(true);

            temp.StartAnimation();

            TempList.Add(temp);
        }
        else
        {
            animObjects.GetComponent<ImageAnimation>().StartAnimation();

        }
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
    internal void FreeSpinVHAnim(int index)
    {

        VHObjectsBlue[index].gameObject.SetActive(true);
        VHObjectsBlue[index].StartAnimation();
        VHObjectsBlue[index].transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.3f, 0, 1.2f); ;
    }

    internal void IconShakeAnim(int[] icon1, int[] icon2)
    {

        slotMatrix[icon1[0]].slotImages[icon1[1]].transform.DOShakePosition(1f, strength: new Vector3(20, 20, 0), vibrato: 20, randomness: 90, fadeOut: true);
        slotMatrix[icon2[0]].slotImages[icon2[1]].transform.DOShakePosition(1f, strength: new Vector3(20, 20, 0), vibrato: 20, randomness: 90, fadeOut: true);
    }

    void EnableIconGlow(int[] pos)
    {
            glowMatrix[pos[0]].row[pos[1]].gameObject.SetActive(true);
            glowMatrix[pos[0]].row[pos[1]].StartAnimation();

        
    }

    internal void DisableGlow()
    {
        for (int i = 0; i < glowMatrix.Length; i++)
        {
            for (int j = 0; j < glowMatrix[i].row.Count; j++)
            {
            glowMatrix[i].row[j].gameObject.SetActive(false);
            glowMatrix[i].row[j].StopAnimation();
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


    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0)
        {
            WinningsAnim(true);
            if (audioController) audioController.PlayWLAudio("win");

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(slotMatrix[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(slotMatrix[0].slotImages[points_anim[k]].gameObject);
                    }
                }
            }
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }
        CheckSpinAudio = false;
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        // slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = null;
        tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.45f).SetEase(Ease.InBack).OnComplete(() =>
        {
            tweener.Pause();
            slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -835f);
            tweener.Kill();
            tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.25f).SetLoops(-1, LoopType.Restart).SetDelay(0);
            alltweens.Add(tweener);

        });
        // tweener.Play();
    }



    private void StopTweening(Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, -330);
        int tweenpos = -835;
        alltweens[index] = slotTransform.DOLocalMoveY(tweenpos, 0.25f).SetEase(Ease.OutQuad); // slot initial pos - iconsizefactor - spacing

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
    public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public class GlowMatrix
{
    public List<ImageAnimation> row = new List<ImageAnimation>();
}

