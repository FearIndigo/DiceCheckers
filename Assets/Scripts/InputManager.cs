using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    public Environment Env;
    
    private Vector3Int _selected;
    private Vector3Int _unselectedVal = new Vector3Int(-1, -1);
    private Camera _mainCam;
    private bool _aiDeciding;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    public void Reset()
    {
        // Change selected and update available moves visuals
        UpdateSelected(_unselectedVal);
        StopAllCoroutines();
        _aiDeciding = false;
    }

    private IEnumerator AiMoveDelayed()
    {
        _aiDeciding = true;
        yield return new WaitForSeconds(0.5f);
        Env.Ai.InferAction();
        _aiDeciding = false;
    }

    private void FixedUpdate()
    {
        // Let AI make their turn, if the game isn't over
        if (Env.Players.IsAiTurn && !Env.Board.Terminal(Env.Board.Board))
        {
            if (Env.Training)
            {
                Env.Ai.InferAction();
            }
            else if (!_aiDeciding)
            {
                StopAllCoroutines();
                StartCoroutine(nameof(AiMoveDelayed));
            }
            
            return;
        }
    }

    private void Update()
    {
        // Let AI make their turn
        if (Env.Players.IsAiTurn)
        {
            return;
        }
        
        // If left mouse button pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Get position in board-space
            var mousePos = Input.mousePosition;
            var pos = Env.Board.GetBoardPos(
                _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _mainCam.nearClipPlane)),
                false);

            var board = Env.Board.Board;

            // Check it's a valid position on the board
            if (!board.TryGetValue(pos, out int boardValue))
            {
                Debug.LogWarning("Selected an invalid board position: (" + pos + ")");
                return;
            }
            
            // If a player isn't already selected
            if (_selected == _unselectedVal)
            {
                // Check player can be selected
                if (!Env.Players.IsPlayersTurn(board, pos))
                {
                    Debug.LogWarning("Can only select players owned by active player.");
                    return;
                }
                
                // Check there are available actions for this player
                if (Env.Players.GetMoves(board, pos).Count == 0)
                {
                    Debug.LogWarning("Cannot select player with no available actions.");
                    return;
                }
                
                // Update selected player
                UpdateSelected(pos);
            }
            else
            {
                // Check if we should change selected player
                if (boardValue != 0 && Env.Players.IsPlayersTurn(board, pos) && Env.Players.GetMoves(board, pos).Count != 0)
                {
                    // Update selected player
                    UpdateSelected(pos);
                    return;
                }
                
                // Get available moves
                var validMoves = Env.Players.GetMoves(board, _selected);
                
                // Get move index
                var moveIndex = validMoves.FindIndex(m =>
                {
                    var otherCell = m.OtherCell();
                    return otherCell.Column == pos.x && otherCell.Row == pos.y;
                });

                // Unselect player if action is not part of available actions
                if (moveIndex == -1)
                {
                    // Unselect player
                    UpdateSelected(_unselectedVal);
                    return;
                }
                
                // Perform action on board
                if (Env.Board.PerformMove(validMoves[moveIndex]))
                {
                    // Unselect player
                    UpdateSelected(_unselectedVal);
                    
                    // Change player turns
                    Env.Players.ChangePlayerTurn();
                }
            }
        }
    }

    void UpdateSelected(Vector3Int selected)
    {
        _selected = selected;

        Env.Board.UpdateActionsTiles(_selected);
    }
}
