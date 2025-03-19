using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

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
        //Debug.Log(NetworkManager.Singleton.LocalClientId);
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
        }
    }
    
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(PlayerType type, int x, int y)
    {
        //Debug.Log("ClickedOnGridPosition " + x + ", " + y);
        if (type != CurrentPlayablePlayerType) return;
        
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
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
    }
}
