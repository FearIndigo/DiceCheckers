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

    [SerializeField] private Agent[] _aiAgents;

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

    public void GameWinner(int owner)
    {
        if (owner == 0)
        {
            _aiAgents[0].AddReward(1f);
            _aiAgents[1].AddReward(-1f);
        }
        else
        {
            _aiAgents[1].AddReward(1f);
            _aiAgents[0].AddReward(-1f);
        }
        
        _aiAgents[0].EndEpisode();
        _aiAgents[1].EndEpisode();
        
        Env.Reset();
    }
}
