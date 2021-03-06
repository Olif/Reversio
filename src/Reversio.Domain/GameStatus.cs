﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Reversio.Domain
{
    public class GameStatus
    {
        public GameStatus(
            Guid gameId,
            GameState gameState,
            Board board,
            DiscColor discOfNextMove,
            IReadOnlyList<Position> discsFlipped,
            Move lastValidMove,
            BlackPlayer blackPlayer,
            WhitePlayer whitePlayer
            )
        {
            GameId = gameId;
            CurrentState = board.CurrentState;
            GameState = gameState;
            Debug.WriteLine(board);
            DiscOfNextMove = discOfNextMove;
            DiscsFlipped = discsFlipped;
            LastValidMove = lastValidMove;

            int blackPlayerScore = 0;
            int whitePlayerScore = 0;
            for (var i = 0; i < Board.EdgeSize; i++)
            {
                for (var j = 0; j < Board.EdgeSize; j++)
                {
                    if (CurrentState[i, j] == -1)
                    {
                        blackPlayerScore += 1;
                    }
                    else if(CurrentState[i, j] == 1)
                    {
                        whitePlayerScore += 1;
                    }
                }
            }

            BlackPlayerStatus = new PlayerStatus(blackPlayer.Name, blackPlayerScore);
            WhitePlayerStatus = new PlayerStatus(whitePlayer?.Name ?? "No player", whitePlayerScore);
        }

        public Guid GameId { get; }

        public Move LastValidMove { get; }

        public IReadOnlyList<Position> DiscsFlipped { get; }

        public DiscColor DiscOfNextMove { get; }

        public GameState GameState { get; }

        public int[,] CurrentState { get; }

        public PlayerStatus BlackPlayerStatus { get; }

        public PlayerStatus WhitePlayerStatus { get; }
    }

    public class PlayerStatus
    {
        public PlayerStatus(string name, int score)
        {
            Name = name;
            Score = score;
        }

        public string Name { get; }

        public int Score { get; }
    }
}
