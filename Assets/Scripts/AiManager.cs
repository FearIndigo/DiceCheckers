using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    [SerializeField] private QValueSO _qValueSO;
    [SerializeField] private bool _training;
    public bool Training => _training;
    [SerializeField] private int _numTraining;
    private int _numTrainingRemaining;
    [SerializeField] private DictionaryIntInt _normalisationMap;
    private Dictionary<bool, (QValueSO.State, PlayerManager.Action)> last;
    [SerializeField] private float _winReward;
    [SerializeField] private float _loseReward;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _qValueSO.LoadBrain();
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
            _qValueSO.SaveBrain();
        }
    }

    public void Reset()
    {
        last = new Dictionary<bool, (QValueSO.State, PlayerManager.Action)>
        {
            [true] = new (default, new PlayerManager.Action()),
            [false] = new (default, new PlayerManager.Action())
        };
        
        if (PlayerManager.Instance.HasAI)
        {
            if (_training)
            {
                Debug.Log("Training Rounds Remaining: " + _numTrainingRemaining);
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
        _numTrainingRemaining = _numTraining;
    }

    void EndTrainingRound()
    {
        if (_training)
        {
            if (_numTrainingRemaining == 0)
            {
                _qValueSO.SaveBrain();
                
                Debug.Log("Training Complete. QValue Count: " + _qValueSO.GetCount());
            }
            else
            {
                _numTrainingRemaining--;
        
                GameManager.Instance.Reset();
            }
        }
    }

    public QValueSO.State NormalisedState(QValueSO.State board)
    {
        // If player A turn, already normalised
        if (PlayerManager.Instance.PlayerATurn) return board;
        
        // Copy board
        var state = new QValueSO.State();
        state.Positions = new List<Vector3Int>();
        state.Players = new List<int>();
        
        // Loop over all state pieces
        for (int i = 0; i < board.Positions.Count; i++)
        {
            // Get normalised key
            var normKey = Normalise(board.Positions[i]);
            
            // Change values
            state.Positions.Add(normKey);
            state.Players.Add(_normalisationMap[board.Players[i]]);
        }

        return state;
    }

    private Vector3Int Normalise(Vector3Int val)
    {
        var norm = val;
        norm.y = Mathf.Abs(norm.y - (BoardManager.Instance.BoardSize - 1));
        norm.x = Mathf.Abs(norm.x - (BoardManager.Instance.BoardSize - 1));

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

    public bool InferAction(QValueSO.State board)
    {
        //var state = NormalisedState(board);
        var state = board;
        var playerATurn = PlayerManager.Instance.PlayerATurn;
        var owner = playerATurn ? 0 : 1;
        var normAction = _qValueSO.ChooseAction(state, owner, _training);
        
        //var action = NormaliseAction(normAction);
        var action = normAction;
        var valid = BoardManager.Instance.PerformAction(state, action);
        
        if (valid)
        {
            last[playerATurn] = (state, action);
            //var newState = NormalisedState(BoardManager.Instance.Board);
            var newState = BoardManager.Instance.Board;
            var previousOpponentMove = last[!PlayerManager.Instance.PlayerATurn];
            // Change player turns
            PlayerManager.Instance.ChangePlayerTurn();

            // Update model if training
            if (_training)
            {
                if (BoardManager.Instance.Terminal(newState))
                {
                    // Update model for winning player
                    _qValueSO.UpdateModel(state, action, owner, newState, _winReward);
                    // Update model for losing player
                    _qValueSO.UpdateModel(previousOpponentMove.Item1, previousOpponentMove.Item2, owner % 1, state, _loseReward);

                    // End training round
                    EndTrainingRound();
                }
                else if (previousOpponentMove.Item1.Positions != null)
                {
                    // Update previous opponent move
                    _qValueSO.UpdateModel(previousOpponentMove.Item1, previousOpponentMove.Item2, owner % 1, state, 0);
                }
            }
        }

        return valid;
    }
}
