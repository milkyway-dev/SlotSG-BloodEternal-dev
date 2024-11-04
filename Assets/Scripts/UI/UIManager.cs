using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Button About_Button;

    [Header("Auto SpinPopup")]
    [SerializeField] private Button AutoSpinButton;
    [SerializeField] private Button AutoSpinPopUpClose;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("About Popup")]
    [SerializeField]
    private GameObject AboutPopup_Object;
    [SerializeField]
    private Button AboutExit_Button;

    [Header("Gambel texts")]
    [SerializeField] private TMP_Text bank;
    [SerializeField] private TMP_Text bet;
    [SerializeField] private TMP_Text potentialWin;

    [Header("Paytable Texts")]
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text Scatter_Text;
    [SerializeField] private TMP_Text Wild_Text;


    [Header("Settings Popup")]
    [SerializeField] private GameObject SettingsPopup_Object;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;
    [SerializeField] private Button SoundOn_Button;
    [SerializeField] private Button SoundOff_Button;
    [SerializeField] private Button MusicOn_Button;
    [SerializeField] private Button MusicOff_Button;

    [Header("all Win Popup")]
    [SerializeField]
    private Sprite BigWin_Sprite;
    [SerializeField]
    private Sprite HugeWin_Sprite;
    [SerializeField]
    private Sprite MegaWin_Sprite;
    [SerializeField]
    private Image Win_Image;
    [SerializeField]
    private GameObject WinPopup_Object;
    [SerializeField]
    private TMP_Text Win_Text;

    [Header("jackpot Win Popup")]
    [SerializeField] private TMP_Text jackpot_Text;
    [SerializeField] private GameObject jackpot_Object;

    [Header("low balance popup")]
    [SerializeField] private GameObject LowBalancePopup_Object;
    [SerializeField] private Button Close_Button;

    [Header("Scripts")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private SlotController slotManager;
    [SerializeField] private SocketController socketManager;

    [Header("disconnection popup")]
    [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;

    [Header("Quit Popup")]
    [SerializeField] private GameObject QuitPopupObject;
    [SerializeField] private Button yes_Button;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button no_Button;

    [Header("Splash Screen")]
    [SerializeField] private GameObject spalsh_screen;
    [SerializeField] private Image progressbar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField]
    private Button QuitSplash_button;

    [Header("AnotherDevice Popup")]
    [SerializeField] private Button CloseAD_Button;
    [SerializeField] private GameObject ADPopup_Object;

    [Header("Pagination")]
    int CurrentIndex = 0;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private Button RightBtn;
    [SerializeField] private Button LeftBtn;

    [SerializeField]
    private Button m_AwakeGameButton;
    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    [Header("player texts")]
    [SerializeField] private TMP_Text playerCurrentWinning;
    [SerializeField] private TMP_Text playerBalance;

    private void Awake()
    {
        //if (spalsh_screen) spalsh_screen.SetActive(true);
        //StartCoroutine(LoadingRoutine());
        SimulateClickByDefault();
    }

    private void SimulateClickByDefault()
    {
        Debug.Log("Awaken The Game...");
        m_AwakeGameButton.onClick.AddListener(() => { Debug.Log("Called The Game..."); });
        m_AwakeGameButton.onClick.Invoke();
    }

    private void Start()
    {
        // Set up each button with the appropriate action
        SetButton(yes_Button, CallOnExitFunction);
        SetButton(no_Button, () => { if (!isExit) { ClosePopup(QuitPopupObject); } });
        SetButton(GameExit_Button, () => OpenPopup(QuitPopupObject));
        SetButton(About_Button, () => OpenPopup(AboutPopup_Object));
        SetButton(AboutExit_Button, () => ClosePopup(AboutPopup_Object));
        SetButton(Settings_Button, () => OpenPopup(SettingsPopup_Object));
        SetButton(SettingsExit_Button, () => ClosePopup(SettingsPopup_Object));
        SetButton(MusicOn_Button, ToggleMusic);
        SetButton(MusicOff_Button, ToggleMusic);
        SetButton(SoundOn_Button, ToggleSound);
        SetButton(SoundOff_Button, ToggleSound);
        SetButton(LeftBtn, () => Slide(-1));
        SetButton(RightBtn, () => Slide(1));
        SetButton(CloseDisconnect_Button, CallOnExitFunction);
        SetButton(Close_Button, CallOnExitFunction);
        SetButton(QuitSplash_button, () => OpenPopup(QuitPopupObject));

        // Initialize other settings
        paytableList[CurrentIndex = 0].SetActive(true);
        isMusic = false;
        isSound = false;
        ToggleMusic();
        ToggleSound();
    }

    private void SetButton(Button button, Action action)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {

            action?.Invoke();

        });
    }

    internal void PopulateWin(int value, double amount)
    {
        switch (value)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;

        }

        if (value == 4)
            StartPopupAnim(amount, true);
        else
            StartPopupAnim(amount, false);

    }

    private IEnumerator LoadingRoutine()
    {
        StartCoroutine(LoadingTextAnimate());
        float fillAmount = 0.7f;
        progressbar.DOFillAmount(fillAmount, 2f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(2f);
        yield return new WaitUntil(() => !socketManager.isLoading);
        progressbar.DOFillAmount(1, 1f).SetEase(Ease.Linear);
        yield return new WaitForSecondsRealtime(1f);
        if (spalsh_screen) spalsh_screen.SetActive(false);
        StopCoroutine(LoadingTextAnimate());
    }

    internal void InitiateGamble(double currentWinnings){
        bank.text=currentWinnings.ToString();
        bet.text=currentWinnings.ToString();
        potentialWin.text=(currentWinnings*2).ToString();
        // gambleObject.SetActive(true);
    }

    internal void UpdategambleInfo(double currentWinnings){
        bank.text=currentWinnings.ToString();
        bet.text=currentWinnings.ToString();
        potentialWin.text=(currentWinnings*2).ToString();

    }
    internal void OnCollect(){
    // gambleObject.SetActive(false);
    }


    internal void UpdatePlayerInfo(PlayerData playerData)
    {
        playerCurrentWinning.text = playerData.currentWining.ToString();
        playerBalance.text = playerData.Balance.ToString();

    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (loadingText) loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.5f);
            if (loadingText) loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void StartPopupAnim(double amount, bool jackpot = false)
    {
        int initAmount = 0;
        if (jackpot)
        {
            if (jackpot_Object) jackpot_Object.SetActive(true);
        }
        else
        {
            if (WinPopup_Object) WinPopup_Object.SetActive(true);

        }

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, (int)amount, 5f).OnUpdate(() =>
        {
            if (jackpot)
            {
                if (jackpot_Text) jackpot_Text.text = initAmount.ToString();
            }
            else
            {
                if (Win_Text) Win_Text.text = initAmount.ToString();

            }
        });

        DOVirtual.DelayedCall(6f, () =>
        {
            if (jackpot)
            {
                ClosePopup(jackpot_Object);

            }
            else
            {
                ClosePopup(WinPopup_Object);
            }
            slotManager.CheckPopups = false;
        });
    }

    internal void LowBalPopup()
    {

        OpenPopup(LowBalancePopup_Object);
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        PopulateSymbolsPayout(symbolsText);
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        // for (int i = 0; i < SymbolsText.Length; i++)
        // {
        //     string text = null;
        //     if (paylines.symbols[i].Multiplier[0][0] != 0)
        //     {
        //         text += "5x - " + paylines.symbols[i].Multiplier[0][0];
        //     }
        //     if (paylines.symbols[i].Multiplier[1][0] != 0)
        //     {
        //         text += "\n4x - " + paylines.symbols[i].Multiplier[1][0];
        //     }
        //     if (paylines.symbols[i].Multiplier[2][0] != 0)
        //     {
        //         text += "\n3x - " + paylines.symbols[i].Multiplier[2][0];
        //     }
        //     if (SymbolsText[i]) SymbolsText[i].text = text;
        // }

        for (int i = 0; i < paylines.symbols.Count; i++)
        {

            if (paylines.symbols[i].Name.ToUpper() == "SCATTER")
            {
                if (Scatter_Text) Scatter_Text.text = paylines.symbols[i].description.ToString();
            }

            if (paylines.symbols[i].Name.ToUpper() == "WILD")
            {
                if (Wild_Text) Wild_Text.text = paylines.symbols[i].description.ToString();
            }
        }
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        socketManager.CloseSocket();
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        paytableList[CurrentIndex = 0].SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
        paytableList[CurrentIndex].SetActive(false);
    }



    private void Slide(int direction)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (CurrentIndex < paytableList.Length - 1 && direction > 0)
        {
            // Move to the next item
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex + 1].SetActive(true);

            CurrentIndex++;
        }
        else if (CurrentIndex >= 1 && direction < 0)
        {

            // Move to the previous item
            paytableList[CurrentIndex].SetActive(false);
            paytableList[CurrentIndex - 1].SetActive(true);

            CurrentIndex--;
        }
        if (CurrentIndex == paytableList.Length - 1)
        {
            RightBtn.interactable = false;
        }
        else
        {
            RightBtn.interactable = true;

        }
        if (CurrentIndex == 0)
        {
            LeftBtn.interactable = false;
        }
        else
        {
            LeftBtn.interactable = true;
        }

    }

    internal void DisconnectionPopup()
    {
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
    }

    private void ToggleMusic()
    {
        isMusic = !isMusic;
        if (isMusic)
        {
            if (MusicOn_Button) MusicOn_Button.interactable = false;
            if (MusicOff_Button) MusicOff_Button.interactable = true;
            audioController.ToggleMute(false, "bg");
        }
        else
        {
            if (MusicOn_Button) MusicOn_Button.interactable = true;
            if (MusicOff_Button) MusicOff_Button.interactable = false;
            audioController.ToggleMute(true, "bg");
        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            if (SoundOn_Button) SoundOn_Button.interactable = false;
            if (SoundOff_Button) SoundOff_Button.interactable = true;
            if (audioController) audioController.ToggleMute(false, "button");
            if (audioController) audioController.ToggleMute(false, "wl");
        }
        else
        {
            if (SoundOn_Button) SoundOn_Button.interactable = true;
            if (SoundOff_Button) SoundOff_Button.interactable = false;
            if (audioController) audioController.ToggleMute(true, "button");
            if (audioController) audioController.ToggleMute(true, "wl");
        }
    }
}
