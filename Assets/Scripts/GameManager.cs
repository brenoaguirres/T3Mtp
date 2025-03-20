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
    private NetworkVariable<PlayerType> _currentPlayablePlayerType = new NetworkVariable<PlayerType>(PlayerType.None,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public PlayerType LocalPlayerType { get => _localPlayerType; }
    public NetworkVariable<PlayerType> CurrentPlayablePlayerType { get => _currentPlayablePlayerType; }
    private PlayerType[,] _playerTypeArray;
    
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

        _playerTypeArray = new PlayerType[3, 3];
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
            CurrentPlayablePlayerType.Value = PlayerType.Cross;
            
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        CurrentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
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
    
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(PlayerType type, int x, int y)
    {
        if (type != CurrentPlayablePlayerType.Value) return;

        if (_playerTypeArray[x, y] != PlayerType.None) return;

        _playerTypeArray[x, y] = type;
        
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            type = type,
            x = x,
            y = y,
        });

        switch (CurrentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Circle:
                _currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
            case PlayerType.Cross:
                _currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
        }
        
        TestWinner();
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None
               && aPlayerType == bPlayerType &&
               bPlayerType == cPlayerType;
    }
    private void WinGame()
    {
        Debug.Log("Winner");
        _currentPlayablePlayerType.Value = PlayerType.None;
    }

    private void SpawnRow()
    {
        
    }
    private void TestWinner()
    {
        if (TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) ||
            TestWinnerLine(_playerTypeArray[0, 0], _playerTypeArray[1, 0], _playerTypeArray[2, 0]) 
            )
        {
            SpawnRow();
            WinGame();
        }
    }
}
