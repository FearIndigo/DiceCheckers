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
    public static InputManager Instance;
    
    private Vector3Int _selected;
    private Vector3Int _unselectedVal = new Vector3Int(-1, -1);
    private Camera _mainCam;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

        _mainCam = Camera.main;
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
        // Change selected and update available moves visuals
        UpdateSelected(_unselectedVal);
    }

    private void Update()
    {
        // Let AI make their turn
        if (!PlayerManager.Instance.IsHumanTurn())
        {
            AiManager.Instance.InferAction();
            Academy.Instance.EnvironmentStep();
            return;
        }
        
        // If left mouse button pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Get position in board-space
            var mousePos = Input.mousePosition;
            var pos = BoardManager.Instance.GetBoardPos(
                _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _mainCam.nearClipPlane)),
                false);

            var board = BoardManager.Instance.Board;

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
                if (!PlayerManager.Instance.IsPlayersTurn(board, pos))
                {
                    Debug.LogWarning("Can only select players owned by active player.");
                    return;
                }
                
                // Check there are available actions for this player
                if (PlayerManager.Instance.GetActions(board, pos).Count == 0)
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
                var actions = PlayerManager.Instance.GetActions(board, _selected);
                
                // Check if we should change selected player
                if (boardValue != 0 && PlayerManager.Instance.IsPlayersTurn(board, pos) && PlayerManager.Instance.GetActions(board, pos).Count != 0)
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
                if (BoardManager.Instance.PerformAction(action))
                {
                    // Unselect player
                    UpdateSelected(_unselectedVal);
                    
                    // Change player turns
                    PlayerManager.Instance.ChangePlayerTurn();
                }
            }
        }
    }

    void UpdateSelected(Vector3Int selected)
    {
        _selected = selected;

        BoardManager.Instance.UpdateActionsTiles(_selected);
    }
}
