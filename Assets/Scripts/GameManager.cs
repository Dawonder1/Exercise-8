using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 5;
    public static GameManager Instance { get; set; }

    public bool IsDead { get; set; }
    private bool isGameStarted = false;
    private PlayerMotor motor;

    //UI and the UI fields
    public Animator gameCanvas, menuAnim;
    public TextMeshProUGUI scoreText, coinText, modifierText, highScoreText;
    public GameObject dailyRewardPanel;
    private float score, coinScore, modifierScore;
    private int lastscore;
    private string lastPlayDate;

    //Death menu
    public Animator deathMenuAnim;
    public TextMeshProUGUI deadScoreText, deadCoinText;

    private void Awake()
    {
        Instance = this;
        modifierScore = 1;
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();

        modifierText.text = "x" + modifierScore.ToString("0.0");
        coinText.text = coinScore.ToString("0");
        scoreText.text = scoreText.text = score.ToString("0");

        highScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
        lastPlayDate = PlayerPrefs.GetString("lastPlayDate", DateTime.Today.ToString());
    }

    private void Update()
    {
        if(MobileInput.Instance.Tap && !isGameStarted  && FindObjectOfType<PlayerMotor>().canStart)
        {
            isGameStarted = true;
            motor.StartRunning();
            FindObjectOfType<GlacierSpawner>().IsScrolling = true;
            FindObjectOfType<CameraMotor>().IsMoving = true;
            gameCanvas.SetTrigger("Show");
            menuAnim.SetTrigger("Hide");
        }
        else if(MobileInput.Instance.Tap && !isGameStarted)
        {
            FindObjectOfType<PlayerMotor>().canStart = true;
        }

        if (isGameStarted  && !IsDead)
        {
            //increase the score
            score += (Time.deltaTime * modifierScore);
            if (lastscore != (int)score)
            {
                lastscore = (int)score;
                scoreText.text = score.ToString("0");
            }
        }
    }

    public void GetCoin()
    {
        coinScore += COIN_SCORE_AMOUNT;
        coinText.text = coinScore.ToString();
    }

    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    public void onPlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void onDeath()
    {
        IsDead = true;
        deadScoreText.text = score.ToString("0");
        deadCoinText.text = coinScore.ToString();
        deathMenuAnim.SetTrigger("Dead");
        FindObjectOfType<GlacierSpawner>().IsScrolling = false;
        gameCanvas.SetTrigger("Hide");
        if (DateTime.Today.ToString() != lastPlayDate)
        {
            dailyRewardPanel.SetActive(true);
        }

        //check if this is a highscore
        if( score > PlayerPrefs.GetInt("HighScore"))
        {
            float s = score;
            if(s % 1 == 0)
            {
                s += 1;
            }
            PlayerPrefs.SetInt("HighScore", (int)s);
        }
    }

    public void ClaimDailyReward()
    {
        dailyRewardPanel.SetActive(false);
        PlayerPrefs.SetString("lastPlayDate", DateTime.Today.ToString());
    }
}
