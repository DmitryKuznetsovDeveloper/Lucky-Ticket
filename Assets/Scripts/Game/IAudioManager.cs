namespace Game
{
    public interface IAudioManager
    {
        void SetMusicVolume(float volume);
        void SetEffectsVolume(float volume);
        void PlayBackgroundMusic();
        void PlayNextMusicTrack();
        void PlayClickSound();
        void PlayPopupSound();
    }
}