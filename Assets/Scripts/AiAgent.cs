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
    public Environment Env;
    [SerializeField] private AiAgent OpponentAgent;
    private BehaviorParameters _params;
    private void Awake()
    {
        _params = GetComponent<BehaviorParameters>();
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect board layout observation
        sensor.AddObservation(Env.Ai.GetObservation());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Ignore if not this players turn
        var owner = Env.Players.PlayerATurn ? 0 : 1;
        if (_params.TeamId != owner)
        {
            Debug.Log("Ai attempting move when not it's turn");
            return;
        }
        
        var discrete = actions.DiscreteActions; // action index
        
        if (Env.Board.PerformAction(Env.Ai.GetAction(discrete[0])))
        {
            Env.Players.ChangePlayerTurn();

            // If game over
            if (Env.Board.Terminal(Env.Board.Board))
            {
                SetReward(1);
                OpponentAgent.SetReward(-1);
                
                EndEpisode();
                OpponentAgent.EndEpisode();

                if (Env.Training)
                {
                    Env.Reset();
                }
            }
        }
        else
        {
            Debug.LogError("Ai performed invalid move!");
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        var actionMasks = Env.Ai.GetActionMasks(); // Action index = valid
        var boardSize = Env.Board.BoardSize;
        var cellCount = boardSize * boardSize;
        for (int actionIndex = 0; actionIndex < cellCount * 16; actionIndex++)
        {
            actionMask.SetActionEnabled(0, actionIndex, actionMasks.GetValueOrDefault(actionIndex, false));
        }
    }
}
