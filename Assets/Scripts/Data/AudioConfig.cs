using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        public AudioClip[] MusicClips; 
        public AudioClip ClickSound;    
        public AudioClip PopupSound;
    }

}