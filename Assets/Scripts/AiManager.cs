using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    [SerializeField] private QValueSO _qValueSO;
    [SerializeField] private string _brainFilename;
    private string path;
    private string persistentPath;
    [SerializeField] private bool _training;
    [SerializeField] private int _numTraining;
    [SerializeField] private DictionaryIntInt _normalisationMap;
    private Dictionary<bool, (StateDict, PlayerManager.Action)> last;
    [SerializeField] private float _killReward;
    [SerializeField] private float _playerLostReward;
    [SerializeField] private float _winReward;
    [SerializeField] private float _loseReward;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetPaths();
            _qValueSO.LoadBrain(path);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            _qValueSO.SaveBrain(path);
        }
    }

    void SetPaths()
    {
        path = Application.dataPath + Path.AltDirectorySeparatorChar + _brainFilename;
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + _brainFilename;
    }

    public void Reset()
    {
        last = new Dictionary<bool, (StateDict, PlayerManager.Action)>
        {
            [true] = new (null, new PlayerManager.Action()),
            [false] = new (null, new PlayerManager.Action())
        };
        
        if (PlayerManager.Instance.HasAI)
        {
            if (_training)
            {
                Debug.Log("Training Rounds Remaining: " + _numTraining);
            }
            else
            {
                Debug.Log("Using Q Model with " + _qValueSO.GetCount() + " Weightings");
            }
        }
    }

    public void SetTrainingEnabled(bool training)
    {
        _training = training;
    }

    void EndTrainingRound()
    {
        if (_training)
        {
            if (_numTraining == 0)
            {
                _qValueSO.SaveBrain(path);
                
                Debug.Log("Training Complete. QValue Count: " + _qValueSO.GetCount());
            }
            else
            {
                _numTraining--;
        
                GameManager.Instance.Reset();
            }
        }
    }

    public StateDict NormalisedState(StateDict board)
    {
        // If player A turn, already normalised
        if (PlayerManager.Instance.PlayerATurn) return board;
        
        // Copy board
        var state = new StateDict();
        
        // Loop over all state pieces
        foreach (var keyVal in board)
        {
            // Get normalised key
            var normKey = Normalise(keyVal.Key);
            
            // Change values
            state[normKey] = _normalisationMap[keyVal.Value];
        }

        return state;
    }

    private Vector3Int Normalise(Vector3Int val)
    {
        var norm = val;
        norm.y = Mathf.Abs(norm.y - 7);
        norm.x = Mathf.Abs(norm.x - 7);

        return norm;
    }

    private PlayerManager.Action NormaliseAction(PlayerManager.Action action)
    {
        // If player A turn, already normalised
        if (PlayerManager.Instance.PlayerATurn) return action;
        
        var normAction = action;
        normAction.From = Normalise(normAction.From);
        normAction.To = Normalise(normAction.To);

        return normAction;
    }

    public bool InferAction(StateDict board)
    {
        var state = NormalisedState(board);
        var normAction = _qValueSO.ChooseAction(state, _training);
        var playerATurn = PlayerManager.Instance.PlayerATurn;
        var action = NormaliseAction(normAction);
        var valid = BoardManager.Instance.PerformAction(action);

        last[playerATurn] = (state, action);

        if (valid)
        {
            // Change player turns
            PlayerManager.Instance.ChangePlayerTurn();

            // Update model if training
            if (_training)
            {
                var previousOpponentMove = last[PlayerManager.Instance.PlayerATurn];
                var newState = NormalisedState(BoardManager.Instance.Board);
                if (BoardManager.Instance.Terminal(newState))
                {
                    // Update model for winning player
                    _qValueSO.UpdateModel(state, action, newState, _winReward);
                    // Update model for losing player
                    _qValueSO.UpdateModel(previousOpponentMove.Item1, previousOpponentMove.Item2, newState, _loseReward);

                    // End training round
                    EndTrainingRound();
                }
                else if (previousOpponentMove.Item1 != null)
                {
                    var prevNumOpponents = PlayerManager.Instance.GetPlayerPositions(state, 1).Count;
                    var newNumOpponents = PlayerManager.Instance.GetPlayerPositions(newState, 1).Count;
                    // Update move made by player reward
                    _qValueSO.UpdateModel(state, action, newState, newNumOpponents < prevNumOpponents ? _killReward : 0);
                    // Update move made by previous player reward
                    _qValueSO.UpdateModel(previousOpponentMove.Item1, previousOpponentMove.Item2, newState, newNumOpponents < prevNumOpponents ? _playerLostReward : 0);
                }
            }
        }

        return valid;
    }
}
