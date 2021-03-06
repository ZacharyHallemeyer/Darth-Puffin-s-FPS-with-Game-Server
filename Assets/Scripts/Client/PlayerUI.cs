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

    // Crouch UI
    public Image crouchImage;

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
        if(CFFAGameManager.players.Count > 0)
        {
            for (int i = 0; i < CFFAGameManager.players.Count; i++)
            {
                scoreBoardRows[i].userNameText.text = CFFAGameManager.players[i + 1].username;
                scoreBoardRows[i].currentKillsText.text = CFFAGameManager.players[i + 1].currentKills.ToString();
                scoreBoardRows[i].currentDeathsText.text = CFFAGameManager.players[i + 1].currentDeaths.ToString();
                scoreBoardRows[i].currentKillsInt = CFFAGameManager.players[i + 1].currentKills;
                scoreBoardRows[i].currentDeathsInt = CFFAGameManager.players[i + 1].currentDeaths;
            }
        }
        else if(CInfectionGameManager.players.Count > 0)
        {
            for (int i = 0; i < CInfectionGameManager.players.Count; i++)
            {
                scoreBoardRows[i].userNameText.text = CInfectionGameManager.players[i + 1].username;
                scoreBoardRows[i].currentKillsText.text = CInfectionGameManager.players[i + 1].currentKills.ToString();
                scoreBoardRows[i].currentDeathsText.text = CInfectionGameManager.players[i + 1].currentDeaths.ToString();
                scoreBoardRows[i].currentKillsInt = CInfectionGameManager.players[i + 1].currentKills;
                scoreBoardRows[i].currentDeathsInt = CInfectionGameManager.players[i + 1].currentDeaths;
            }
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
        if(CFFAGameManager.players.Count > 0)
        {
            scoreBoardRows = new ScoreBoardRow[CFFAGameManager.players.Count];
            foreach(PlayerManager _player in CFFAGameManager.players.Values)
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
        }
        else if(CInfectionGameManager.players.Count > 0)
        {
            scoreBoardRows = new ScoreBoardRow[CInfectionGameManager.players.Count];
            foreach (PlayerManager _player in CInfectionGameManager.players.Values)
            {
                if (_player != null)
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
        }
        scoreBoardParent.SetActive(false);
    }

    // End of Score Board ==================

    // Crouch ==============================

    public void ChangeToCrouch()
    {
        CancelInvoke("ChangeToStandAnim");
        InvokeRepeating("ChangeToCrouchAnim", 0f, .01f);
    }

    /// <summary>
    /// Compress square that represents player state. Must be called with Invoke repeating
    /// </summary>
    public void ChangeToCrouchAnim()
    {
        if (crouchImage.rectTransform.sizeDelta.y > 50)
        {
            crouchImage.rectTransform.anchoredPosition -= new Vector2(0f, 3f);
            crouchImage.rectTransform.sizeDelta -= Vector2.up * 6;
        }
        else
        {
            crouchImage.rectTransform.sizeDelta = new Vector2(100, 50);
            CancelInvoke("ChangeToCrouchAnim");
        }
    }

    public void ChangeToStand()
    {
        CancelInvoke("ChangeToCrouchAnim");
        InvokeRepeating("ChangeToStandAnim", 0f, .01f);
    }

    /// <summary>
    /// Expands square that represents player state. Must be called with Invoke repeating
    /// </summary>
    public void ChangeToStandAnim()
    {
        if (crouchImage.rectTransform.sizeDelta.y < 100)
        {
            crouchImage.rectTransform.localPosition += new Vector3(0f, 3f, 0f);
            crouchImage.rectTransform.sizeDelta += Vector2.up * 6;
        }
        else
        {
            crouchImage.rectTransform.sizeDelta = new Vector2(100, 100);
            CancelInvoke("ChangeToStandAnim");
        }
    }

    // End of Crouch =======================
}
