using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Ai", fileName = "QValue")]
public class QValueSO : ScriptableObject
{
    [System.Serializable]
    public struct State
    {
        public List<Vector3Int> Positions;
        public List<int> Players;

        public bool TryGetValue(Vector3Int key, out int value)
        {
            if (this.Positions.Contains(key))
            {
                var index = this.Positions.IndexOf(key);
                value = this.Players[index];
                return true;
            }

            value = -1;
            return false;
        }

        public void Set(Vector3Int key, int value)
        {
            if (this.Positions.Contains(key))
            {
                var index = this.Positions.IndexOf(key);
                this.Players[index] = value;
            }
            else
            {
                this.Positions.Add(key);
                this.Players.Add(value);
            }
        }
    }

    [System.Serializable]
    public struct PlayersAction
    {
        public PlayersAction(List<int> players, PlayerManager.Action action)
        {
            this.Players = players;
            this.Action = action;
        }
        public List<int> Players;
        public PlayerManager.Action Action;
    }

    [System.Serializable]
    public struct QValue
    {
        public QValue(PlayersAction playersAction, float q)
        {
            this.PlayersAction = playersAction;
            this.Q = q;
        }
        public PlayersAction PlayersAction;
        public float Q;
    }
    
    [SerializeField]
    private List<QValue> _sq;
    private Dictionary<PlayersAction, float> _q;
    [SerializeField] private float _alpha = 0.5f;
    [SerializeField] private float _epsilon = 0.1f;

    public void SaveBrain()
    {
        Debug.Log("Saving Brain...");
        _sq = new List<QValue>();
        for (int i = 0; i < _q.Count; i++)
        {
            var keyVal = _q.ElementAt(i);
            _sq.Add(new QValue(keyVal.Key, keyVal.Value));
        }
        Debug.Log("Saved brain weights: " + GetCount());
    }
    
    

    public void LoadBrain()
    {
        _q = new Dictionary<PlayersAction, float>();
        if (_sq.Count > 0)
        {
            Debug.Log("Loading Brain...");
            foreach (var keyVal in _sq)
            {
                _q.Add(keyVal.PlayersAction, keyVal.Q);
            }
            Debug.Log("Loaded brain weights: " + GetCount());
        }
        else
        {
            Debug.Log("New Brain");
        }
    }

    public int GetCount()
    {
        return _q.Count;
    }
    
    public float GetQValue(State state, PlayerManager.Action action)
    {
        return _q.GetValueOrDefault(new PlayersAction(state.Players, action), 0);
    }
    
    public void UpdateQValue(State state, PlayerManager.Action action, float oldQ, float reward, float futureRewards)
    {
        var key = new PlayersAction(state.Players, action);
        _q[key] = oldQ + _alpha * ((reward + futureRewards) - oldQ);
    }

    public float BestFutureReward(State state, int owner)
    {
        var bestReward = 0f;

        var allActions = PlayerManager.Instance.GetAllPlayerActions(state, owner);

        if (allActions.Count == 0) return bestReward;

        bestReward = -1f;
        
        // Loop all actions
        foreach (var action in allActions)
        {
            var qValue = GetQValue(state, action);
            if (qValue > bestReward)
            {
                bestReward = qValue;
            }
        }
        
        return bestReward;
    }

    public PlayerManager.Action ChooseAction(State state, int owner, bool epsilon = true)
    {
        // Sort actions for player based on highest q value
        var (highestQ, sortedActions) = SortPlayerActions(state, owner);

        // If epsilon is true and given random probability
        if (epsilon && Random.value < _epsilon)
        {
            // Get actions for random q value
            var randomQActions = sortedActions.ElementAt(Random.Range(0, sortedActions.Count)).Value;
            // Pick random action
            return randomQActions[Random.Range(0, randomQActions.Count)];
        }
        
        Debug.Log(highestQ);
        
        // Return random action with highest q value
        return sortedActions[highestQ][Random.Range(0, sortedActions[highestQ].Count)];
    }

    (float, Dictionary<float, List<PlayerManager.Action>>) SortPlayerActions(State state, int owner)
    {
        var allActions = PlayerManager.Instance.GetAllPlayerActions(state, owner);
        var sortedActions = new Dictionary<float, List<PlayerManager.Action>>();
        var highestQ = -1f;
        // Loop actions
        foreach (var action in allActions)
        {
            var qValue = GetQValue(state, action);
            if (sortedActions.ContainsKey(qValue))
            {
                sortedActions[qValue].Add(action);
            }
            else
            {
                sortedActions[qValue] = new List<PlayerManager.Action> {action};
            }
            
            if (qValue > highestQ)
            {
                highestQ = qValue;
            }
        }
        
        return (highestQ, sortedActions);
    }

    public void UpdateModel(State oldState, PlayerManager.Action action, int owner, State newState, float reward)
    {
        var old = GetQValue(oldState, action);
        var bestFuture = BestFutureReward(newState, owner);
        UpdateQValue(oldState, action, old, reward, bestFuture);
    }
}
