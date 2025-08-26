using UnityEngine;
using GameStuff;

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
        
        // 맵 변경 시 BGM 변경
        ChangeMapBGM(_mapId);
        
        Debug.Log($"맵이 변경되었습니다 {CurrentMapID}");
    }
    
    private void ChangeMapBGM(int _mapId)
    {
        var soundManager = Singleton.Get<SoundManager>();
        if (soundManager != null)
        {
            // 맵 ID를 Map enum으로 변환
            GameStuff.Map mapType = (GameStuff.Map)_mapId;
            soundManager.PlayMapBGM(mapType);
        }
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
