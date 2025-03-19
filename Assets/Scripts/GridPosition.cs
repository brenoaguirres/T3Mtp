using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private int _x;
    [SerializeField] private int _y;
    private void OnMouseDown()
    {
        Debug.Log("Click: " + _x + ", " + _y);
        GameManager.Singleton.ClickedOnGridPositionRpc(GameManager.Singleton.LocalPlayerType, _x, _y);
    }
}
