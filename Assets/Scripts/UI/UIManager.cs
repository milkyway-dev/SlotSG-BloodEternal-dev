using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
public class UIManager : MonoBehaviour
{

    [Header("AutoSpin Popup")]
    [SerializeField] private Button AutoSpinButton;
    [SerializeField] private Button AutoSpinPopUpClose;
    [SerializeField] private TMP_Text autoSpinCost;
    [SerializeField] private GameObject autoSpinPopupObject;
    [Header("Free Spin Popup")]
    [SerializeField] private GameObject FreeSPinPopUpObject;
    [SerializeField] private TMP_Text FreeSpinCount;

    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;

    [Header("Paytable Popup")]
    [SerializeField] private Button paytable_Button;
    [SerializeField] private GameObject payTablePopup_Object;
    [SerializeField] private Button paytableExit_Button;


    [Header("Paytable Texts")]
    [SerializeField] private TMP_Text[] SymbolsText;
    [SerializeField] private TMP_Text Scatter_Text;
    [SerializeField] private TMP_Text Wild_Text;

    [Header("Pagination")]
    int CurrentIndex = 0;
    [SerializeField] private GameObject[] paytableList;
    [SerializeField] private Button RightBtn;
    [SerializeField] private Button LeftBtn;

    [Header("Gamble texts")]
    [SerializeField] private TMP_Text bank;
    [SerializeField] private TMP_Text bet;
    [SerializeField] private TMP_Text potentialWin;


    [Header("Settings Popup")]
    [SerializeField] private GameObject SettingsPopup_Object;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;
    [SerializeField] private Button SoundOn_Button;
    [SerializeField] private Button SoundOff_Button;
    [SerializeField] private Button MusicOn_Button;
    [SerializeField] private Button MusicOff_Button;

    [Header("all Win Popup")]
    [SerializeField] private GameObject specialWinObject;

    [SerializeField] private ImageAnimation normalWinImage;
    [SerializeField] private TMP_Text specialWinTitle;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;

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



    [SerializeField]
    private Button m_AwakeGameButton;
    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;

    [Header("player texts")]
    [SerializeField] private TMP_Text playerCurrentWinning;
    [SerializeField] private TMP_Text playerBalance;

    [SerializeField] private GameObject currentPopup;
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
        SetButton(no_Button, () => { if (!isExit) { ClosePopup(); } });
        SetButton(GameExit_Button, () => OpenPopup(QuitPopupObject));
        SetButton(paytable_Button, () => OpenPopup(payTablePopup_Object));
        SetButton(paytableExit_Button, () => ClosePopup());
        SetButton(Settings_Button, () => OpenPopup(SettingsPopup_Object));
        SetButton(SettingsExit_Button, () => ClosePopup());
        SetButton(MusicOn_Button, ToggleMusic);
        SetButton(MusicOff_Button, ToggleMusic);
        SetButton(SoundOn_Button, ToggleSound);
        SetButton(SoundOff_Button, ToggleSound);
        SetButton(LeftBtn, () => Slide(-1));
        SetButton(RightBtn, () => Slide(1));
        SetButton(CloseDisconnect_Button, CallOnExitFunction);
        SetButton(Close_Button, CallOnExitFunction);
        SetButton(QuitSplash_button, () => OpenPopup(QuitPopupObject));
        SetButton(AutoSpinButton, () => OpenPopup(autoSpinPopupObject));
        SetButton(AutoSpinPopUpClose, () => ClosePopup());
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

    internal void InitiateGamble(double currentWinnings)
    {
        bank.text = currentWinnings.ToString();
        bet.text = currentWinnings.ToString();
        potentialWin.text = (currentWinnings * 2).ToString();
        // gambleObject.SetActive(true);
    }

    internal void UpdategambleInfo(double currentWinnings, bool half = false)
    {
        bank.text = currentWinnings.ToString("f4");
        bet.text = half ? (currentWinnings / 2).ToString() : currentWinnings.ToString("f4");
        potentialWin.text = half ? currentWinnings.ToString() : (currentWinnings * 2).ToString("f4");

    }
    internal void OnCollect()
    {
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


    internal void UpdateAutoSpinCost(double cost)
    {
        autoSpinCost.text = cost.ToString();
    }
    internal void LowBalPopup()
    {

        OpenPopup(LowBalancePopup_Object);
    }



    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    internal void PopulateSymbolsPayout(List<Symbol> symbols)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = "";
            for (int j = 0; j < symbols[i].Multiplier.Count; j++)
            {
                text += $"{6 - j}x - {symbols[i].Multiplier[j][0]} \n";
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
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
        currentPopup=Popup;
        paytableList[CurrentIndex = 0].SetActive(true);
    }

    internal void ClosePopup()
    {
        if (audioController) audioController.PlayButtonAudio();
        if(currentPopup.name.ToUpper()!="DISCONNECTIONPOPUP"){
            currentPopup.SetActive(false);
            if (MainPopup_Object) MainPopup_Object.SetActive(false);

        currentPopup=null;
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

    internal void FreeSpinPopup(int amount)
    {
        FreeSpinCount.text = amount.ToString();
        OpenPopup(FreeSPinPopUpObject);

    }

    internal void CloseFreeSpinPopup()
    {
        ClosePopup();
        FreeSpinCount.text = "0";
    }
    internal void EnableWinPopUp(int value)
    {

        OpenPopup(WinPopup_Object);
        if (value > 0)
            specialWinObject.SetActive(true);

        switch (value)
        {
            case 0:
                {
                    normalWinImage.gameObject.SetActive(true);
                    normalWinImage.StartAnimation();
                    break;
                }
            case 1:
                specialWinTitle.text = "BIG WIN";
                break;
            case 2:
                specialWinTitle.text = "HUGE WIN";
                break;
            case 3:
                specialWinTitle.text = "MEGA WIN";
                break;
        }
    }

    internal void DeductBalanceAnim(double finalAmount, double initAmount){

        DOTween.To(() => initAmount, (val) => initAmount = val, finalAmount, 0.8f).OnUpdate(() =>
        {
            playerBalance.text = initAmount.ToString("f4");

        }).OnComplete(() =>
        {

            playerBalance.text = finalAmount.ToString();
        });
    }

    internal IEnumerator WinTextAnim(double amount)
    {
        double initAmount = 0;
        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 0.8f).OnUpdate(() =>
        {
            Win_Text.text = initAmount.ToString("f5");

        }).OnComplete(() =>
        {

            Win_Text.text = amount.ToString();
        });
        yield return new WaitForSeconds(1.8f);
        normalWinImage.StopAnimation();
        if (normalWinImage.gameObject.activeSelf)
        {

            normalWinImage.gameObject.SetActive(false);
        }
        Win_Text.transform.DOLocalMoveY(-411, 0.35f);
        Win_Text.transform.DOScale(new Vector3(0, 0, 0), 0.4f).OnComplete(() =>
        {
            ClosePopup();
            Win_Text.transform.localScale = Vector3.one;
            Win_Text.transform.localPosition = Vector3.zero;
            Win_Text.text = "0";


        });
        yield return new WaitForSeconds(0.5f);
        specialWinObject.SetActive(false);


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
