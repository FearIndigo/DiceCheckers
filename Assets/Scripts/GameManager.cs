using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private TextMeshProUGUI _winnerText;
    private string[] _playerNames;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
        
        // Reset game
        Reset();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Reset()
    {
        _playerNames = new string[] {"Player A", "Player B"};
        _winnerText.text = "";
        BoardManager.Instance.Reset();
        PlayerManager.Instance.Reset();
        AiManager.Instance.Reset();
        DiceManager.Instance.Reset();
        InputManager.Instance.Reset();
    }

    public void GameWinner(int owner)
    {
        _winnerText.text = _playerNames[owner] + " Wins!";
        
        Debug.Log(_winnerText.text);
    }
}
