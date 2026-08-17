using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class NetworkCallbacksDebuggerComponent : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Settings")]
        [SerializeField] private bool _logInputEvents = false;
        private void OnEnable()
        {
            var runner = GetComponent<NetworkRunner>();
            if (runner != null)
            {
                runner.AddCallbacks(this);
            }
        }

        private void OnDisable()
        {
            var runner = GetComponent<NetworkRunner>();
            if (runner != null)
            {
                runner.RemoveCallbacks(this);
            }
        }

        #region Lobby & Connection

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"<color=green>[Fusion] Player Joined:</color> {player} (Me: {runner.LocalPlayer})");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"<color=orange>[Fusion] Player Left:</color> {player}");
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log($"<color=green>[Fusion] Connected to Server.</color> Mode: {runner.GameMode}");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"<color=red>[Fusion] Disconnected from Server.</color> Reason: {reason}");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"<color=red>[Fusion] Shutdown.</color> Reason: {shutdownReason}");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            Debug.Log($"<color=cyan>[Fusion] Connect Request:</color> From {request.RemoteAddress}");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log($"<color=red>[Fusion] Connect Failed.</color> Address: {remoteAddress}, Reason: {reason}");
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"<color=cyan>[Fusion] Session List Updated.</color> Count: {sessionList.Count}");
            foreach (var session in sessionList)
            {
                Debug.Log($" - Session: {session.Name} [{session.PlayerCount}/{session.MaxPlayers}]");
            }
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log($"[Fusion] Auth Response: {data.Count} items.");
        }

        #endregion

        #region Simulation & Scene

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log($"[Fusion AOI] Object {obj.Name} exited AOI for player {player}");
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log($"[Fusion AOI] Object {obj.Name} entered AOI for player {player}");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            Debug.Log($"[Fusion] Simulation Message received.");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            Debug.Log($"[Fusion] Reliable Data received from {player}. Size: {data.Count}");
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
             if (progress >= 1f) Debug.Log($"[Fusion] Reliable Data Progress from {player}: {progress * 100}%");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log($"<color=orange>[Fusion] Host Migration started.</color>");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log($"<color=green>[Fusion] Scene Load Done.</color>");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log($"<color=cyan>[Fusion] Scene Load Start.</color>");
        }

        #endregion

        #region Input (High Frequency)

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (_logInputEvents)
            {
                Debug.Log($"[Fusion Input] Input requested.");
            }
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            if (_logInputEvents)
            {
                Debug.LogWarning($"[Fusion Input] Input missing for {player}.");
            }
        }

        #endregion
    }
}
