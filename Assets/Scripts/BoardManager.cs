using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public Environment Env;
    
    [SerializeField] private Tilemap _playerTilemap;
    [SerializeField] private Tilemap _actionsTilemap;
    [SerializeField] private TileBase _actionTile;
    private Vector3Int _boardOffset;
    [SerializeField] private int _boardSize;
    public int BoardSize => _boardSize;
    private Dictionary<Vector3Int, int> _board;
    public Dictionary<Vector3Int, int> Board => _board;

    public void Reset()
    {
        _boardOffset = new Vector3Int(-_boardSize / 2, -_boardSize / 2);
        _board = new Dictionary<Vector3Int, int>();
        for (int x = 0; x < _boardSize; x++)
        {
            for (int y = 0; y < _boardSize; y++)
            {
                if (y == 0)
                {
                    // Player 1 setup
                    _board[new Vector3Int(x, y, 0)] = 1;
                }
                else if (y == _boardSize - 1)
                {
                    // Player 2 setup
                    _board[new Vector3Int(x, y, 0)] = 2;
                }
                else
                {
                    // Rest of board
                    _board[new Vector3Int(x, y, 0)] = 0;
                }
            }
        }
        
        // Update board visuals
        UpdatePlayerTiles();
    }

    public bool PerformAction(PlayerManager.Action action)
    {
        // Test From has value that is a player, and To is a valid position that isn't a player on the same team
        if (!_board.TryGetValue(action.From, out int from) || from == 0 || !_board.TryGetValue(action.To, out int to) || from == to)
        {
            Debug.LogError("Illegal Action: (" + action.From + ", " + action.To + ").");
            return false;
        }
        
        // Check it is this players turn
        if (!Env.Players.IsPlayersTurn(_board, action.From))
        {
            Debug.LogError("Can only perform action on own player.");
            return false;
        }
        
        // Move player on board
        _board = Result(_board, action, Env.Players.PlayerATurn ? 0 : 1);
        
        // Update board visuals
        UpdatePlayerTiles();

        // Check if terminal board
        if (Terminal(_board))
        {
            Env.Game.GameWinner(Env.Players.GetOwner(_board, action.To));
        }

        return true;
    }

    public Dictionary<Vector3Int, int> Result(Dictionary<Vector3Int, int> board, PlayerManager.Action action, int owner)
    {
        // Test From has value that is a player, and To is a valid position that isn't a player on the same team
        if (!board.TryGetValue(action.From, out int from) || from == 0 || !board.TryGetValue(action.To, out int to) || from == to)
        {
            Debug.LogError("Illegal Action: (" + action.From + ", " + action.To + ").");
            return board;
        }
        
        // Check it is this players turn
        if (Env.Players.GetOwner(board, action.From) != owner)
        {
            Debug.LogError("Can only perform action on own player.");
            return board;
        }
        
        // Copy board
        var newBoard = new Dictionary<Vector3Int, int>(board);

        // If owner A upgrading to super player
        if(Env.Players.GetOwner(board, action.From) == 0 && action.To.y == _boardSize - 1)
        {
            newBoard[action.To] = 3;
        }
        // If owner B upgrading to super player
        else if (Env.Players.GetOwner(board, action.From) == 1 && action.To.y == 0)
        {
            newBoard[action.To] = 4;
        }
        // Move player
        else
        {
            newBoard[action.To] = from;
        }
        newBoard[action.From] = 0;
        
        // Return resulting board
        return newBoard;
    }

    public Vector3Int GetBoardPos(Vector3 position, bool local = true)
    {
        if (local)
        {
            return _playerTilemap.LocalToCell(position + _boardOffset);
        }
        else
        {
            return _playerTilemap.WorldToCell(position - _boardOffset);
        }
    }

    public bool Terminal(Dictionary<Vector3Int, int> board)
    {
        for (int i = 0; i < 2; i++)
        {
            var players = Env.Players.GetPlayerPositions(board, i);
            if (players.Count == 0)
            {
                return true;
            }
        }

        return false;
    }
    
    private void UpdatePlayerTiles()
    {
        // Clear all tiles
        _playerTilemap.ClearAllTiles();

        // Initialise change data
        List<TileChangeData> changeTiles = new List<TileChangeData>();
        
        // Loop board
        foreach (var keyVal in _board)
        {
            // If there is a player
            if (keyVal.Value != 0)
            {
                changeTiles.Add(new TileChangeData(
                    GetBoardPos(keyVal.Key), 
                    Env.Players.GetPlayerTile(keyVal.Value),
                    Color.white,
                    Matrix4x4.identity
                    ));
            }
        }
        
        // Update tilemap
        _playerTilemap.SetTiles(changeTiles.ToArray(), true);
    }

    public void UpdateActionsTiles(Vector3Int pos)
    {
        var actions = Env.Players.GetActions(_board, pos);
        
        // Clear all tiles
        _actionsTilemap.ClearAllTiles();

        // Initialise change data
        List<TileChangeData> changeTiles = new List<TileChangeData>();
        
        // Get owner
        var owner = Env.Players.GetOwner(_board, pos);
        
        // Loop actions
        foreach (var action in actions)
        {
            // White if empty, red if opposing player
            var color = _board.TryGetValue(action.To, out int to) && to == 0 ?
                Color.white :
                Env.Players.GetOwner(_board, action.To) != owner ?
                    Color.red : 
                    Color.white;
            changeTiles.Add(new TileChangeData(
                (GetBoardPos(action.To)), 
                _actionTile,
                color,
                Matrix4x4.identity
            ));
        }
        
        // Update tilemap
        _actionsTilemap.SetTiles(changeTiles.ToArray(), true);
    }
}
