using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    // Grapple UI
    public Slider grappleSlider;
    public Gradient grappleGradient;
    public Image grappleFill;
    public TextMeshProUGUI grappleOutOfBoundsText;

    // Ammo UI
    public TextMeshProUGUI ammoText;

    // JetPack UI
    public Slider jetPackSlider;
    public Gradient jetPackGradient;
    public Image jetPackFill;

    // ScoreBoard UI
    public GameObject scoreBoardParent;
    public GameObject scoreBoardRowPrefab;

    public class ScoreBoardRow
    {
        public GameObject scoreBoardRow;
        public TextMeshProUGUI userNameText;
        public TextMeshProUGUI currentKillsText;
        public TextMeshProUGUI currentDeathsText;
        public int currentKillsInt;
        public int currentDeathsInt;
    }
    public ScoreBoardRow[] scoreBoardRows;


    // Grapple =============================
    public void SetMaxGrapple(float maxValue)
    {
        grappleSlider.maxValue = maxValue;
        grappleSlider.value = maxValue;

        grappleFill.color = grappleGradient.Evaluate(1f);
    }

    public void SetGrapple(float grappleTime)
    {
        grappleSlider.value = grappleTime;
        grappleFill.color = grappleGradient.Evaluate(grappleSlider.normalizedValue);
    }

    public void GrappleOutOfBoundsUI()
    {
        grappleOutOfBoundsText.text = "X";
        InvokeRepeating("HideGrappleOutOfBoundsUI", 1f, 0f);
    }

    private void HideGrappleOutOfBoundsUI()
    {
        grappleOutOfBoundsText.text = "";
        CancelInvoke("HideGrappleOutOfBoundsUI");
    }

    // End of Grapple ======================

    // Ammo ================================

    public void ChangeGunUIText(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo + " / " + maxAmmo;
    }

    // End of Ammo =========================

    // Jet Pack ============================
    public void SetMaxJetPack(float maxValue)
    {
        jetPackSlider.maxValue = maxValue;
        jetPackSlider.value = maxValue;

        jetPackFill.color = jetPackGradient.Evaluate(1f);
    }

    public void SetJetPack(float jetPackTime)
    {
        jetPackSlider.value = jetPackTime;
        jetPackFill.color = jetPackGradient.Evaluate(jetPackSlider.normalizedValue);
    }

    // End of Jet Pack =====================

    // Score Board =========================

    public void ScoreBoard()
    {
        if (!scoreBoardParent.activeSelf)
            DisplayScoreBoard();
        else
            scoreBoardParent.SetActive(false);
    }

    public void DisplayScoreBoard()
    {
        scoreBoardParent.SetActive(true);
        for (int i = 0; i < GameManager.players.Count; i++)
        {
            scoreBoardRows[i].userNameText.text = GameManager.players[i + 1].username;
            scoreBoardRows[i].currentKillsText.text = GameManager.players[i + 1].currentKills.ToString();
            scoreBoardRows[i].currentDeathsText.text = GameManager.players[i + 1].currentDeaths.ToString();
            scoreBoardRows[i].currentKillsInt = GameManager.players[i + 1].currentKills;
            scoreBoardRows[i].currentDeathsInt = GameManager.players[i + 1].currentDeaths;
        }

        // Sort Score board by kills (bubble sort)
        bool _changeMade = true;
        while(_changeMade)
        {
            _changeMade = false;
            for(int i = 0; i < scoreBoardRows.Length-1; i++)
            {
                if (scoreBoardRows[i].currentKillsInt < scoreBoardRows[i+1].currentKillsInt)
                {
                    string _tempUsername = scoreBoardRows[i].userNameText.text;
                    string _tempcurrentKillsText= scoreBoardRows[i].currentKillsText.text;
                    string _tempcurrentDeathsText= scoreBoardRows[i].currentDeathsText.text;
                    int _tempcurrentKillsInt= scoreBoardRows[i].currentKillsInt;
                    int _tempcurrentDeathsInt= scoreBoardRows[i].currentDeathsInt;
                    scoreBoardRows[i].userNameText.text = scoreBoardRows[i + 1].userNameText.text;
                    scoreBoardRows[i].currentKillsText.text = scoreBoardRows[i + 1].currentKillsText.text;
                    scoreBoardRows[i].currentDeathsText.text = scoreBoardRows[i + 1].currentDeathsText.text;
                    scoreBoardRows[i].currentKillsInt = scoreBoardRows[i + 1].currentKillsInt;
                    scoreBoardRows[i].currentDeathsInt = scoreBoardRows[i + 1].currentDeathsInt;
                    scoreBoardRows[i + 1].userNameText.text = _tempUsername;
                    scoreBoardRows[i + 1].currentKillsText.text = _tempcurrentKillsText;
                    scoreBoardRows[i + 1].currentDeathsText.text = _tempcurrentDeathsText;
                    scoreBoardRows[i + 1].currentKillsInt = _tempcurrentKillsInt;
                    scoreBoardRows[i + 1].currentDeathsInt = _tempcurrentDeathsInt;
                    _changeMade = true;
                }
            }
        }
    }

    public void InitScoreBoard()
    {
        float _yPos = 0.2f;
        if(scoreBoardRows != null)
        {
            foreach(ScoreBoardRow _scoreBoardRow in scoreBoardRows)
            {
                Destroy(_scoreBoardRow.scoreBoardRow);
            }
        }
        scoreBoardRows = new ScoreBoardRow[GameManager.players.Count];
        foreach(PlayerManager _player in GameManager.players.Values)
        {
            if(_player != null)
            {
                GameObject _scoreBoardRow = Instantiate(scoreBoardRowPrefab, scoreBoardParent.transform);
                _scoreBoardRow.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, _yPos, 0);

                _yPos -= .1f;
                scoreBoardRows[_player.id - 1] = new ScoreBoardRow();
                scoreBoardRows[_player.id - 1].scoreBoardRow = _scoreBoardRow;
                scoreBoardRows[_player.id - 1].userNameText = _scoreBoardRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                scoreBoardRows[_player.id - 1].currentKillsText = _scoreBoardRow.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                scoreBoardRows[_player.id - 1].currentDeathsText = _scoreBoardRow.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            }
        }
        scoreBoardParent.SetActive(false);
    }

    // End of Score Board ==================
}
