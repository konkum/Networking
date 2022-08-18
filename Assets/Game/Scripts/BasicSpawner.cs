using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private static BasicSpawner _instance;

    public static BasicSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BasicSpawner>();
            }
            return _instance;
        }
    }

    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private NetworkPrefabRef playerPrefab;
    private NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private bool _mouseButton0;

    private void Update()
    {
        _mouseButton0 = _mouseButton0 | InputHolder.Instance.button_0;
    }

    public Dictionary<PlayerRef, NetworkObject> SpawnedCharaters => _spawnedCharacters;

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        var horizontal = joystick.Horizontal;
        var vertical = joystick.Vertical;

        data.direction = Helpers.Camera.transform.right * horizontal;
        data.direction += Helpers.Camera.transform.forward * vertical;

        data.direction = new Vector3(data.direction.x, 0, data.direction.z);

        //Unredlated
        data.camDir = new Vector3(Helpers.Camera.transform.forward.x, 0, Helpers.Camera.transform.forward.z);

        Ray ray = Helpers.Camera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

        data.firePoint = ray.origin;
        data.attactDirection = ray.direction;

        if (_mouseButton0)
        {
            data.button_0 |= NetworkInputData.MOUSEBUTTON0;
        }
        _mouseButton0 = false;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3(player.RawEncoded % runner.Config.Simulation.DefaultPlayers * 3, 2, 0);
            NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    private async void StartGame(GameMode gameMode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(Screen.width / 2, 0, 600, 120), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(Screen.width / 2, 120, 600, 120), "Client"))
            {
                StartGame(GameMode.Client);
            }
            if (GUI.Button(new Rect(Screen.width / 2, 240, 600, 120), "Server"))
            {
                StartGame(GameMode.Server);
            }
        }
    }
}