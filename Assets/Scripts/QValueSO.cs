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
    private QValueDict q;
    [SerializeField] private float _alpha;
    [SerializeField] private float _epsilon;

    public void SaveBrain(string path)
    {
        Debug.Log("Saving brain at " + path);

        var json = JsonUtility.ToJson(q);
        using StreamWriter writer = new StreamWriter(path);
        writer.Write(json);
        Debug.Log("Saved brain weights: " + GetCount());
    }

    public void LoadBrain(string path)
    {
        if (File.Exists(path))
        {
            Debug.Log("Loading brain from " + path);
            using StreamReader reader = new StreamReader(path);
            var json = reader.ReadToEnd();
            q = JsonUtility.FromJson<QValueDict>(json);
            Debug.Log("Loaded brain weights: " + GetCount());
        }
        else
        {
            Debug.Log("New brain");
            q = new QValueDict();
        }
    }

    public int GetCount()
    {
        return q.Count;
    }
    
    public float GetQValue(StateDict state, PlayerManager.Action action)
    {
        var key = new StateActionDict {[state] = action};
        if (q.ContainsKey(key))
        {
            return q[key];
        }
        
        return 0;
    }
    
    public void UpdateQValue(StateDict state, PlayerManager.Action action, float oldQ, float reward, float futureRewards)
    {
        var key = new StateActionDict {[state] = action};
        if (q.ContainsKey(key))
        {
            Debug.Log("Set");
            q[key] = oldQ + _alpha * ((reward + futureRewards) - oldQ); 
        }
        else
        {
            q.Add(key, oldQ + _alpha * ((reward + futureRewards) - oldQ));
        }
    }

    public float BestFutureReward(StateDict state)
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

    public PlayerManager.Action ChooseAction(StateDict state, bool epsilon = true)
    {
        // Sort actions for player based on highest q value
        var (highestQ, sortedActions) = SortPlayerAActions(state);

        // If epsilon is true and given random probability
        if (epsilon && Random.value < _epsilon)
        {
            // Get actions for random q value
            var randomQActions = sortedActions.ElementAt(Random.Range(0, sortedActions.Count)).Value;
            // Pick random action
            return randomQActions[Random.Range(0, randomQActions.Count)];
        }
        
        // Return random action with highest q value
        return sortedActions[highestQ][Random.Range(0, sortedActions[highestQ].Count)];
    }

    (float, Dictionary<float, List<PlayerManager.Action>>) SortPlayerAActions(StateDict state)
    {
        var allActions = PlayerManager.Instance.GetAllPlayerAActions(state);
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

    public void UpdateModel(StateDict oldState, PlayerManager.Action action, StateDict newState, float reward)
    {
        var old = GetQValue(oldState, action);
        var bestFuture = BestFutureReward(newState);
        UpdateQValue(oldState, action, old, reward, bestFuture);
    }
}
