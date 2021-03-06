﻿using System;
using FluentAssertions;
using Reversio.Domain.Events;
using Xunit;

namespace Reversio.Domain.UnitTest
{
    public class GameEngineSpecs
    {
        private GameEngine _sut;

        public GameEngineSpecs()
        {
            _sut = new GameEngine();
        }

        [Fact]
        public void Cannot_put_non_registered_player_in_waiting_queue()
        {
            var player = new BlackPlayer("not registered");
            Action joinGame = () => _sut.CreateNewGame(player);

            joinGame.ShouldThrow<PlayerNotRegisteredException>();
        }

        [Fact]
        public void Creates_new_game_when_two_participants_has_joined_the_waiting_queue()
        {
            bool partipant1JoinedGame = false;
            bool participant2JoinedGame = false;
            var participant1 = new Player("p1");
            var participant2 = new Player("p2");
            _sut.RegisterPlayer(participant1);
            _sut.RegisterPlayer(participant2);
            _sut.GameStarted += (object sender, Player participant, GameStartedEventArgs args) =>
            {
                if (participant1 == participant)
                {
                    partipant1JoinedGame = true;
                }

                if (participant2 == participant)
                {
                    participant2JoinedGame = true;
                }
            };

            _sut.PutPlayerInQueue(participant1);
            _sut.PutPlayerInQueue(participant2);

            partipant1JoinedGame.Should().BeTrue();
            participant2JoinedGame.Should().BeTrue();
        }

        [Fact]
        public void Player_can_make_a_move_if_it_is_the_players_turn()
        {
            var part1 = new BlackPlayer("p1");
            var part2 = new WhitePlayer("p2");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            var gameState = _sut.CreateNewGame(part1);
            _sut.JoinGame(gameState.GameId, part2);

            var flippedPieces = _sut.MakeMove(gameState.GameId, part1, new Position(2, 3));

            flippedPieces.Should().NotBeNull();
        }

        [Fact]
        public void Player_cannot_make_a_move_if_it_is_not_the_players_turn()
        {
            var part1 = new BlackPlayer("p1");
            var part2 = new WhitePlayer("p2");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            var gameState = _sut.CreateNewGame(part1);
            _sut.JoinGame(gameState.GameId, part2);

            var flippedPieces = _sut.MakeMove(gameState.GameId, part2, new Position(5, 3));

            flippedPieces.Should().BeNull();
        }

        [Fact]
        public void A_player_cannot_make_a_move_when_he_is_not_part_of_the_game()
        {
            var part1 = new BlackPlayer("p1");
            var part2 = new WhitePlayer("p2");
            var part3 = new BlackPlayer("p3");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            _sut.RegisterPlayer(part3);
            var gameState = _sut.CreateNewGame(part1);
            _sut.JoinGame(gameState.GameId, part2);

            Action moveOnWrongGame = () => _sut.MakeMove(gameState.GameId, part3, new Position(5, 3));

            moveOnWrongGame.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void A_player_can_invite_another_non_active_player_to_game()
        {
            bool playerInvited = false;
            var part1 = new Player("1");
            var part2 = new Player("2");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            _sut.PlayerInvitedToNewGame += (sender, args) => playerInvited = true;

            var isChallangeSent = _sut.TryInvitePlayerToGame(part1, part2);

            playerInvited.Should().BeTrue();
            isChallangeSent.Should().BeTrue();
        }

        [Fact]
        public void A_player_cannot_invite_a_player_that_is_already_playing_a_game()
        {
            bool playerInvited = false;
            var part1 = new Player("2");
            var part2 = new Player("3");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            _sut.CreateNewGame(part2);
            _sut.PlayerInvitedToNewGame += (sender, args) => playerInvited = true;

            var isChallangeSent = _sut.TryInvitePlayerToGame(part1, part2);

            playerInvited.Should().BeFalse();
            isChallangeSent.Should().BeFalse();
        }

        [Fact]
        public void An_invited_player_can_accept_the_invitation_which_starts_a_new_game()
        {
            var playerInvited = false;
            var part1GotNewGameEvent = false;
            var part2GotNewGameEvent = false;
            var newGameStarted = false;
            var part1 = new Player("4");
            var part2 = new Player("5");
            _sut.RegisterPlayer(part1);
            _sut.RegisterPlayer(part2);
            _sut.PlayerInvitedToNewGame += (sender, args) => playerInvited = true;
            _sut.GameStarted += (sender, participant, args) =>
            {
                newGameStarted = true;
                if (participant == part1)
                {
                    part1GotNewGameEvent = true;
                }
                if (participant == part2)
                {
                    part2GotNewGameEvent = true;
                }
            };
            var isChallangeSent = _sut.TryInvitePlayerToGame(part1, part2);

            _sut.InvitationResponse(part2, part1, true);

            isChallangeSent.Should().BeTrue();
            playerInvited.Should().BeTrue();
            newGameStarted.Should().BeTrue();
            part1GotNewGameEvent.Should().BeTrue();
            part2GotNewGameEvent.Should().BeTrue();
        }

        [Fact]
        public void When_game_is_finished_it_is_removed()
        {
            var positions = new char[8, 8]
            {
                {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

                {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

                {' ', ' ', 'X', ' ', 'X', ' ', ' ', ' '},

                {' ', ' ', 'X', 'O', 'X', ' ', ' ', ' '},

                {' ', ' ', 'X', 'X', 'X', ' ', ' ', ' '},

                {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

                {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},

                {' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '},
            };

            bool isFinished = false;
            var board = new Board(positions.Translate());
            var blackPlayer = new BlackPlayer("black");
            var whitePlayer = new WhitePlayer("white");
            _sut.RegisterPlayer(blackPlayer);
            _sut.RegisterPlayer(whitePlayer);
            var game = new Game(blackPlayer, board);
            game.JoinOpponent(whitePlayer);
            _sut.AddGame(game);
            _sut.GameStateChanged += (sender, participant, args) =>
            {
                if (game.GameId == args.CurrentState.GameId)
                {
                    isFinished = args.CurrentState.GameState == GameState.Finished;
                }
            };

            var finishedState = _sut.MakeMove(game.GameId, blackPlayer, new Position(3, 2));

            finishedState.Should().NotBeEmpty();
            isFinished.Should().BeTrue();
            _sut.ActiveGames.Should().NotContain(x => x.GameId == game.GameId);
        }

        [Fact]
        public void When_An_Invitation_Is_Sent_To_A_Player_And_The_Player_Already_Has_Sent_An_Invitation_Starts_A_Game()
        {
            bool gameStarted = false;
            var blackPlayer = new BlackPlayer("invA");
            var whitePlayer = new WhitePlayer("invB");
            _sut.RegisterPlayer(blackPlayer);
            _sut.RegisterPlayer(whitePlayer);
            _sut.GameStarted += (sender, participant, args) =>
            {
                if (participant == blackPlayer || participant == whitePlayer)
                {
                    gameStarted = true;
                }
            };

            _sut.TryInvitePlayerToGame(blackPlayer, whitePlayer);
            _sut.TryInvitePlayerToGame(whitePlayer, blackPlayer);

            gameStarted.Should().BeTrue();

        }
    }
}
