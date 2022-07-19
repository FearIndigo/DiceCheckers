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
    public Environment Env;

    [SerializeField] private TextMeshProUGUI _winnerText;
    private string[] _playerNames;

    // Start is called before the first frame update
    void Start()
    {
        // Reset game
        Env.Reset();
    }

    public void Reset()
    {
        _playerNames = new string[] {"Player A", "Player B"};
        _winnerText.text = "";
        Env.Board.Reset();
        Env.Dice.Reset();
        Env.Input.Reset();
        Env.Players.Reset();
    }

    public void GameWinner(int owner)
    {
        _winnerText.text = _playerNames[owner] + " Wins!";
        
        Debug.Log(_winnerText.text);
    }
}
