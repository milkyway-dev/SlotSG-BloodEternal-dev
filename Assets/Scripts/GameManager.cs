using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotBehaviour slotManager;
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

    // [Header("info text")]

    [SerializeField] private bool isSpinning;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private int autoSpinCount;
    [SerializeField] private bool isFreeSPin;
    [SerializeField] private double currentBalance;
    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;
    private Coroutine autoSpinRoutine;
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

    }

    void InitGame(List<string> LineIds)
    {
        for (int i = 0; i < LineIds.Count; i++)
        {
            slotManager.FetchLines(LineIds[i], i);
        }
        slotManager.SetInitialUI();
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
            yield return OnSpin();
        }
        yield return OnSpinEnd();
        yield return null;
    }
    bool OnSpinStart()
    {
        isSpinning = true;

        if (currentBalance < currentTotalBet  && !isFreeSPin)
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
        yield return slotManager.StopSpin();
        yield return OnSpinEnd();
    }
    IEnumerator OnSpinEnd()
    {

        if (!isAutoSpin && !isFreeSPin)
        {
            isSpinning = false;
            ToggleButtonGrp(true);
        }
        yield return null;
    }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (Maxbet_button) Maxbet_button.interactable = toggle;
        if (BetMinus_Button) BetMinus_Button.interactable = toggle;
        if (BetPlus_Button) BetPlus_Button.interactable = toggle;
    }

    private void OnchangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if (betCounter < 1)
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

        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[betCounter].ToString();
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[betCounter] * SocketManager.initialData.Lines.Count).ToString();
        // currentTotalBet = SocketManager.initialData.Bets[betCounter] * SocketManager.initialData.Lines.Count;
        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }

        private void MaxBet()
    {
        // if (audioController) audioController.PlayButtonAudio();
        // BetCounter = SocketManager.initialData.Bets.Count - 1;
        // if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count).ToString();
        // if (BetPerLine_text) BetPerLine_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        // currentTotalBet = SocketManager.initialData.Bets[BetCounter] * SocketManager.initialData.Lines.Count;
        if (currentBalance < currentTotalBet)
            uIManager.LowBalPopup();
    }

}
