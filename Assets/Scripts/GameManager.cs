using System;
using System.Collections.Generic;
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
    
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }
    
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler OnRematch;

    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> _gridVector2IntList;
        public Vector2Int _centerGridPosition;
        public Orientation _orientation;
    }

    private PlayerType _localPlayerType;
    private NetworkVariable<PlayerType> _currentPlayablePlayerType = new NetworkVariable<PlayerType>(PlayerType.None,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public PlayerType LocalPlayerType { get => _localPlayerType; }
    public NetworkVariable<PlayerType> CurrentPlayablePlayerType { get => _currentPlayablePlayerType; }
    private PlayerType[,] _playerTypeArray;
    private List<Line> _lineList;
    
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
        _lineList = new List<Line>
        {
            // Horizontal
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),},
                _centerGridPosition = new Vector2Int(1, 0),
                _orientation = Orientation.Horizontal,
            },
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),},
                _centerGridPosition = new Vector2Int(1, 1),
                _orientation = Orientation.Horizontal,
            },
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2),},
                _centerGridPosition = new Vector2Int(1, 2),
                _orientation = Orientation.Horizontal,
            },
            
            // Vertical
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2),},
                _centerGridPosition = new Vector2Int(0, 1),
                _orientation = Orientation.Vertical,
            },
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2),},
                _centerGridPosition = new Vector2Int(1, 1),
                _orientation = Orientation.Vertical,
            },
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2),},
                _centerGridPosition = new Vector2Int(2, 1),
                _orientation = Orientation.Vertical,
            },
            
            // Diagonal
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2),},
                _centerGridPosition = new Vector2Int(1, 1),
                _orientation = Orientation.DiagonalA,
            },
            new Line
            {
                _gridVector2IntList = new List<Vector2Int>{new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0),},
                _centerGridPosition = new Vector2Int(1, 1),
                _orientation = Orientation.DiagonalB,
            },
        };
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

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            _playerTypeArray[line._gridVector2IntList[0].x, line._gridVector2IntList[0].y],
            _playerTypeArray[line._gridVector2IntList[1].x, line._gridVector2IntList[1].y],
            _playerTypeArray[line._gridVector2IntList[2].x, line._gridVector2IntList[2].y]
            );
    }
    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None
               && aPlayerType == bPlayerType &&
               bPlayerType == cPlayerType;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line l = _lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = l,
            winPlayerType = winPlayerType,
        });
    }
    
    private void TestWinner()
    {
        for (int i=0; i < _lineList.Count; i++)
        {
            Line l = _lineList[i];
            if (TestWinnerLine(l))
            {
                _currentPlayablePlayerType.Value = PlayerType.None;
                TriggerOnGameWinRpc(i, _playerTypeArray[l._centerGridPosition.x, l._centerGridPosition.y]);
                break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < _playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < _playerTypeArray.GetLength(1); y++)
            {
                _playerTypeArray[x, y] = PlayerType.None;
            }
        }

        _currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }
}
