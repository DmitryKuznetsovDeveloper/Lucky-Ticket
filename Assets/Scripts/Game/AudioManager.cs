using System;
using Data;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace Game
{
    public sealed class AudioManager : IStartable,IAudioManager, IDisposable
    {
        private readonly AudioSource _musicSource;
        private readonly AudioSource[] _effectSources;
        private readonly AudioConfig _audioConfig;
        private readonly float _fadeDuration;

        private int _currentMusicIndex;
        private int _currentEffectSource;

        private CancellationTokenSource _cts; // Токен для отмены асинхронных операций

        private const string MusicVolumeKey = "MusicVolume";
        private const string EffectsVolumeKey = "EffectsVolume";

        public AudioManager(AudioSource musicSource, AudioSource[] effectSources, AudioConfig audioConfig, float fadeDuration = 1.0f)
        {
            _musicSource = musicSource;
            _effectSources = effectSources;
            _audioConfig = audioConfig;
            _fadeDuration = fadeDuration;
        }
        
        public void Start()
        {
            LoadVolumeSettings();
            if (_audioConfig.MusicClips.Length > 0)
            {
                _currentMusicIndex = UnityEngine.Random.Range(0, _audioConfig.MusicClips.Length);
                PlayBackgroundMusic();
            }
        }

        private void LoadVolumeSettings()
        {
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            float effectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, 1f);

            SetMusicVolume(musicVolume);
            SetEffectsVolume(effectsVolume);
        }

        public void SetMusicVolume(float volume)
        {
            if (_musicSource != null)
            {
                _musicSource.volume = volume;
                PlayerPrefs.SetFloat(MusicVolumeKey, volume);
            }
        }

        public void SetEffectsVolume(float volume)
        {
            foreach (var source in _effectSources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }
            PlayerPrefs.SetFloat(EffectsVolumeKey, volume);
        }

        public void PlayBackgroundMusic()
        {
            _cts?.Cancel(); // Отменяем предыдущую задачу, если она существует
            _cts = new CancellationTokenSource();

            if (_audioConfig.MusicClips.Length == 0)
                return;

            FadeInNewMusic(_audioConfig.MusicClips[_currentMusicIndex], _cts.Token).Forget();
        }

        public void PlayNextMusicTrack()
        {
            _currentMusicIndex = (_currentMusicIndex + 1) % _audioConfig.MusicClips.Length;
            PlayBackgroundMusic();
        }

        private async UniTaskVoid FadeInNewMusic(AudioClip newClip, CancellationToken cancellationToken)
        {
            try
            {
                if (_musicSource == null)
                {
                    Debug.LogWarning("MusicSource is null. Cannot fade in new music.");
                    return;
                }

                // Плавное уменьшение громкости до 0
                while (_musicSource.volume > 0)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    _musicSource.volume -= Time.deltaTime / _fadeDuration;
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                if (_musicSource == null) return;

                _musicSource.clip = newClip;
                _musicSource.Play();

                // Плавное увеличение громкости до 1
                while (_musicSource.volume < 1)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    _musicSource.volume += Time.deltaTime / _fadeDuration;
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("FadeInNewMusic was cancelled.");
            }
        }

        public void PlayClickSound() => PlayEffect(_audioConfig.ClickSound);

        public void PlayPopupSound() => PlayEffect(_audioConfig.PopupSound);

        private void PlayEffect(AudioClip clip)
        {
            if (_effectSources[_currentEffectSource] != null)
            {
                _effectSources[_currentEffectSource].clip = clip;
                _effectSources[_currentEffectSource].Play();
            }
            _currentEffectSource = (_currentEffectSource + 1) % _effectSources.Length;
        }
        
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
