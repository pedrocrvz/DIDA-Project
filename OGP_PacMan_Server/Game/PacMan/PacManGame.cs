﻿using System;
using System.Collections.Generic;
using ClientServerInterface.Client;
using ClientServerInterface.PacMan.Client.Game;
using ClientServerInterface.PacMan.Server;
using ClientServerInterface.Server;
using OGP_PacMan_Server.Game;

namespace OGP_PacMan_Server.Game.PacMan {
    public class PacManGame : IGame<Board> {

        private int numberPlayers;

        private int playerSpeed = 5;
        
        private int topBorder = 40;

        private int lowerBorder = 320;

        private int leftBorder = 0;

        private int rightBorder = 330;

        private int playerSize = 25;

        private int ghostSize = 30;

        private int coinSize = 22;

        private List<ConnectedClient> clients;

        private List<Movement> newMovements;

        public Board State { get; private set; }

        public bool GameEnded { get; private set; }

        private List<Coin> coins;

        private List<ServerGhost> ghosts;

        private List<PacManPlayer> players;

        private List<Wall> walls;

        public PacManGame(int numberPlayer) {
            this.numberPlayers = numberPlayer;
            coins = new List<Coin>();
            ghosts = new List<ServerGhost>();
            players = new List<PacManPlayer>();
            walls = new List<Wall>();
            newMovements = new List<Movement>();
            GameEnded = false;
        }

        public void Start(List<ConnectedClient> clients) {
            int coinCounter = 1;

            List<Ghost> boardGhosts = new List<Ghost>();

            List<PacManPlayer> boardPlayers = new List<PacManPlayer>();

            this.clients = clients;

            //Inicialize Walls
            walls.Add(new Wall(1, new Position(288, 240)));
            walls.Add(new Wall(2, new Position(128, 240)));
            walls.Add(new Wall(3, new Position(248, 40)));
            walls.Add(new Wall(4, new Position(88, 40)));
            //Inicialize Ghosts
            ghosts.Add(new ServerGhost(GhostColor.Red, new Position(180, 73), 1, new Speed(5, 0)));
            ghosts.Add(new ServerGhost(GhostColor.Yellow, new Position(221, 273), 2, new Speed(5, 0)));
            ghosts.Add(new ServerGhost(GhostColor.Pink, new Position(301, 72), 3, new Speed(5, 5)));
            //Inicialize Players
            for (int i = 1; i <= this.clients.Count; i++) {
                players.Add(new PacManPlayer(i, new Position(8, i * 40), Movement.Direction.Stopped, 0, true));
            }
            //Inicializa 1 Column of coins 
            for (int i = 1; i <= 8; i++) {
                coins.Add(new Coin(coinCounter++, new Position(8, i * 40)));
            }
            //Inicializa 2 Column of coins 
            for (int i = 1; i <= 8; i++){
                coins.Add(new Coin(coinCounter++, new Position(48, i * 40)));
            }
            //Inicializa 3 Column of coins 
            for (int i = 1; i <= 5; i++){
                coins.Add(new Coin(coinCounter++, new Position(88, (i * 40) + 120)));
            }
            //Inicializa 4 Column of coins 
            for (int i = 1; i <= 5; i++){
                coins.Add(new Coin(coinCounter++, new Position(128, i * 40)));
            }
            //Inicializa 5 Column of coins
            for (int i = 1; i <= 8; i++){
                coins.Add(new Coin(coinCounter++, new Position(168, i * 40)));
            }
            //Inicializa 6 Column of coins
            for (int i = 1; i <= 8; i++) { 
                coins.Add(new Coin(coinCounter++, new Position(208, i * 40)));
            }
            //Inicializa 7 Column of coins
            for (int i = 1; i <= 5; i++){
                coins.Add(new Coin(coinCounter++, new Position(248, (i * 40) + 120)));
            }
            //Inicializa 8 Column of coins
            for (int i = 1; i <= 5; i++){
                coins.Add(new Coin(coinCounter++, new Position(288, i * 40)));
            }
            //Inicializa 9 Column of coins
            for (int i = 1; i <= 8; i++){
                coins.Add(new Coin(coinCounter++, new Position(328, i * 40)));
            }

            foreach (ServerGhost ghost in ghosts){
                boardGhosts.Add(new Ghost(ghost.Color, ghost.Position, ghost.Id));
            }

            State = new Board(boardGhosts, players, coins);

        }

        public void NextState() {
            int deadCount = 0;

            List<Ghost> boardGhosts = new List<Ghost>();

            List<PacManPlayer> boardPlayers = new List<PacManPlayer>();

            GhostMovement();

            foreach (PacManPlayer player in players ){
                //Check if player is alive
                if (!player.Alive){
                    Console.WriteLine(player.Id);
                    continue;
                }

                PlayerMovement(player);
                
                Coin coin = CheckCoin(player);
                
                if (coin != null) {
                  coins.Remove(coin);
                }
                
                if (CheckPlayerWallCollision(player)){
                    player.Alive = false;
                    deadCount++;
                    continue;
                }

                if (CheckPlayerGhostCollision(player)){
                    deadCount++;
                    player.Alive = false;
                }

            }

            if (deadCount == numberPlayers) {
                GameEnded = true;
            }
            
            foreach (ServerGhost ghost in ghosts) {
                boardGhosts.Add(new Ghost(ghost.Color, ghost.Position, ghost.Id));
            }

            newMovements.Clear();

            State = new Board(boardGhosts, players, coins);
        }

