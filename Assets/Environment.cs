using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private int _maxSteps;
    private int _steps;

    public AiManager Ai;
    public BoardManager Board;
    public DiceManager Dice;
    public GameManager Game;
    public InputManager Input;
    public PlayerManager Players;

    public void Reset()
    {
        _steps = 0;
        Game.Reset();
    }
    
    public void UpdateSteps()
    {
        _steps++;

        if (_steps == _maxSteps)
        {
            Ai.Interrupted();
        }
    }
}
