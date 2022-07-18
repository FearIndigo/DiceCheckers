using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    [SerializeField] private DictionaryIntInt _normalisationMap;
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

    void EndTrainingRound()
    {
        GameManager.Instance.Reset();
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
}
