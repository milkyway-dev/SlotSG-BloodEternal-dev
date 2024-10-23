using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotController slotManager;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private SocketController socketController;
    [SerializeField] private AudioController audioController;
    [SerializeField] private PayoutCalculation PayCalculator;

    [Header("Action Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button Maxbet_button;
    [SerializeField] private Button BetMinus_Button;
    [SerializeField] private Button BetPlus_Button;

    [Header("info text")]
    [SerializeField] private TMP_Text betPerLine_text;
    [SerializeField] private TMP_Text totalBet_text;

    [SerializeField] private bool isSpinning;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private int autoSpinCount;
    [SerializeField] private bool isFreeSpin;
    [SerializeField] private bool freeSpinStarted;
    [SerializeField] private double currentBalance;
    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;
    private Coroutine autoSpinRoutine;

    [SerializeField] private int wildPosition;

    void Awake()
    {

    }
    void Start()
    {
        socketController.OnInit = InitGame;
        socketController.ShowDisconnectionPopup = uIManager.DisconnectionPopup;
        socketController.OpenSocket();

        SlotStart_Button.onClick.RemoveAllListeners();
        SlotStart_Button.onClick.AddListener(ExecuteSpin);
        AutoSpin_Button.onClick.RemoveAllListeners();
        AutoSpin_Button.onClick.AddListener(ExecuteAutoSpin);
        AutoSpinStop_Button.onClick.AddListener(() =>
        {

            StartCoroutine(StopAutoSpinCoroutine());
        });

        BetPlus_Button.onClick.AddListener(() => { OnBetChange(true); });
        BetMinus_Button.onClick.AddListener(() => { OnBetChange(false); });
        Maxbet_button.onClick.AddListener(MaxBet);

    }

    void InitGame()
    {
        // for (int i = 0; i < LineIds.Count; i++)
        // {
        //     slotManager.FetchLines(LineIds[i], i);
        // }

        betCounter = 0;
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;

        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        Debug.Log(socketController.socketModel.initGameData.Bets[betCounter]);
        if (betPerLine_text) betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
        Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");

    }


    void ExecuteSpin()
    {

        StartCoroutine(SpinRoutine());
    }

    void ExecuteAutoSpin()
    {
        if (!isSpinning && autoSpinCount > 0)
        {

            isAutoSpin = true;
            AutoSpin_Button.gameObject.SetActive(false);
            AutoSpinStop_Button.gameObject.SetActive(true);
            autoSpinRoutine = StartCoroutine(AutoSpinRoutine());
        }

    }
    IEnumerator AutoSpinRoutine()
    {
        while (autoSpinCount > 0 && isAutoSpin)
        {
            autoSpinCount--;
            yield return SpinRoutine();
            yield return new WaitForSeconds(1);
        }
        isAutoSpin = false;
        isSpinning = false;
        ToggleButtonGrp(true);
        yield return null;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        isAutoSpin = false;
        AutoSpin_Button.gameObject.SetActive(true);
        AutoSpinStop_Button.gameObject.SetActive(false);
        yield return new WaitUntil(() => !isSpinning);
        if (autoSpinRoutine != null)
        {
            StopCoroutine(autoSpinRoutine);
            autoSpinRoutine = null;
        }
        ToggleButtonGrp(true);


    }
    IEnumerator SpinRoutine()
    {
        if (OnSpinStart())
        {
            // var spinData = new { data = new { currentBet = betCounter, currentLines = 30, spins = 1 }, id = "SPIN" };
            // socketController.SendData("message", spinData);
            if (isFreeSpin && !freeSpinStarted)
            {
                freeSpinStarted = true;
                yield return InitiateFreeSpin();
            }
            yield return OnSpin();
        }
        yield return OnSpinEnd();
        yield return null;
    }
    bool OnSpinStart()
    {
        isSpinning = true;

        if (currentBalance < currentTotalBet && !isFreeSpin)
        {
            uIManager.LowBalPopup();
            return false;
        }
        if (audioController) audioController.PlaySpinAudio();
        PayCalculator.ResetLines();
        ToggleButtonGrp(false);
        return true;
    }

    IEnumerator OnSpin()
    {
        yield return slotManager.StartSpin();
        yield return new WaitForSeconds(2f);
        List<int[]> vHPos = new List<int[]>
            {
                new int[] { 1, 0 },
                new int[] { 1, 1 }
            };
        yield return slotManager.StopSpin(2, 0,vHPos);

        yield return OnSpinEnd();
    }
    IEnumerator OnSpinEnd()
    {
        slotManager.DeActivateReelBorder();
        if (!isAutoSpin && !isFreeSpin)
        {
            slotManager.DisableGlow();
            isSpinning = false;
            ToggleButtonGrp(true);
        }
        yield return null;
    }
    IEnumerator InitiateFreeSpin()
    {
        slotManager.IconShakeAnim(new int[] { 1, 1 }, new int[] { 2, 1 });
        yield return new WaitForSeconds(1f);
        slotManager.FreeSpinVHAnim(1);
    }
    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
    }

    private void OnBetChange(bool inc)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (inc)
        {
            if (betCounter < socketController.socketModel.initGameData.Bets.Count - 1)
            {
                betCounter++;
            }
        }
        else
        {
            if (betCounter > 0)
            {
                betCounter--;
            }
        }

        Debug.Log("adsasdad+ " + socketController.socketModel.initGameData.Bets.Count);
        if (betPerLine_text) betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;
        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();

        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();

        betCounter = socketController.socketModel.initGameData.Bets.Count - 1;
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;

        totalBet_text.text = currentTotalBet.ToString();
        betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();

        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }


}
