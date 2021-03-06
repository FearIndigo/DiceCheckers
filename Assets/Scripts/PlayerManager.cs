using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    [System.Serializable]
    public struct Action : IEquatable<Action>
    {
        public Action(Vector3Int from, Vector3Int to)
        {
            this.From = from;
            this.To = to;
        }
        public Vector3Int From;
        public Vector3Int To;

        public bool Equals(Action other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            return obj is Action other && Equals(other);
        }

        public override int GetHashCode()
        {
            int res = 0x2D2816FE;
            res = res * 31 + From.GetHashCode();
            res = res * 31 + To.GetHashCode();
            return res;
        }
    }

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

    public void Set2PlayerMode()
    {
        _playerAAi = false;
        _playerBAi = false;
    }

    public void Set1PlayerMode()
    {
        _playerAAi = false;
        _playerBAi = true;
    }

    public void Set0PlayerMode()
    {
        _playerAAi = true;
        _playerBAi = true;
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
    
    public List<Action> GetActions(Dictionary<Vector3Int, int> board, Vector3Int playerPos)
    {
        var actions = new List<Action>();

        // Check position is valid and has a player
        if (!board.TryGetValue(playerPos, out int player) || player == 0)
        {
            Debug.LogWarning("Cannot get action for: (" + playerPos + ").");
            return actions;
        }

        var allPlayerMoves = player > 2 ? Env.Dice.KingCurrentMove.Union(Env.Dice.CurrentMove).ToList() : Env.Dice.CurrentMove;

        // Loop all possible moves
        foreach (var move in allPlayerMoves)
        {
            var to = playerPos + move;
            // Check To position is valid and doesnt have player on the same team
            if (board.TryGetValue(to, out int toVal) && (toVal == 0 || _playerOwnershipMap[toVal] != _playerOwnershipMap[player]))
            {
                // Add to potential actions
                actions.Add(new Action(playerPos, to));
            }
        }

        // Return valid player actions
        return actions;
    }
    
    public List<Action> GetAllPlayerActions(Dictionary<Vector3Int, int> board, int owner)
    {
        var allActions = new List<Action>();
        
        var playerPositions = GetPlayerPositions(board, owner);

        // Loop all player positions
        foreach (var playerPos in playerPositions)
        {
            var actions = GetActions(board, playerPos);
            if (actions.Count != 0)
            {
                allActions.AddRange(actions);
            }
        }

        return allActions;
    }
}