        public void GhostMovement() {
            foreach (ServerGhost ghost in ghosts){
                ghost.Position.X += ghost.Speed.X;
                if (CheckGhostWallCollision(ghost) || CheckGhostBorderCollision(ghost)){
                    ghost.Position.X -= ghost.Speed.X;
                    ghost.Speed.X = -ghost.Speed.X;
                }
                ghost.Position.Y += ghost.Speed.Y;
                if (CheckGhostWallCollision(ghost) || CheckGhostBorderCollision(ghost)){
                    ghost.Position.Y -= ghost.Speed.Y;
                    ghost.Speed.Y = -ghost.Speed.Y;
                }
            }
        }

        public void PlayerMovement(PacManPlayer player) {
            //Player Movements
            lock (newMovements) {
                foreach (Movement movement in newMovements){
                    if (movement.Id == player.Id){
                        switch (movement.Direct){
                            case Movement.Direction.Down:
                                player.Position.Y += playerSpeed;
                                if (CheckPlayerBorderCollision(player)){
                                    player.Position.Y -= playerSpeed;
                                }
                                break;
                            case Movement.Direction.Up:
                                player.Position.Y -= playerSpeed;
                                if (CheckPlayerBorderCollision(player)){
                                    player.Position.Y += playerSpeed;
                                }
                                break;
                            case Movement.Direction.Left:
                                player.Position.X -= playerSpeed;
                                if (CheckPlayerBorderCollision(player)){
                                    player.Position.X += playerSpeed;
                                }
                                break;
                            case Movement.Direction.Right:
                                player.Position.X += playerSpeed;
                                if (CheckPlayerBorderCollision(player)){
                                    player.Position.X -= playerSpeed;
                                }
                                break;
                            case Movement.Direction.Stopped:
                                break;
                        }
                        player.Direction = movement.Direct;
                    }
                }
            }
        }

        public bool CheckPlayerWallCollision(PacManPlayer player) {
            //TODO:Test this
            foreach (Wall wall in walls){
                if (((wall.Position.X - wall.Width <= player.Position.X) && (player.Position.X - playerSize <= wall.Position.X))
                    && ((wall.Position.Y <= player.Position.Y + playerSize) && (player.Position.Y <= wall.Position.Y + wall.Length))){
                    return true;
                }
            }
            return false;
        }

        public bool CheckGhostWallCollision(ServerGhost ghost) {
            //TODO:test
            foreach (Wall wall in walls) {
                if (((wall.Position.X - wall.Width <= ghost.Position.X) && (ghost.Position.X - ghostSize <= wall.Position.X))
                    && ((wall.Position.Y <= ghost.Position.Y + ghostSize) && (ghost.Position.Y <= wall.Position.Y + wall.Length))) {
                    return true;
                }
            }
            return false;
        }

        public bool CheckPlayerGhostCollision(PacManPlayer player) {
            foreach (ServerGhost ghost in ghosts) {
                if (((ghost.Position.X - ghost.Width <= player.Position.X) && (player.Position.X - playerSize <= ghost.Position.X))
                    && ((ghost.Position.Y <= player.Position.Y + playerSize) && (player.Position.Y <= ghost.Position.Y + ghost.Length))){
                    return true;
                }
            }
            return false;
        }

        public bool CheckPlayerBorderCollision(PacManPlayer player) {
            if (((rightBorder < player.Position.X) || (player.Position.X < leftBorder))
                || ((lowerBorder < player.Position.Y) || (player.Position.Y < topBorder))){
                return true;
            }
            return false;
        }

        public bool CheckGhostBorderCollision(ServerGhost ghost) {
            if (((rightBorder < ghost.Position.X) || (ghost.Position.X < leftBorder))
                || ((lowerBorder < ghost.Position.Y) || (ghost.Position.Y < topBorder))) {
                return true;
            }
            return false;
        }

        public Coin CheckCoin(PacManPlayer player) {
            foreach (Coin coin in coins){
                if (((coin.Position.X - coinSize <= player.Position.X) && (player.Position.X - playerSize <= coin.Position.X))
                    && ((coin.Position.Y <= player.Position.Y + playerSize) && (player.Position.Y <= coin.Position.Y + coinSize))) {
                    player.Score += 1;
                    return coin;
                }
            }
            return null;
        }

        public void AddMovements(Movement movement) {
            lock (newMovements){
                newMovements.Add(movement);
            }
        }
        
    }
}