using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SkillzSDK;

namespace Match3_Evo
{
    public class SoundManager : MonoBehaviour
    {
        static string musicVolumeAudioMixer = "musicVolume";
        static string soundVolumeAudioMixer = "soundVolume";

        
        public bool mute;
        public float musicVolume;
        public float soundVolume;
        public AnimationCurve volumeRemapCurve;
        [SerializeField] AudioMixer audioMixer;
        List<SoundData> soundDatas = new List<SoundData>();

        public float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                musicVolume = value;
                audioMixer.SetFloat(musicVolumeAudioMixer, volumeRemapCurve.Evaluate(musicVolume));
            }
        }

        public float SoundVolume
        {
            get { return soundVolume; }
            set
            {
                soundVolume = value;
                audioMixer.SetFloat(soundVolumeAudioMixer, volumeRemapCurve.Evaluate(soundVolume));
            }
        }

        private void Awake()
        {
            if (GM.soundMng == null)
            {
                GM.soundMng = this;
            }
        }

        private void Start()
        {
#if !UNITY_EDITOR
            SkillzCrossPlatform.setSkillzBackgroundMusic("music.mp3");
#endif
            LoadSettings();
        }

        //private void OnDestroy()
        //{
        //    SaveSettings();
        //}

        public void AddSoundData(SoundData soundData)
        {
            soundDatas.Add(soundData);
        }

        public void SaveSettings()
        {
#if !UNITY_EDITOR
            SkillzCrossPlatform.setSkillzMusicVolume(MusicVolume);
            SkillzCrossPlatform.setSFXVolume(SoundVolume);
#endif
        }

        public void LoadSettings()
        { 
#if !UNITY_EDITOR
            MusicVolume = SkillzCrossPlatform.getSkillzMusicVolume();
            SoundVolume = SkillzCrossPlatform.getSFXVolume();
#endif
        }

        public void PlayDelayed(EnumSoundID soundID, float delay)
        {
            StartCoroutine(PlayDelayedCoroutine(soundID, delay));
        }

        IEnumerator PlayDelayedCoroutine(EnumSoundID soundID, float delay)
        {
            yield return new WaitForSeconds(delay);
            Play(soundID);
        }

        public void Play(EnumSoundID soundID)
        {
            if (!mute)
            {
                for (int i = 0; i < soundDatas.Count; i++)
                {
                    if (soundDatas[i].SoundID == soundID && !soundDatas[i].AudioSource.isPlaying)
                    {
                        soundDatas[i].AudioSource.Play();
                        break;
                    }
                }
            }
        }

        public void Stop(EnumSoundID soundID)
        {
            if (!mute)
            {
                for (int i = 0; i < soundDatas.Count; i++)
                {
                    if (soundDatas[i].SoundID == soundID && soundDatas[i].AudioSource.isPlaying)
                    {
                        soundDatas[i].AudioSource.Stop();
                    }
                }
            }
        }

        public void StopAll()
        {
            StopAllCoroutines();
            for (int i = 0; i < soundDatas.Count; i++)
            {
                soundDatas[i].AudioSource.Stop();
            }
        }
    }
}