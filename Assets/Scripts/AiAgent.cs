using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;

public class AiAgent : Agent
{
    [SerializeField] private AiAgent OpponentAgent;
    private BehaviorParameters _params;
    private void Awake()
    {
        _params = GetComponent<BehaviorParameters>();
    }

    public void SetOpponentAndTeam(AiAgent opponentAgent, int teamId)
    {
        OpponentAgent = opponentAgent;
        _params.TeamId = teamId;
    }
    
    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect dice roll observation
        sensor.AddOneHotObservation(DiceManager.Instance.Index, DiceManager.Instance.NumMoves);
        // Collect board layout observation
        sensor.AddObservation(AiManager.Instance.GetObservation());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discrete = actions.DiscreteActions; // action index
        
        if (BoardManager.Instance.PerformAction(AiManager.Instance.GetAction(discrete[0])))
        {
            PlayerManager.Instance.ChangePlayerTurn();

            if (BoardManager.Instance.Terminal(BoardManager.Instance.Board))
            {
                AddReward(1);
                OpponentAgent?.AddReward(-1);
                
                EndEpisode();
                OpponentAgent?.EndEpisode();
                
                GameManager.Instance?.Reset();
            }
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var actionMasks = AiManager.Instance.GetActionMasks(); // Action index = valid
        var boardSize = BoardManager.Instance.BoardSize;
        var cellCount = boardSize * boardSize;
        for (int actionIndex = 0; actionIndex < cellCount * 16; actionIndex++)
        {
            actionMask.SetActionEnabled(0, actionIndex, actionMasks.GetValueOrDefault(actionIndex, false));
        }
    }
}
