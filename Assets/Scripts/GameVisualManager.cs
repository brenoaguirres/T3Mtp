using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    
    [SerializeField] private Transform _crossPrefab;
    [SerializeField] private Transform _circlePrefab;

    public void Start()
    {
        GameManager.Singleton.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        //Debug.Log("GameManager_OnClickedOnGridPosition");
        //Debug.Log(GameManager.Singleton.LocalPlayerType);
        SpawnObjectRpc(GameManager.Singleton.LocalPlayerType, e.x, e.y);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(GameManager.PlayerType type, int x, int y)
    {
        //Debug.Log("SpawnObject");
        //Debug.Log(type);
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
        //spawnedCrossTransform.position = GetGridWorldPosition(x, y);
    }

    private Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE, 0);
    }
}
