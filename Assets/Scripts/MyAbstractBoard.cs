using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class MyAbstractBoard : AbstractBoard
{
    [SerializeField] private Environment Env;

    private BoardSize boardSize;
    
    private void Awake()
    {
        boardSize = new BoardSize
        {
            Columns = Env.Board.BoardSize,
            Rows = Env.Board.BoardSize,
            NumCellTypes = 5 // 0 = empty, 1 = player A, 2 = player B, 3 = player A upgraded, 4 = player B upgraded
        };
    }

    public override BoardSize GetMaxBoardSize()
    {
        return boardSize;
    }

    public override int GetCellType(int row, int col)
    {
        return Env.Board.Board[new Vector3Int(col, row)];
    }

    public override int GetSpecialType(int row, int col)
    {
        return 0;
    }

    public override bool IsMoveValid(Move m)
    {
        return Env.Board.IsValid(m);
    }

    public override bool MakeMove(Move m)
    {
        return Env.Board.PerformMove(m);
    }
}
