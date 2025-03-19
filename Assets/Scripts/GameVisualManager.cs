using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
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
        Transform spawnedCrossTransform = Instantiate(_crossPrefab);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        spawnedCrossTransform.position = GetGridWorldPosition(e.x, e.y);
    }

    private Vector3 GetGridWorldPosition(int x, int y)
    {
        return new Vector3(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE, 0);
    }
}
