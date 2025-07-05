using UnityEngine;

public class MapManager : MonoBehaviour
{
    public int CurrentMapID { get; private set; } = -1;

    public delegate void MapChangedDelegate(int _newMapId);
    public event MapChangedDelegate OnMapChanged;

    public void SetCurrentMap(int _mapId)
    {
        if (CurrentMapID == _mapId)
            return;

        CurrentMapID = _mapId;
        OnMapChanged?.Invoke(CurrentMapID);
        Debug.Log($"현재 맵 값 변경 {CurrentMapID}");
    }

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
