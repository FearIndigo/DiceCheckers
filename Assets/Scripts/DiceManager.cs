using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    public Environment Env;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private SpriteRenderer _kingRenderer;
    [SerializeField] private Sprite[] _moveSprites;
    [SerializeField] private Color[] _colours;
    private List<Direction>[] _diceDirections;
    [SerializeField] private int _index;

    [SerializeField] private int _kingIndex;
    public List<Direction> CurrentDirections => _diceDirections[_index];
    public List<Direction> KingCurrentDirections => _diceDirections[_kingIndex];

    public void Reset()
    {
        // Setup all moves
        SetupAllMoves();
        
        // Get starting move
        UpdateMove();
    }

    void SetupAllMoves()
    {
        _diceDirections = new List<Direction>[6];
        
        for (int i = 0; i < _diceDirections.Length; i++)
        {
            var directions = new List<Direction>();
            switch (i)
            {
                case 0:
                    directions.Add(Direction.Up);
                    break;
                case 1:
                    directions.Add(Direction.Down);
                    break;
                case 2:
                    directions.Add(Direction.Left);
                    break;
                case 3:
                    directions.Add(Direction.Right);
                    break;
                case 4:
                    directions.Add(Direction.Up);
                    directions.Add(Direction.Down);
                    break;
                case 5:
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Right);
                    break;
            }
            
            // Add directions to dice directions
            _diceDirections[i] = directions;
        }
    }
    
    public void UpdateMove()
    {
        var board = Env.Board.Board;
        
        // Exit if terminal board
        if (Env.Board.Terminal(board))
        {
            return;
        }

        var owner = Env.Players.PlayerATurn ? 0 : 1;
        _renderer.color = _colours[owner];

        // Update Normal piece move
        var validMove = false;
        while (!validMove)
        {
            // Get random index
            _index = Random.Range(0, _diceDirections.Length);
            
            // Get random king index, excluding selected normal index
            var otherIndexes = new int[_diceDirections.Length - 1];
            var index = -1;
            for (int i = 0; i < _diceDirections.Length; i++)
            {
                if (i == _index) continue;
                index++;
                otherIndexes[index] = i;
            }
            _kingIndex = otherIndexes[Random.Range(0, otherIndexes.Length)];

            // Check there are valid moves for owner player
            // Get players for owner
            var playerPositions = Env.Players.GetPlayerPositions(board, owner);
            
            // Loop players for owner
            foreach (var playerPos in playerPositions)
            {
                // Check there are valid actions for this player
                if (Env.Players.GetMoves(board, playerPos).Count != 0)
                {
                    // There are valid moves for this player
                    validMove = true;

                    break;
                }
            }
        }

        // Update sprite
        _renderer.sprite = _moveSprites[_index];
        _kingRenderer.sprite = _moveSprites[_kingIndex];
    }
}
