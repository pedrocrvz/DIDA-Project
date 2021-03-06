﻿using System;
using System.Net.Sockets;
using System.Threading;
using ClientServerInterface.PacMan.Server;
using OGPPacManClient.PuppetMaster;
using Timer = System.Timers.Timer;

namespace OGPPacManClient.Client.Movement {
    public abstract class AbstractMovementController {
        private readonly Timer timer;
        private readonly int userId;
        private bool isServerDead;
        private IPacmanServer server;
        private string serverUrl;

        protected AbstractMovementController(IPacmanServer server, string serverUrl, int delta, int userId) {
            this.userId = userId;
            this.server = server;
            this.serverUrl = serverUrl;
            timer = new Timer(delta) {AutoReset = true};
            timer.Elapsed += (sender, args) => NotifyServer();
            isServerDead = true;
        }

        public abstract ClientServerInterface.PacMan.Server.Movement.Direction GetDirection();

        public void Start() {
            timer.Start();
        }

        public void Stop() {
            timer.Stop();
        }

        public void NotifyServer() {
            new Thread(() => {
                try {
                    var dir = GetDirection();
                    if (dir != ClientServerInterface.PacMan.Server.Movement.Direction.Stopped) {
                        var mov = new ClientServerInterface.PacMan.Server.Movement(userId, dir);
                        ClientPuppet.Instance.DoDelay(serverUrl);
                        server.SendAction(mov);
                    }
                    isServerDead = false;
                }
                catch (SocketException) {
                    isServerDead = true;
                    Console.WriteLine("Server is dead");
                }
            }).Start();
        }

        public void setNewServer(IPacmanServer server, string serverUrl) {
            Console.WriteLine("NEW SERVER");
            this.server = server;
            this.serverUrl = serverUrl;
            isServerDead = false;
        }

        public bool GetServerStatus() {
            return isServerDead;
        }
    }
}