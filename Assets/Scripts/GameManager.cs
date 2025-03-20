using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public PlayerType type;
        public int x;
        public int y;
    }
    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    private PlayerType _localPlayerType;
    private PlayerType _currentPlayablePlayerType;
    public PlayerType LocalPlayerType { get => _localPlayerType; }
    public PlayerType CurrentPlayablePlayerType { get => _currentPlayablePlayerType; }
    
    public void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            _localPlayerType = PlayerType.Cross;
        }
        else
        {
            _localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            _currentPlayablePlayerType = PlayerType.Cross;

            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
    }

    public void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            TriggerOnGameStartedRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnCurrentPlayablePlayerTypeChangeRpc()
    {
        OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    }
    
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(PlayerType type, int x, int y)
    {
        if (type != CurrentPlayablePlayerType) return;
        
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            type = type,
            x = x,
            y = y,
        });

        switch (CurrentPlayablePlayerType)
        {
            default:
            case PlayerType.Circle:
                _currentPlayablePlayerType = PlayerType.Cross;
                break;
            case PlayerType.Cross:
                _currentPlayablePlayerType = PlayerType.Circle;
                break;
        }
        
        TriggerOnCurrentPlayablePlayerTypeChangeRpc();
    }
}
