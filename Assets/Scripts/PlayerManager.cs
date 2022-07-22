using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.Serialization;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    public Environment Env;

    [SerializeField] private DictionaryIntInt _playerTileMap;
    [SerializeField] private DictionaryIntInt _playerOwnershipMap;
    [SerializeField] private TileBase[] _playerTiles;
    [SerializeField] private bool _playerATurn;
    [SerializeField] private bool _playerAAi;
    public bool PlayerAAi => _playerAAi;
    [SerializeField] private bool _playerBAi;
    public bool PlayerBAi => _playerBAi;
    public bool HasAI => _playerAAi || _playerBAi;
    public bool IsAiTurn => _playerATurn ? _playerAAi : _playerBAi;
    
    public bool PlayerATurn => _playerATurn;

    public void Reset()
    {
        _playerATurn = true;
    }

    public TileBase GetPlayerTile(int playerValue)
    {
        return _playerTiles[_playerTileMap[playerValue]];
    }

    public void ChangePlayerTurn()
    {
        // Swap the active player
        _playerATurn = !_playerATurn;
        
        if (Env.Training && HasAI && _playerATurn)
        {
            Env.UpdateSteps();
        }
        
        // Change available moveset
        Env.Dice.UpdateMove();
    }

    public List<Vector3Int> GetPlayerPositions(Dictionary<Vector3Int, int> board, int owner)
    {
        var players = new List<Vector3Int>();

        // Loop all positions on board
        foreach (var keyVal in board)
        {
            // If position is owned by player
            if (GetOwner(board, keyVal.Key) == owner)
            {
                // Add to players
                players.Add(keyVal.Key);
            }
        }

        return players;
    }

    public int GetOwner(Dictionary<Vector3Int, int> board, Vector3Int playerPos)
    {
        // Check if its a valid position, and contains a player
        if (!board.TryGetValue(playerPos, out int player) || player == 0)
        {
            //Debug.LogWarning("Cannot get owner for position: (" + playerPos + ")");
            return -1;
        }
        
        return _playerOwnershipMap[player];
    }

    public bool IsPlayersTurn(Dictionary<Vector3Int, int> board, Vector3Int playerPos)
    {
        // Return true if position contains player owned by the active player
        return GetOwner(board, playerPos) == (_playerATurn ? 0 : 1);
    }
    
    public List<Move> GetMoves(Dictionary<Vector3Int, int> board, Vector3Int playerPos)
    {
        var validMoves = new List<Move>();

        // Check position is valid and has a player
        if (!board.TryGetValue(playerPos, out int player) || player == 0)
        {
            Debug.LogWarning("Cannot get action for: (" + playerPos + ").");
            return validMoves;
        }

        var directions = player > 2 ?
            Env.Dice.KingCurrentDirections.Union(Env.Dice.CurrentDirections).ToList() :
            Env.Dice.CurrentDirections;

        // Loop all possible moves
        foreach (var direction in directions)
        {
            var newMove = new Move
            {
                Column = playerPos.x,
                Row = playerPos.y,
                Direction = direction
            };
            if (Env.Board.IsValid(newMove))
            {
                validMoves.Add(newMove);
            }
        }

        // Return valid player moves
        return validMoves;
    }
}
