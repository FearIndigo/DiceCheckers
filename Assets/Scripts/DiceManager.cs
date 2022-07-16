using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite[] _moveSprites;
    private List<Vector3Int>[] _allMoves;
    private List<Vector3Int> _currentMove;
    public List<Vector3Int> CurrentMove => _currentMove;

    public List<Vector3Int> SuperMove
    {
        get;
        private set;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
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

        // Setup super move
        SuperMove = new List<Vector3Int>();
        SuperMove.Add(new Vector3Int(0, 1));
        SuperMove.Add(new Vector3Int(0, -1));
        SuperMove.Add(new Vector3Int(-1, 0));
        SuperMove.Add(new Vector3Int(1, 0));
        SuperMove.Add(new Vector3Int(-1, 1));
        SuperMove.Add(new Vector3Int(1, -1));
        SuperMove.Add(new Vector3Int(1, 1));
        SuperMove.Add(new Vector3Int(-1, -1));
        SuperMove.Add(new Vector3Int(-1, 2));
        SuperMove.Add(new Vector3Int(-1, -2));
        SuperMove.Add(new Vector3Int(-2, 1));
        SuperMove.Add(new Vector3Int(-2, -1));
        SuperMove.Add(new Vector3Int(1, 2));
        SuperMove.Add(new Vector3Int(1, -2));
        SuperMove.Add(new Vector3Int(2, 1));
        SuperMove.Add(new Vector3Int(2, -1));
    }
    
    public void UpdateMove()
    {
        var board = BoardManager.Instance.Board;
        
        // Exit if terminal board
        if (BoardManager.Instance.Terminal(board))
        {
            return;
        }
        
        var index = 0;
        var validMoves = new bool[2];
        while (!validMoves[0] || !validMoves[1])
        {
            validMoves[0] = false;
            validMoves[1] = false;
            
            // Get random index
            index = Random.Range(0, _allMoves.Length);
        
            // Set new move
            _currentMove = _allMoves[index];
            
            // Check there are valid moves for both players
            for (int i = 0; i < 2; i++)
            {
                // Get players for owner
                var playerPositions = PlayerManager.Instance.GetPlayerPositions(board, i);
                
                // Loop players for owner
                foreach (var playerPos in playerPositions)
                {
                    // Check there are valid actions for this player
                    if (PlayerManager.Instance.GetActions(board, playerPos).Count != 0)
                    {
                        // There are valid moves for this player
                        validMoves[i] = true;
                        break;
                    }
                }
            }
        }

        // Update sprite
        _renderer.sprite = _moveSprites[index];
    }
}
