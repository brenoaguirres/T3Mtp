using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    
    [SerializeField] private Transform _crossPrefab;
    [SerializeField] private Transform _circlePrefab;
    [SerializeField] private Transform _lineCompletePrefab;

    private List<GameObject> _visualGameObjectList;

    private void Awake()
    {
        _visualGameObjectList = new();
    }

    public void Start()
    {
        GameManager.Singleton.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Singleton.OnGameWin += GameManager_OnGameWin;
        GameManager.Singleton.OnRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        float eulerZ = 0f;
        switch (e.line._orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:    eulerZ = 0f; break;
            case GameManager.Orientation.Vertical:      eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA:     eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB:     eulerZ = -45f; break;
        }
        Transform lineCompleteTransform = Instantiate(
            _lineCompletePrefab, 
            GetGridWorldPosition(e.line._centerGridPosition.x, e.line._centerGridPosition.y),
            Quaternion.Euler(0, 0, eulerZ)
        );
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
        _visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnGameRematch(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        foreach (GameObject visualGameObject in _visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        _visualGameObjectList.Clear();
    }
    
    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.type, e.x, e.y);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(GameManager.PlayerType type, int x, int y)
    {
        Transform prefab;
        switch (type)
        {
            default:
            case GameManager.PlayerType.Circle:
                prefab = _circlePrefab;
                break;
            case GameManager.PlayerType.Cross:
                prefab = _crossPrefab;
                break;
        }
        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        _visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE, 0);
    }
}
