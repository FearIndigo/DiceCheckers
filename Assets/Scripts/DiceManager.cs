using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
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
    private List<Vector3Int>[] _allMoves;
    private int _index;
    private int _kingIndex;
    public List<Vector3Int> CurrentMove => _allMoves[_index];
    public List<Vector3Int> KingCurrentMove => _allMoves[_kingIndex];

    public void Reset()
    {
        // Setup all moves
        SetupAllMoves();
        
        // Get starting move
        UpdateMove();
    }

    void SetupAllMoves()
    {
        _allMoves = new List<Vector3Int>[7];
        
        for (int i = 0; i < _allMoves.Length; i++)
        {
            var moves = new List<Vector3Int>();
            switch (i)
            {
                case 0:
                    moves.Add(new Vector3Int(0, 1));
                    moves.Add(new Vector3Int(0, -1));
                    break;
                case 1:
                    moves.Add(new Vector3Int(-1, 0));
                    moves.Add(new Vector3Int(1, 0));
                    break;
                case 2:
                    moves.Add(new Vector3Int(0, 1));
                    moves.Add(new Vector3Int(0, -1));
                    moves.Add(new Vector3Int(-1, 0));
                    moves.Add(new Vector3Int(1, 0));
                    break;
                case 3:
                    moves.Add(new Vector3Int(-1, 1));
                    moves.Add(new Vector3Int(1, -1));
                    break;
                case 4:
                    moves.Add(new Vector3Int(1, 1));
                    moves.Add(new Vector3Int(-1, -1));
                    break;
                case 5:
                    moves.Add(new Vector3Int(-1, 1));
                    moves.Add(new Vector3Int(1, -1));
                    moves.Add(new Vector3Int(1, 1));
                    moves.Add(new Vector3Int(-1, -1));
                    break;
                case 6:
                    moves.Add(new Vector3Int(-1, 2));
                    moves.Add(new Vector3Int(-1, -2));
                    moves.Add(new Vector3Int(-2, 1));
                    moves.Add(new Vector3Int(-2, -1));
                    moves.Add(new Vector3Int(1, 2));
                    moves.Add(new Vector3Int(1, -2));
                    moves.Add(new Vector3Int(2, 1));
                    moves.Add(new Vector3Int(2, -1));
                    break;
            }
            
            // Add moves to all moves
            _allMoves[i] = moves;
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
        var validKing = false;
        while (!validMove)
        {
            // Get random index
            _index = Random.Range(0, _allMoves.Length);

            // Check there are valid moves for owner player
            // Get players for owner
            var playerPositions = Env.Players.GetPlayerPositions(board, owner);
            
            // Loop players for owner
            foreach (var playerPos in playerPositions)
            {
                // Check there are valid actions for this player
                if (Env.Players.GetActions(board, playerPos).Count != 0)
                {
                    // There are valid moves for this player
                    validMove = true;

                    // Get random king index
                    _kingIndex = Random.Range(0, _allMoves.Length);

                    break;
                }
            }
        }

        // Update sprite
        _renderer.sprite = _moveSprites[_index];
        _kingRenderer.sprite = _moveSprites[_kingIndex];
    }
}
