using System.Collections.Generic;
using UnityEngine;

using SoundStuff;
using GameStuff;
using System.Threading.Tasks;

public class SoundManager : MonoBehaviour
{
    private AudioSource bgmAudio;
    private AudioSource effectAudio;
    private AudioSource leftstepAudio;
    private AudioSource rightstepAudio;

    [SerializeField] private List<AudioClip> footstepClips;
    [SerializeField] private List<AudioClip> mapBGMClips;
    [SerializeField] private List<AudioClip> playerActionSounds;

    private AudioClip currentMapBGM;
    private GameStuff.Map currentMap;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        bgmAudio = gameObject.AddComponent<AudioSource>();
        effectAudio = gameObject.AddComponent<AudioSource>();
        leftstepAudio = gameObject.AddComponent<AudioSource>();
        rightstepAudio = gameObject.AddComponent<AudioSource>();

        bgmAudio.loop = true;

        bgmAudio.playOnAwake = false;
        effectAudio.playOnAwake = false;
        leftstepAudio.playOnAwake = false;
        rightstepAudio.playOnAwake = false;
    }

    public void PlayMapBGM(GameStuff.Map _mapType)
    {
        currentMap = _mapType;
        int mapIndex = (int)_mapType - 4; // FrontVillage=4, Village=5, Dungeon=6
        
        if (mapIndex >= 0 && mapIndex < mapBGMClips.Count)
        {
            currentMapBGM = mapBGMClips[mapIndex];
            PlayBGM(currentMapBGM);
        }
    }

    public void PlayBossBGM(AudioClip _bossBGM)
    {
        if (_bossBGM != null)
        {
            PlayBGM(_bossBGM);
        }
    }

    public void ReturnToMapBGM()
    {
        if (currentMapBGM != null)
        {
            PlayBGM(currentMapBGM);
        }
    }

    public void PlayPlayerActionSound(PlayerActionType _actionType)
    {
        int index = (int)_actionType;
        if (index >= 0 && index < playerActionSounds.Count)
        {
            PlayEffectOneShot(playerActionSounds[index]);
        }
    }
    
    public void PlayUIClip(AudioClip clip)
    {
        PlayEffectOneShot(clip);
    }

    public void PlayEffectOneShot(AudioClip _audioClip)
    {
        if (_audioClip == null) return;
        effectAudio.PlayOneShot(_audioClip);
    }

    public void PlayEffectOneShot(string _soundName)
    {
        // ResourceLoader로 AudioClip 로드 후 재생
        var audioClip = ResourceLoader.Load<AudioClip>(_soundName, LoadType.AudioClip);
        if (audioClip != null)
        {
            PlayEffectOneShot(audioClip);
        }
    }

    public void PlayEffectSound(SoundType _type)
    {

    }

    public void PlayEffectSound(PlayerActionType _action)
    {

    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        bgmAudio.clip = clip;
        bgmAudio.loop = true;
        bgmAudio.Play();
    }

    public void SetEffectVolume(float _volume)
    {
        effectAudio.volume = _volume;
        leftstepAudio.volume = _volume;
        rightstepAudio.volume = _volume;
    }

    public void SetBGMVolume(float _volume)
    {
        bgmAudio.volume = _volume;
    }

    public void StopAllSounds()
    {
        // BGM 정지
        if (bgmAudio.isPlaying)
            bgmAudio.Stop();
        
        // 이펙트 사운드 정지
        if (effectAudio.isPlaying)
            effectAudio.Stop();
        
        // 발걸음 사운드 정지
        if (leftstepAudio.isPlaying)
            leftstepAudio.Stop();
        if (rightstepAudio.isPlaying)
            rightstepAudio.Stop();
    }

    public async void PlayFootstepSound(FootstepType _stepType, bool _isLeft, float _delayedMilliseconds = 0f)
    {
        await Task.Delay((int)_delayedMilliseconds);

        int step = GetFootstepIndex(_stepType);
    }

    public async void PlayFootstepSound(int _layer, bool _isLeft, float _delayedMilliseconds = 0f)
    {
        await Task.Delay((int)_delayedMilliseconds);

        FootstepType stepType = LayerToFootstepType(_layer);
        int step = GetFootstepIndex(stepType);

        if (_isLeft)
        {
            if (leftstepAudio.isPlaying)
                return;

            leftstepAudio.clip = footstepClips[step];
            leftstepAudio.Play();
        }
        else
        {
            if (rightstepAudio.isPlaying)
                return;

            rightstepAudio.clip = footstepClips[step];
            rightstepAudio.Play();
        }
    }

    private int GetFootstepIndex(FootstepType _stepType)
    {
        switch (_stepType)
        {
            case FootstepType.Grass: return 0;
            case FootstepType.Stone: return 1;
            case FootstepType.Water: return 2;
            case FootstepType.Wood: return 3;
            case FootstepType.Metal: return 4;
            default: return 0;
        }
    }

    private FootstepType LayerToFootstepType(int _layer)
    {
        switch (_layer)
        {
            case 20: return FootstepType.Grass;
            case 21: return FootstepType.Stone;
            case 22: return FootstepType.Water;
            case 23: return FootstepType.Wood;
            case 24: return FootstepType.Metal;
            default: return FootstepType.Grass;
        }
    }
}
