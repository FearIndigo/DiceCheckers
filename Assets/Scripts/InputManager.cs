using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.MLAgents;
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
                if (Env.Players.GetActions(board, pos).Count == 0)
                {
                    Debug.LogWarning("Cannot select player with no available actions.");
                    return;
                }
                
                // Update selected player
                UpdateSelected(pos);
            }
            else
            {
                // Get available actions
                var actions = Env.Players.GetActions(board, _selected);
                
                // Check if we should change selected player
                if (boardValue != 0 && Env.Players.IsPlayersTurn(board, pos) && Env.Players.GetActions(board, pos).Count != 0)
                {
                    // Update selected player
                    UpdateSelected(pos);
                    return;
                }
                
                // Get action
                var action = new PlayerManager.Action(_selected, pos);

                // Unselect player if action is not part of available actions
                if (!actions.Contains(action))
                {
                    // Unselect player
                    UpdateSelected(_unselectedVal);
                    return;
                }
                
                // Perform action on board
                if (Env.Board.PerformAction(action))
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
