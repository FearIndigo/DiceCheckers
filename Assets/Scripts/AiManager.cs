using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AiManager : MonoBehaviour
{
    public Environment Env;

    [SerializeField] private List<AiAgent> _aiAgents;
    [SerializeField] private GameObject _aiAgentPrefab;
    private Dictionary<Vector3Int,int> _moveMap = new Dictionary<Vector3Int, int>
    {
        [new Vector3Int(0, 1)] = 0,
        [new Vector3Int(0, -1)] = 1,
        [new Vector3Int(-1, 0)] = 2,
        [new Vector3Int(1, 0)] = 3,
        [new Vector3Int(-1, 1)] = 4,
        [new Vector3Int(1, -1)] = 5,
        [new Vector3Int(1, 1)] = 6,
        [new Vector3Int(-1, -1)] = 7,
        [new Vector3Int(-1, 2)] = 8,
        [new Vector3Int(-1, -2)] = 9,
        [new Vector3Int(-2, 1)] = 10,
        [new Vector3Int(-2, -1)] = 11,
        [new Vector3Int(1, 2)] = 12,
        [new Vector3Int(1, -2)] = 13,
        [new Vector3Int(2, 1)] = 14,
        [new Vector3Int(2, -1)] = 15,
    };
    private Dictionary<int, Vector3Int> _moveMapInverted = new Dictionary<int, Vector3Int>
    {
        [0] = new Vector3Int(0, 1),
        [1] = new Vector3Int(0, -1),
        [2] = new Vector3Int(-1, 0),
        [3] = new Vector3Int(1, 0),
        [4] = new Vector3Int(-1, 1),
        [5] = new Vector3Int(1, -1),
        [6] = new Vector3Int(1, 1),
        [7] = new Vector3Int(-1, -1),
        [8] = new Vector3Int(-1, 2),
        [9] = new Vector3Int(-1, -2),
        [10] = new Vector3Int(-2, 1),
        [11] = new Vector3Int(-2, -1),
        [12] = new Vector3Int(1, 2),
        [13] = new Vector3Int(1, -2),
        [14] = new Vector3Int(2, 1),
        [15] = new Vector3Int(2, -1),
    };

    public void Interrupted()
    {
        _aiAgents[0].EpisodeInterrupted();
        _aiAgents[1].EpisodeInterrupted();
        Env.Reset();
    }
    
    public void InferAction()
    {
        var owner = Env.Players.PlayerATurn ? 0 : 1;
        _aiAgents[owner].RequestDecision();
    }

    public float[] GetObservation()
    {
        var board = Env.Board.Board;
        var playerATurn = Env.Players.PlayerATurn;

        if (playerATurn)
        {
            return ConvertBoardToObservation(board);
        }
        else
        {
            var normBoard = BoardFromPlayerBPerspective(board);

            return ConvertBoardToObservation(normBoard);
        }
    }

    private Dictionary<Vector3Int, int> BoardFromPlayerBPerspective(Dictionary<Vector3Int, int> board)
    {
        var normFactor = Env.Board.BoardSize - 1;
        var normBoard = new Dictionary<Vector3Int, int>();
        foreach (var keyVal in board)
        {
            var normKey = keyVal.Key;
            normKey.x = Mathf.Abs(normKey.x - normFactor);
            normKey.y = Mathf.Abs(normKey.y - normFactor);
            int value;
            switch (keyVal.Value)
            {
                case 0:
                default:
                    value = 0;
                    break;
                case 1:
                    value = 2;
                    break;
                case 2:
                    value = 1;
                    break;
                case 3:
                    value = 4;
                    break;
                case 4:
                    value = 3;
                    break;
            }
            normBoard[normKey] = value;
        }

        return normBoard;
    }

    private float[] ConvertBoardToObservation(Dictionary<Vector3Int, int> board)
    {
        var boardSize = Env.Board.BoardSize;
        var cellCount = boardSize * boardSize;
        var observation = new float[cellCount];
        var index = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                var key = new Vector3Int(x, y, 0);

                float value;
                switch (board[key])
                {
                    default:
                        value = 0f;
                        break;
                    case 1:
                        value = 0.5f;
                        break;
                    case 2:
                        value = -0.5f;
                        break;
                    case 3:
                        value = 1f;
                        break;
                    case 4:
                        value = -1f;
                        break;
                }
                observation[index] = value;
                index++;
            }
        }

        return observation;
    }

    public PlayerManager.Action GetAction(int actionIndex)
    {
        var cellIndex = Mathf.FloorToInt(actionIndex / 16f);
        var moveIndex = actionIndex % 16;

        Vector3Int from = GetPositionAtIndex(cellIndex);
        Vector3Int to = from + _moveMapInverted[moveIndex];

        if (!Env.Players.PlayerATurn)
        {
            var normFactor = Env.Board.BoardSize - 1;
            from.x = Mathf.Abs(from.x - normFactor);
            from.y = Mathf.Abs(from.y - normFactor);
            to.x = Mathf.Abs(to.x - normFactor);
            to.y = Mathf.Abs(to.y - normFactor);
        }

        return new PlayerManager.Action(from, to);
    }

    public Dictionary<int, bool> GetActionMasks()
    {
        var board = Env.Players.PlayerATurn ?
            Env.Board.Board :
            BoardFromPlayerBPerspective(Env.Board.Board);
        var actionMasks = new Dictionary<int, bool>();

        var allActions = Env.Players.GetAllPlayerActions(board, 0);

        // Set valid actions
        foreach (var action in allActions)
        {
            var moveIndex = _moveMap[action.To - action.From];
            var actionIndex = GetIndexAtPosition(action.From) * 16 + moveIndex;
            actionMasks[actionIndex] = true;
        }

        return actionMasks;
    }

    public int GetIndexAtPosition(Vector3Int position)
    {
        var boardSize = Env.Board.BoardSize;
        var index = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (position == new Vector3Int(x, y, 0))
                {
                    return index;
                }
                
                index++;
            }
        }

        return index;
    }

    public Vector3Int GetPositionAtIndex(int testIndex)
    {
        var boardSize = Env.Board.BoardSize;
        var index = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (index == testIndex)
                {
                    return new Vector3Int(x, y, 0);
                }

                index++;
            }
        }

        return new Vector3Int(0, 0, 0);
    }
}
