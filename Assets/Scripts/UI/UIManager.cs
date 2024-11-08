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
    [SerializeField] private TMP_Text Bat_Text;
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
    [SerializeField] internal Button Settings_Button;
    [SerializeField] private Button SettingsExit_Button;
    [SerializeField] private Button SoundToggle_button;
    [SerializeField] private Button MusicToggle_button;
    [SerializeField] private Button Mute_button;
    [SerializeField] private Sprite empty;
    [SerializeField] private Sprite button;
    private bool isMusic = true;
    private bool isSound = true;
    private bool isMute = false;

    [Header("all Win Popup")]
    [SerializeField] private GameObject specialWinObject;

    [SerializeField] private ImageAnimation normalWinImage;
    [SerializeField] private TMP_Text specialWinTitle;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;


    [Header("low balance popup")]
    [SerializeField] private GameObject LowBalancePopup_Object;
    [SerializeField] private Button Close_Button;


    [Header("disconnection popup")]
    [SerializeField] private GameObject DisconnectPopup_Object;
    [SerializeField] private Button CloseDisconnect_Button;

    [Header("Quit Popup")]
    [SerializeField] private GameObject QuitPopupObject;
    [SerializeField] private Button GameExit_Button;
    [SerializeField] private Button no_Button;
    [SerializeField] private Button yes_Button;

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

    private bool isExit = false;

    [Header("player texts")]
    [SerializeField] private TMP_Text playerCurrentWinning;
    [SerializeField] private TMP_Text playerBalance;

    private GameObject currentPopup;

    internal Action<bool, string> ToggleAudio;
    internal Action<string> playButtonAudio;

    internal Action OnExit;

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

        SetButton(MusicToggle_button, ToggleMusic);
        SetButton(SoundToggle_button, ToggleSound);
        SetButton(Mute_button, ToggleMute);

        SetButton(LeftBtn, () => Slide(-1));
        SetButton(RightBtn, () => Slide(1));
        SetButton(CloseDisconnect_Button, CallOnExitFunction);
        SetButton(Close_Button, ClosePopup);
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
            playButtonAudio?.Invoke("default");
            action?.Invoke();

        });
    }



    // private IEnumerator LoadingRoutine()
    // {
    //     StartCoroutine(LoadingTextAnimate());
    //     float fillAmount = 0.7f;
    //     progressbar.DOFillAmount(fillAmount, 2f).SetEase(Ease.Linear);
    //     yield return new WaitForSecondsRealtime(2f);
    //     yield return new WaitUntil(() => !socketManager.isLoading);
    //     progressbar.DOFillAmount(1, 1f).SetEase(Ease.Linear);
    //     yield return new WaitForSecondsRealtime(1f);
    //     if (spalsh_screen) spalsh_screen.SetActive(false);
    //     StopCoroutine(LoadingTextAnimate());
    // }

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


    internal void UpdatePlayerInfo(PlayerData playerData)
    {
        playerCurrentWinning.text = playerData.currentWining.ToString();
        playerBalance.text = playerData.Balance.ToString("f4");

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

    internal void PopulateSymbolsPayout(UIData uIData)
    {
        string text = "";
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            text = "";
            for (int j = 0; j < uIData.symbols[i].Multiplier.Count; j++)
            {
                text += $"{6 - j}x - {uIData.symbols[i].Multiplier[j][0]} \n";
            }
            SymbolsText[i].text = text;
        }

        text = "";
        for (int i = 0; i < uIData.wildMultiplier.Count; i++)
        {
            text += $"{i + 3}x - {uIData.wildMultiplier[i]} \n";
        }
        Wild_Text.text = text;

        text = "";
        string multiplierRow = "";
        string valueRow = "";

        for (int i = 0; i < uIData.BatsMultiplier.Count; i++)
        {
            if (uIData.BatsMultiplier[i] == 0)
                continue;
            multiplierRow += $"X{uIData.BatsMultiplier.Count - i}\t";
            valueRow += $"{uIData.BatsMultiplier[i]}\t";
        }

        Bat_Text.text = multiplierRow + "\n" + valueRow;



    }

    private void CallOnExitFunction()
    {
        isExit = true;
        OnExit?.Invoke();
        // audioController.PlayButtonAudio();
        // socketManager.CloseSocket();
    }

    private void OpenPopup(GameObject Popup)
    {
        if(currentPopup!=null && !DisconnectPopup_Object.activeSelf){
            ClosePopup();
        }
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        currentPopup = Popup;
        // paytableList[CurrentIndex].SetActive(true);
    }

    internal void ClosePopup()
    {
        if (!DisconnectPopup_Object.activeSelf)
        {
            if(currentPopup!=null){
            currentPopup.SetActive(false);
            if (MainPopup_Object) MainPopup_Object.SetActive(false);

            currentPopup = null;
            }

        }

        // CurrentIndex=0;
        // paytableList[CurrentIndex].SetActive(true);
    }



    private void Slide(int direction)
    {

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
        // if (CurrentIndex == paytableList.Length - 1)
        // {
        //     RightBtn.interactable = false;
        // }
        // else
        // {
        //     RightBtn.interactable = true;

        // }
        // if (CurrentIndex == 0)
        // {
        //     LeftBtn.interactable = false;
        // }
        // else
        // {
        //     LeftBtn.interactable = true;
        // }

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

    internal void DeductBalanceAnim(double finalAmount, double initAmount)
    {

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
            MusicToggle_button.image.sprite = button;

            ToggleAudio?.Invoke(false, "bg");
        }
        else
        {
            MusicToggle_button.image.sprite = empty;
            ToggleAudio?.Invoke(true, "bg");
        }
    }

    private void ToggleSound()
    {
        isSound = !isSound;
        if (isSound)
        {
            SoundToggle_button.image.sprite = button;
            ToggleAudio?.Invoke(false, "button");
            ToggleAudio?.Invoke(false, "wl");
        }
        else
        {
            SoundToggle_button.image.sprite = empty;
            ToggleAudio?.Invoke(true, "button");
            ToggleAudio?.Invoke(true, "wl");
        }
    }

    private void ToggleMute()
    {
        isMute = !isMute;
        if (isMute)
        {
            isSound=!isSound;
            isMusic=!isMusic;
            ToggleAudio?.Invoke(true, "all");
            Mute_button.transform.GetChild(1).gameObject.SetActive(true);
            Mute_button.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Mute_button.transform.GetChild(1).gameObject.SetActive(false);
            Mute_button.transform.GetChild(0).gameObject.SetActive(true);
            ToggleSound();
            ToggleMusic();
        }


    }
}
