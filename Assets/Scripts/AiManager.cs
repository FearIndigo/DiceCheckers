using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    [SerializeField] private QValueSO _qValueSO;
    [SerializeField] private bool _training;
    [SerializeField] private int _numTraining;
    [SerializeField] private DictionaryIntInt _normalisationMap;
    private Dictionary<bool, (Dictionary<Vector3Int, int>, PlayerManager.Action)> last;
    [SerializeField] private float _killReward;
    [SerializeField] private float _winReward;
    [SerializeField] private float _loseReward;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void Reset()
    {
        if (_training)
        {
            Debug.Log("Training Rounds Remaining: " + _numTraining);
            last = new Dictionary<bool, (Dictionary<Vector3Int, int>, PlayerManager.Action)>
            {
                [true] = new (null, new PlayerManager.Action()),
                [false] = new (null, new PlayerManager.Action())
            };
        }
        else
        {
            Debug.Log("Using Q Model with " + _qValueSO.GetCount() + " Weightings");
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
                Debug.Log("Training Complete. QValue Count: " + _qValueSO.GetCount());
                
                
            }
            else
            {
                _numTraining--;
        
                GameManager.Instance.Reset();
            }
        }
    }

    public Dictionary<Vector3Int, int> NormalisedState(Dictionary<Vector3Int, int> board)
    {
        // If player A turn, already normalised
        if (PlayerManager.Instance.PlayerATurn) return board;
        
        // Copy board
        var state = new Dictionary<Vector3Int, int>();
        
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

    public bool InferAction(Dictionary<Vector3Int, int> board)
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
                var previous = last[PlayerManager.Instance.PlayerATurn];
                var newState = NormalisedState(BoardManager.Instance.Board);
                if (BoardManager.Instance.Terminal(newState))
                {
                    // Update model for winning player
                    _qValueSO.UpdateModel(state, action, newState, _winReward);
                    // Update model for losing player
                    _qValueSO.UpdateModel(previous.Item1, previous.Item2, newState, _loseReward);

                    // End training round
                    EndTrainingRound();
                }
                else if (previous.Item1 != null)
                {
                    var prevNumOpponents = PlayerManager.Instance.GetPlayerPositions(state, 1).Count;
                    var newNumOpponents = PlayerManager.Instance.GetPlayerPositions(newState, 1).Count;
                    // Update model when continuing play, small reward for killing opponents
                    _qValueSO.UpdateModel(previous.Item1, previous.Item2, newState, newNumOpponents < prevNumOpponents ? _killReward : 0);
                }
            }
        }

        return valid;
    }
}
