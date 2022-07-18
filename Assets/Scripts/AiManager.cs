using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    private Dictionary<int,AiAgent> _aiAgents;
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

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            Academy.Instance.AutomaticSteppingEnabled = false;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void Setup(bool playerAAi, bool playerBAi)
    {
        if (playerAAi || playerBAi)
        {
            _aiAgents ??= new Dictionary<int, AiAgent>();
            
            if (playerAAi && (!_aiAgents.ContainsKey(0) || _aiAgents[0] == null))
            {
                Debug.Log("Spawning Player A AI...");
                _aiAgents[0] = Instantiate(_aiAgentPrefab).GetComponent<AiAgent>();
                _aiAgents[0].SetOpponentAndTeam(null, 0);
            }

            if (playerBAi && (!_aiAgents.ContainsKey(1) || _aiAgents[1] == null))
            {
                Debug.Log("Spawning Player B AI...");
                _aiAgents[1] = Instantiate(_aiAgentPrefab).GetComponent<AiAgent>();
                _aiAgents[1].SetOpponentAndTeam(null, 1);
            }

            if (_aiAgents.ContainsKey(0) && _aiAgents.ContainsKey(1))
            {
                _aiAgents[0].SetOpponentAndTeam(_aiAgents[1], 0);
                _aiAgents[1].SetOpponentAndTeam(_aiAgents[0], 1);
            }
        }
    }

    public void InferAction()
    {
        var owner = PlayerManager.Instance.PlayerATurn ? 0 : 1;
        _aiAgents[owner].RequestDecision();
    }

    public float[] GetObservation()
    {
        var board = BoardManager.Instance.Board;
        var playerATurn = PlayerManager.Instance.PlayerATurn;

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
        var normFactor = BoardManager.Instance.BoardSize - 1;
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
        var boardSize = BoardManager.Instance.BoardSize;
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

        if (!PlayerManager.Instance.PlayerATurn)
        {
            var normFactor = BoardManager.Instance.BoardSize - 1;
            from.x = Mathf.Abs(from.x - normFactor);
            from.y = Mathf.Abs(from.y - normFactor);
            to.x = Mathf.Abs(to.x - normFactor);
            to.y = Mathf.Abs(to.y - normFactor);
        }

        return new PlayerManager.Action(from, to);
    }

    public Dictionary<int, bool> GetActionMasks()
    {
        var board = PlayerManager.Instance.PlayerATurn ?
            BoardManager.Instance.Board :
            BoardFromPlayerBPerspective(BoardManager.Instance.Board);
        var actionMasks = new Dictionary<int, bool>();

        var allActions = PlayerManager.Instance.GetAllPlayerActions(board, 0);

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
        var boardSize = BoardManager.Instance.BoardSize;
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
        var boardSize = BoardManager.Instance.BoardSize;
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
