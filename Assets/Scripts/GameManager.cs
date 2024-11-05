using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotController slotManager;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private SocketController socketController;
    [SerializeField] private AudioController audioController;
    [SerializeField] private PaylineController PayLineCOntroller;

    [Header("For spins")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button Maxbet_button;
    [SerializeField] private Button BetMinus_Button;
    [SerializeField] private Button BetPlus_Button;
    [SerializeField] private TMP_Text betPerLine_text;
    [SerializeField] private TMP_Text totalBet_text;
    [SerializeField] private bool isSpinning;

    [Header("For auto spins")]
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private TMP_Text AUtoSpinCountText;
    [SerializeField] TMP_Dropdown autoSpinDropDown;
    [SerializeField] TMP_Dropdown betPerLineDropDown;
    [SerializeField] private bool isAutoSpin;
    [SerializeField] private int autoCounter = 0;
    [SerializeField] private int autoSpinCounter;
    List<int> autoOptions = new List<int>() { 15, 20, 25, 30, 40, 100 };

    [Header("For Gamble")]
    [SerializeField] private Button Double_Button;
    [SerializeField] private Button Head_option;
    [SerializeField] private Button Tail_Option;
    [SerializeField] private Button Collect_Option;
    [SerializeField] private Button allGambleButton;
    [SerializeField] private Button halfGambleButton;
    [SerializeField] private GameObject gambleObject;
    [SerializeField] private ImageAnimation coinAnim;
    [SerializeField] private string option;
    public double currenwinning;
    [SerializeField] private bool isFreeSpin;
    [SerializeField] private bool freeSpinStarted;
    [SerializeField] private double currentBalance;
    [SerializeField] private double currentTotalBet;
    [SerializeField] private int betCounter = 0;




    private Coroutine autoSpinRoutine;

    private Coroutine freeSpinRoutine;
    private Coroutine iterativeRoutine;
    [SerializeField] private int wildPosition;
    [SerializeField] private int maxIterationWinShow;
    [SerializeField] private int winIterationCount;

    [SerializeField] private int freeSpinCount;
    void Start()
    {
        slotManager.shuffleInitialMatrix();
        socketController.OnInit = InitGame;
        socketController.ShowDisconnectionPopup = uIManager.DisconnectionPopup;
        socketController.OpenSocket();
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        SlotStart_Button.onClick.AddListener(ExecuteSpin);
        AutoSpin_Button.onClick.AddListener(ExecuteAutoSpin);
        AutoSpinStop_Button.onClick.AddListener(() => StartCoroutine(StopAutoSpinCoroutine()));
        BetPlus_Button.onClick.AddListener(() => { OnBetChange(true); });
        BetMinus_Button.onClick.AddListener(() => { OnBetChange(false); });
        Maxbet_button.onClick.AddListener(MaxBet);
        Double_Button.onClick.AddListener(OnInitGamble);
        Head_option.onClick.AddListener(() => StartCoroutine(OnSelectGamble("HEAD")));
        Tail_Option.onClick.AddListener(() => StartCoroutine(OnSelectGamble("TAIL")));
        Collect_Option.onClick.AddListener(() => StartCoroutine(OnGambleCollect()));

        allGambleButton.onClick.AddListener(() => changeGambleType(true));
        halfGambleButton.onClick.AddListener(() => changeGambleType(false));

        autoSpinDropDown.onValueChanged.AddListener((int index) =>
        {
            autoSpinCounter = index;
            CalculateCost();
        });

        betPerLineDropDown.onValueChanged.AddListener((int index) =>
        {
            betCounter = index;
            CalculateCost();
        });
    }

    void InitGame()
    {

        betCounter = 0;
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;

        if (totalBet_text) totalBet_text.text = currentTotalBet.ToString();
        if (betPerLine_text) betPerLine_text.text = socketController.socketModel.initGameData.Bets[betCounter].ToString();
        PayLineCOntroller.paylines = socketController.socketModel.initGameData.lineData;
        uIManager.UpdatePlayerInfo(socketController.socketModel.playerData);
        uIManager.PopulateSymbolsPayout(socketController.socketModel.uIData.symbols);
        PopulateAutoSpinDropDown();
        PopulateBetPerlineDropDown();
        CalculateCost();
        Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");

    }


    void ExecuteSpin() => StartCoroutine(SpinRoutine());


    void ExecuteAutoSpin()
    {
        if (!isSpinning && autoSpinCounter > 0)
        {

            isAutoSpin = true;
            // AutoSpin_Button.gameObject.SetActive(false);
            AutoSpinStop_Button.gameObject.SetActive(true);
            autoSpinRoutine = StartCoroutine(AutoSpinRoutine());
        }

    }

    IEnumerator FreeSpinRoutine()
    {
        while (freeSpinCount > 0)
        {
            freeSpinCount--;
            yield return SpinRoutine();
            yield return new WaitForSeconds(1);
        }
        isAutoSpin = false;
        isSpinning = false;
        isFreeSpin = false;
        ToggleButtonGrp(true);
        yield return null;
    }
    IEnumerator AutoSpinRoutine()
    {
        while (autoSpinCounter > 0 && isAutoSpin)
        {
            autoSpinCounter--;
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
            var spinData = new { data = new { currentBet = betCounter, currentLines = 30, spins = 1 }, id = "SPIN" };
            socketController.SendData("message", spinData);
            yield return OnSpin();
        }
        yield return OnSpinEnd();

        if (socketController.socketModel.resultGameData.isFreeSpin)
        {
            if (freeSpinRoutine != null)
                StopCoroutine(freeSpinRoutine);

            isFreeSpin = true;
            freeSpinCount = socketController.socketModel.resultGameData.count;
            yield return InitiateFreeSpin(socketController.socketModel.resultGameData.vampHuman);
            freeSpinRoutine = StartCoroutine(FreeSpinRoutine());
            yield break;
        }
        if (!isAutoSpin && !isFreeSpin)
        {
            slotManager.DisableGlow();
            isSpinning = false;
            ToggleButtonGrp(true);
        }

    }
    bool OnSpinStart()
    {
        isSpinning = true;
        winIterationCount = 0;
        slotManager.StopIconAnimation();
        PayLineCOntroller.ResetLines(true);
        if (iterativeRoutine != null)
            StopCoroutine(iterativeRoutine);
        slotManager.disableIconsPanel.SetActive(false);
        if (currentBalance < currentTotalBet && !isFreeSpin)
        {
            uIManager.LowBalPopup();
            return false;
        }
        if (audioController) audioController.PlaySpinAudio();
        ToggleButtonGrp(false);
        return true;
    }

    IEnumerator OnSpin()
    {
        yield return slotManager.StartSpin();
        yield return new WaitUntil(() => socketController.isResultdone);
        slotManager.PopulateSLotMatrix(socketController.socketModel.resultGameData.ResultReel);
        // yield return new WaitForSeconds(2f);
        float[] delay = slotManager.CalculateDelay(socketController.socketModel.resultGameData.ResultReel);
        yield return slotManager.StopSpin(delay[0], delay[1]);

    }
    IEnumerator OnSpinEnd()
    {
        slotManager.DeActivateReelBorder();

        freeSpinCount = socketController.socketModel.resultGameData.count;
        if (socketController.socketModel.resultGameData.symbolsToEmit.Count > 0 && socketController.socketModel.resultGameData.count == 0)
        {

            slotManager.disableIconsPanel.SetActive(true);

            slotManager.StartIconBlastAnimation(Helper.FlattenSymbolsToEmit(socketController.socketModel.resultGameData.symbolsToEmit));
            yield return new WaitForSeconds(0.6f);
            slotManager.StopIconBlastAnimation();
            yield return new WaitForSeconds(0.1f);
        }


        uIManager.UpdatePlayerInfo(socketController.socketModel.playerData);



        if (!isAutoSpin && !isFreeSpin && socketController.socketModel.resultGameData.symbolsToEmit.Count > 1)
        {
            iterativeRoutine = StartCoroutine(IterativeWinShowRoutine(socketController.socketModel.resultGameData.symbolsToEmit));
            yield return iterativeRoutine;
        }

        if (isFreeSpin)
        {
            // slotManager.StartIconBlastAnimation(socketController.socketModel.resultGameData.bloodSplash);
            // yield return new WaitForSeconds(0.6f);
            // slotManager.StopIconBlastAnimation();
            slotManager.ShowWildAndBloodANimation(socketController.socketModel.resultGameData.bloodSplash);
            // slotManager.ShowOnlyIcons(socketController.socketModel.resultGameData.bloodSplash);
            for (int i = 0; i < socketController.socketModel.resultGameData.linesToEmit.Count; i++)
            {

                PayLineCOntroller.GeneratePayline(socketController.socketModel.resultGameData.linesToEmit[i] - 1, false);
            }
            yield return new WaitForSeconds(1);
        }
        else
        {
            slotManager.ShowOnlyIcons(Helper.FlattenSymbolsToEmit(socketController.socketModel.resultGameData.symbolsToEmit));
            for (int i = 0; i < socketController.socketModel.resultGameData.linesToEmit.Count; i++)
            {

                PayLineCOntroller.GeneratePayline(socketController.socketModel.resultGameData.linesToEmit[i] - 1, false);
            }
        }





    }

    private IEnumerator IterativeWinShowRoutine(List<List<string>> symbolsToEmit)
    {
        winIterationCount = maxIterationWinShow;
        while (winIterationCount > 0)
        {
            for (int i = 0; i < symbolsToEmit.Count; i++)
            {
                if (winIterationCount > 0)
                {
                    slotManager.StartIconAnimation(symbolsToEmit[i]);
                    if(socketController.socketModel.resultGameData.linesToEmit.Count>0){

                    PayLineCOntroller.GeneratePayline(socketController.socketModel.resultGameData.linesToEmit[i] - 1);
                    yield return new WaitForSeconds(0.8f);
                    PayLineCOntroller.ResetLines();
                    }
                    slotManager.StopIconAnimation();
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    yield break;
                }
            }
            winIterationCount--;
            yield return null;
        }
        slotManager.StopIconAnimation();
        // slotManager.disableIconsPanel.SetActive(false);
    }

    IEnumerator InitiateFreeSpin(List<string> VHPos)
    {
        slotManager.disableIconsPanel.SetActive(false);
        slotManager.IconShakeAnim(VHPos);
        yield return new WaitForSeconds(1f);
        slotManager.StartIconBlastAnimation(VHPos, true);
        yield return new WaitForSeconds(0.15f);
        slotManager.FreeSpinVHAnim(VHPos);
        yield return new WaitForSeconds(0.3f);
    }

    void OnInitGamble()
    {
        object gambleInitData = new { data = new { }, id = "GAMBLEINIT" };
        socketController.SendData("message", gambleInitData);
        changeGambleType(true);
        uIManager.UpdategambleInfo(currenwinning);
        gambleObject.SetActive(true);


    }
    IEnumerator OnSelectGamble(string type)
    {
        ToggleGambleBtnGrp(false);
        coinAnim.StartAnimation();
        object gambleResData = new { data = new { selected = type, gambleOption = option }, id = "GAMBLERESULT" };
        socketController.SendData("message", gambleResData);
        yield return new WaitUntil(() => socketController.isResultdone);
        uIManager.UpdategambleInfo(socketController.socketModel.gambleData.currentWinning);
        if (socketController.socketModel.gambleData.currentWinning <= 0)
        {
            yield return OnGambleCollect();
            yield break;
        }
        coinAnim.StopAnimation();
        ToggleGambleBtnGrp(true);

    }

    IEnumerator OnGambleCollect()
    {
        ToggleGambleBtnGrp(false);
        object gambleResData = new { data = new { }, id = "GAMBLECOLLECT" };
        socketController.SendData("message", gambleResData);
        yield return new WaitUntil(() => socketController.isResultdone);
        PlayerData playerData = new PlayerData();
        playerData.currentWining = socketController.socketModel.gambleData.currentWinning;
        playerData.Balance = socketController.socketModel.gambleData.balance;
        uIManager.UpdatePlayerInfo(playerData);
        gambleObject.SetActive(false);
        ToggleGambleBtnGrp(true);


    }

    internal void changeGambleType(bool full)
    {

        if (full)
        {
            halfGambleButton.transform.GetChild(0).gameObject.SetActive(false);
            allGambleButton.transform.GetChild(0).gameObject.SetActive(true);
            option = "ALL";

            uIManager.UpdategambleInfo(currenwinning);

        }
        else
        {
            halfGambleButton.transform.GetChild(0).gameObject.SetActive(true);
            allGambleButton.transform.GetChild(0).gameObject.SetActive(false);
            option = "HALF";
            uIManager.UpdategambleInfo(currenwinning, true);

        }

    }

    void ToggleGambleBtnGrp(bool toggle)
    {
        Head_option.interactable = toggle;
        Tail_Option.interactable = toggle;
        Collect_Option.interactable = toggle;
        allGambleButton.interactable = toggle;
        halfGambleButton.interactable = toggle;

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

    private void PopulateAutoSpinDropDown()
    {
        autoSpinDropDown.ClearOptions();
        List<string> autoOptionsString = new List<string>();

        for (int i = 0; i < autoOptions.Count; i++)
        {
            autoOptionsString.Add(autoOptions[i].ToString());
        }
        autoSpinDropDown.AddOptions(autoOptionsString);
        autoSpinDropDown.value = 0;
        autoSpinDropDown.RefreshShownValue();
    }

    private void PopulateBetPerlineDropDown()
    {

        betPerLineDropDown.ClearOptions();

        List<string> betOptionsString = new List<string>();

        for (int i = 0; i < socketController.socketModel.initGameData.Bets.Count; i++)
        {
            betOptionsString.Add(socketController.socketModel.initGameData.Bets[i].ToString());
        }

        betPerLineDropDown.AddOptions(betOptionsString);

        betPerLineDropDown.value = 0;
        betPerLineDropDown.RefreshShownValue();

    }



    private double CalculateCost()
    {
        currentTotalBet = socketController.socketModel.initGameData.Bets[betCounter] * socketController.socketModel.initGameData.lineData.Count;
        uIManager.UpdateAutoSpinCost(currentTotalBet * autoOptions[autoSpinCounter]);

        return 0;
    }

}
