using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameManager : MonoBehaviour
{

    public static PlayerNameManager Instance { get; private set; }
    
    [Header("Name Entry Panel")]
    [SerializeField] private GameObject nameEntryPanel;   
    [SerializeField] private TMP_InputField nameInputField; 
    [SerializeField] private Button playButton;             
    [SerializeField] private TMP_Text errorText;           
    
    [Header("Leaderboard Display")]
    [SerializeField] private TMP_Text leaderboardText;


    [Header("Leaderboard UI")]
    [SerializeField] private Camera LeaderCamera;
    [SerializeField] private Button btnLBShow;
    [SerializeField] private Button btnLBHide;

    private const string KEY_COUNT  = "LB_Count";
    private const string KEY_NAME   = "LB_Name_";
    private const string KEY_SCORE  = "LB_Score_";
    
    public string CurrentPlayerName { get; private set; } = "";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (errorText != null)
            errorText.text = "";
        
        if (nameEntryPanel != null)
            nameEntryPanel.SetActive(true);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        
        SetupButtons();
        RefreshLeaderboardUI();
    }

    private void SetupButtons()
    {
        btnLBShow.onClick.AddListener(() =>
        {
            LeaderCamera.gameObject.SetActive(true);
            btnLBHide.gameObject.SetActive(true);
            btnLBShow.gameObject.SetActive(false);
        });
        btnLBHide.onClick.AddListener(() =>
        {
            LeaderCamera.gameObject.SetActive(false);
            btnLBShow.gameObject.SetActive(true);
            btnLBHide.gameObject.SetActive(false);
        });
    }
    private void OnPlayClicked()
    {
        string enteredName = nameInputField != null
            ? nameInputField.text.Trim()
            : "";

        if (string.IsNullOrEmpty(enteredName))
        {
            if (errorText != null)
                errorText.text = "<color=#FF4444>ًﻻﻭﺃ ﻚﻤﺳﺍ ﻝﺎﺧﺩﺇ ﺀﺎﺟﺮﻟﺍ</color>";
            return;
        }

        CurrentPlayerName = enteredName;

        if (errorText != null)
            errorText.text = "";

        if (nameEntryPanel != null)
            nameEntryPanel.SetActive(false);

        Debug.Log($"[PlayerNameManager] اسم اللاعب: {CurrentPlayerName}");
    }


    /// <param name="score">(0–100)</param>
    public void SaveScore(int score)
    {
        if (string.IsNullOrEmpty(CurrentPlayerName))
        {
            Debug.LogWarning("[PlayerNameManager] لا يوجد اسم لاعب — لم يتم الحفظ");
            return;
        }

        int count = PlayerPrefs.GetInt(KEY_COUNT, 0);

        PlayerPrefs.SetString(KEY_NAME  + count, CurrentPlayerName);
        PlayerPrefs.SetInt   (KEY_SCORE + count, score);
        PlayerPrefs.SetInt   (KEY_COUNT, count + 1);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerNameManager] saved : {CurrentPlayerName} — {score}");

        RefreshLeaderboardUI();
    }


    public void RefreshLeaderboardUI()
    {
        if (leaderboardText == null) return;

        int count = PlayerPrefs.GetInt(KEY_COUNT, 0);

        if (count == 0)
        {
            leaderboardText.text = "NO SCORE";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < count; i++)
        {
            string n = PlayerPrefs.GetString(KEY_NAME  + i, "؟");
            int    s = PlayerPrefs.GetInt   (KEY_SCORE + i, 0);
            
            sb.Append(n).Append(" ").Append(s);

            if (i < count - 1)
                sb.Append("\n");
        }

        leaderboardText.text = sb.ToString();
    }


    public void ClearLeaderboard()
    {
        int count = PlayerPrefs.GetInt(KEY_COUNT, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(KEY_NAME  + i);
            PlayerPrefs.DeleteKey(KEY_SCORE + i);
        }
        PlayerPrefs.DeleteKey(KEY_COUNT);
        PlayerPrefs.Save();

        RefreshLeaderboardUI();
        Debug.Log("[PlayerNameManager] clean");
    }
}
