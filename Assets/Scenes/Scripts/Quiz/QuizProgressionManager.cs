using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuizProgressionManager : MonoBehaviour
{
    [Serializable]
    private class Step
    {
        public QuizCube cube;
        public GameObject barrier;
    }

    [SerializeField] private List<Step> steps =
        new List<Step>();

    [Header("References")]
    [SerializeField] private QuizUI quizUI;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text alertText;


    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject LosePanel;
    [Header("Quiz Rules")]
    [SerializeField] private int maxWrongAnswers = 3;

    [Header("Leaderboard")]
    [SerializeField] private TMP_Text winResultText;

    [Header("Sound Effects")] [SerializeField]
    private AudioListener[] audioListeners;

    [Header("Player Animator Controller")] [SerializeField]
    private Animator anim;
    
    private int currentScore;
    private int wrongAnswers;

    public event Action OnAllSolved;

    private void OnEnable()
    {
        if (quizUI != null)
        {
            quizUI.Answered += HandleAnswer;
        }
    }

    private void OnDisable()
    {
        if (quizUI != null)
        {
            quizUI.Answered -= HandleAnswer;
        }

        UnsubscribeFromCubes();
    }

    private void Start()
    {
        SubscribeToCubes();

        ResetQuiz();
    }

    private void SubscribeToCubes()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].cube == null)
                continue;

            int index = i;

            steps[i].cube.OnSolved +=
                _ => Advance(index);
        }
    }

    private void UnsubscribeFromCubes()
    {

    }

    private void HandleAnswer(bool correct)
    {
        string[] support = { "!ﺖﻨﺴﺣﺍ", "ﻊﺋﺍﺭ ﺖﻧﺍ!","!ﻞﻫﺬﻣ", "!ﻞﻄﺑ", "ًَﺍﺪﺟ ﻲﻛﺫ!", "!ﺮﻤﺘﺳﺍ" };
        if (correct)
        {
            int range = Random.Range(0, support.Length-1);
            alertText.text = 
                $"<color=#00FF00>{support[range]}</color>";

            return;
        }

        wrongAnswers++;

        alertText.text =
            $"<color=#FF0000>ﺔﺌﻃﺎﺧ ﺔﺑﺎﺟﺍ!</color> {wrongAnswers}/{maxWrongAnswers}";

        if (wrongAnswers >= maxWrongAnswers)
        {
            LoseQuiz();
        }
    }

    IEnumerator HappyAnimation()
    {
        yield return new WaitForSeconds(1f);
        anim.SetBool("Answered", true);
        yield return new WaitForSeconds(2f);
        anim.SetBool("Answered", false);

    }
    private void Advance(int index)
    {
        if (wrongAnswers >= maxWrongAnswers)
            return;

        currentScore++;
        StartCoroutine(HappyAnimation());
        UpdateScoreUI();
        audioListeners[0].enabled = true;
        Step step = steps[index];
        
        if (step.barrier != null)
        {
            step.barrier.SetActive(false);
        }

        int next = index + 1;

        if (next < steps.Count)
        {
            if (steps[next].cube != null)
            {
                steps[next].cube.Unlock();

                Debug.Log(
                    $"Next Cube: {steps[next].cube.name}"
                );
            }
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        alertText.text =
            $"ﺭﺎﺒﺘﺧﻻﺍ ﻰﻬﺘﻧﺍ: ";

        WinPanel.SetActive(true);
        if (scoreText != null)
        {
            scoreText.text =
                $"ﻚﺘﻣﻼﻋ: {currentScore} / {steps.Count}";
        }

        int percentage = steps.Count > 0
            ? Mathf.RoundToInt((float)currentScore / steps.Count * 100)
            : 0;

        string playerName = PlayerNameManager.Instance != null
            ? PlayerNameManager.Instance.CurrentPlayerName
            : "";

        if (winResultText != null)
            winResultText.text = $"{playerName} {percentage}";

        if (PlayerNameManager.Instance != null)
            PlayerNameManager.Instance.SaveScore(percentage);

        OnAllSolved?.Invoke();
    }

    private void LoseQuiz()
    {
        alertText.text = "<color=#FF0000>ًﺍﺩﺪﺠﻣ ﻝﻭﺎﺣ ﺕﺮﺴﺧ ﺪﻘﻟ!</color>";
        if (quizUI != null)
        {
            quizUI.ShowLose();
        }

        LosePanel.SetActive(true);
        Invoke(nameof(ResetQuiz), 1.5f);
        LosePanel.GetComponentInChildren<Button>().onClick.AddListener(() => ResetQuiz());
    }

    private void ResetQuiz()
    {
        currentScore = 0;
        wrongAnswers = 0;
// lock cubes
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].cube != null)
            {
                steps[i].cube.ResetCube();
            }
            
            // reput barrier
            if (steps[i].barrier != null)
            {
                steps[i].barrier.SetActive(true);
            }
        }
        if (steps.Count > 0 &&
            steps[0].cube != null)
        {
            steps[0].cube.Unlock();
        }

        UpdateScoreUI();

        Debug.Log(
            "QUIZ RESET START FROM CUBE 1"
        );
        // teleport player
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                $"ﺔﻣﻼﻌﻟﺍ: {currentScore} / {steps.Count}";
        }
        LosePanel.SetActive(false);
        WinPanel.SetActive(false);
    }
}