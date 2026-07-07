using UnityEngine;

namespace InkJam.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private AudioClip _clipSlide;
        private AudioClip _clipExit;
        private AudioClip _clipBleed;
        private AudioClip _clipWin;
        private AudioClip _clipFail;

        [Header("SFX Volume")]
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _musicSource = gameObject.AddComponent<AudioSource>();
            
            _sfxSource.playOnAwake = false;
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            LoadClips();
        }

        private void LoadClips()
        {
            _clipSlide = Resources.Load<AudioClip>("Audio/slide");
            _clipExit = Resources.Load<AudioClip>("Audio/exit");
            _clipBleed = Resources.Load<AudioClip>("Audio/bleed");
            _clipWin = Resources.Load<AudioClip>("Audio/win");
            _clipFail = Resources.Load<AudioClip>("Audio/fail");

            if (_clipSlide == null) Debug.LogWarning("[AudioManager] Missing Audio/slide.wav");
            if (_clipExit == null) Debug.LogWarning("[AudioManager] Missing Audio/exit.wav");
            if (_clipBleed == null) Debug.LogWarning("[AudioManager] Missing Audio/bleed.wav");
            if (_clipWin == null) Debug.LogWarning("[AudioManager] Missing Audio/win.wav");
            if (_clipFail == null) Debug.LogWarning("[AudioManager] Missing Audio/fail.wav");
        }

        public void PlaySlide() => PlaySFX(_clipSlide);
        public void PlayExit() => PlaySFX(_clipExit);
        public void PlayBleedSpread() => PlaySFX(_clipBleed, 0.8f);
        public void PlayWin() => PlaySFX(_clipWin);
        public void PlayFail() => PlaySFX(_clipFail);

        private void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
            }
        }
    }
}
