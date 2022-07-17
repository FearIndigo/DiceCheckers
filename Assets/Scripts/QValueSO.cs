using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Ai", fileName = "QValue")]
public class QValueSO : ScriptableObject
{
    private Dictionary<(Dictionary<Vector3Int, int>, PlayerManager.Action), float> q;
    [SerializeField] private float _alpha;
    [SerializeField] private float _epsilon;
    
    public int GetCount()
    {
        return q.Count;
    }
    
    public float GetQValue(Dictionary<Vector3Int, int> state, PlayerManager.Action action)
    {
        // Initialise if not done already
        q ??= new Dictionary<(Dictionary<Vector3Int, int>, PlayerManager.Action), float>();
        
        return q.GetValueOrDefault((state, action), 0);
    }
    
    public void UpdateQValue(Dictionary<Vector3Int, int> state, PlayerManager.Action action, float oldQ, float reward, float futureRewards)
    {
        q[(state, action)] = oldQ + _alpha * ((reward + futureRewards) - oldQ);
    }

    public float BestFutureReward(Dictionary<Vector3Int, int> state)
    {
        var bestReward = 0f;

        var allActions = PlayerManager.Instance.GetAllPlayerAActions(state);

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

    public PlayerManager.Action ChooseAction(Dictionary<Vector3Int, int> state, bool epsilon = true)
    {
        // Sort actions for player based on highest q value
        var sortedActions = PlayerManager.Instance.GetAllPlayerAActions(state).OrderByDescending(action => GetQValue(state, action)).ToList();

        // If epsilon is true and given random probability
        if (epsilon && Random.value < _epsilon)
        {
            // Pick random action
            return sortedActions[Random.Range(0, sortedActions.Count)];
        }
        
        // Return action with highest q value
        return sortedActions[0];
    }

    public void UpdateModel(Dictionary<Vector3Int, int> oldState, PlayerManager.Action action, Dictionary<Vector3Int, int> newState, float reward)
    {
        var old = GetQValue(oldState, action);
        var bestFuture = BestFutureReward(newState);
        UpdateQValue(oldState, action, old, reward, bestFuture);
    }
}
