using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HeartUnity.AudioDataList;

namespace HeartUnity
{

    public class AudioPlayer
    {
        // static data
        public static AudioSource[] musicSources;
        public static List<AudioUnitData> voiceDatas = new();

        public static void Init(MusicDataList musicList, AudioDataList audioList, AudioDataList[] voiceLists)
        {
            AudioPlayer.audioList = audioList;
            AudioPlayer.musicList = musicList;
            voiceDatas.Clear();
            if (voiceLists != null)
            {
                foreach (var vl in voiceLists)
                {
                    voiceDatas.AddRange(vl.audioDatas);
                }
            }
            CreateSources();
        }

        public static int activeMusicIndex = 0;
        private static MusicUnitData currentMusicData;
        public static List<AudioSource> audioSources = new List<AudioSource>();

        public static void StopMusic()
        {
            // fade out current music if existing, and make it secondary
            FadeAndSwapMusic();
            // nulls current music data
            currentMusicData = null;
            // stops the new primary music data
            musicSource.Stop();
        }

        public static void StopAudio(AudioPlayResult playResult)
        {
            if (playResult.audioSource != null && playResult.audioClip == playResult.audioSource.clip)
            {
                playResult.audioSource.Stop();
            }
        }

        public static float FADE_TIME = 1.0f;
        public static float fadeInTime;
        public static float fadeOutTime;
        private static MusicUnitData fadeOutMusic;

        public static AudioSource musicSource { get { return musicSources[activeMusicIndex]; } }
        public static AudioSource musicSourceSecondary { get { return musicSources[(activeMusicIndex + 1) % 2]; } }
        public static MusicDataList musicList;
        public static AudioDataList audioList;

        public static void ManualUpdate()
        {
            if (currentMusicData != null)
            {
                var volRatio = 1f;
                if (fadeInTime > 0)
                {
                    fadeInTime -= Time.deltaTime;
                    volRatio = Mathf.Min(1f - fadeInTime / FADE_TIME, 1);
                }
                musicSource.volume = volRatio * AudioConfig.masterVolume * AudioConfig.musicVolume * currentMusicData.volumeMultiplier;
            }
            if (fadeOutMusic != null && fadeOutTime > 0)
            {
                fadeOutTime -= Time.deltaTime;
                var volRatio = fadeOutTime / FADE_TIME;
                musicSourceSecondary.volume = volRatio * AudioConfig.masterVolume * AudioConfig.musicVolume * fadeOutMusic.volumeMultiplier;
            }

        }

        public static void PlayMusic(string key, bool loop = true)
        {
            if (currentMusicData?.key == key) return;
            foreach (var mus in musicList.musicDatas)
            {
                if (mus.key == key)
                {
                    FadeAndSwapMusic();
                    currentMusicData = mus;
                    MusicFadeIn(loop, mus);
                }
            }
        }

        private static void MusicFadeIn(bool loop, MusicUnitData mus)
        {
            fadeInTime = FADE_TIME;
            var clip = mus.sourceFile;
            musicSource.Stop();
            musicSource.volume = 0;
            musicSource.clip = clip;
            musicSource.Play();
            musicSource.loop = loop && mus.fadeIntoItself <= 0;
            if (loop && mus.fadeIntoItself > 0)
            {
                musicSource.DOKill();
                var tween = DOTween.Sequence(musicSource).AppendInterval(mus.sourceFile.length - mus.fadeIntoItself).AppendCallback(LoopMusicIntoItself);
            }
        }

        private static void FadeAndSwapMusic()
        {
            if (musicSource.isPlaying)
            {
                FadeMusicOut();
                activeMusicIndex = (activeMusicIndex + 1) % 2;
            }
        }

        private static void FadeMusicOut()
        {
            if (musicSource.isPlaying)
            {
                fadeOutMusic = currentMusicData;
                musicSource.DOKill();
                fadeOutTime = FADE_TIME;
            }

        }

        private static void LoopMusicIntoItself()
        {
            FadeAndSwapMusic();
            MusicFadeIn(true, currentMusicData);
        }

        public static void CreateSources()
        {
            if (musicSources == null || musicSources[0] == null)
            {
                currentMusicData = null;

                musicSources = new AudioSource[2];
                for (int i = 0; i < 2; i++)
                {
                    var go = new GameObject();
                    musicSources[i] = go.AddComponent<AudioSource>();
                    musicSources[i].playOnAwake = false;
                    musicSources[i].loop = true;
                    GameObject.DontDestroyOnLoad(go);
                }

            }
            if (audioSources.Count == 0 || audioSources[0] == null)
            {
                audioSources.Clear();
                for (int i = 0; i < 5; i++)
                {
                    CreateFreeAudioSource();
                }
            }
        }

        private static void CreateFreeAudioSource()
        {
            var go = new GameObject();
            var sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            GameObject.DontDestroyOnLoad(go);
            audioSources.Add(sfxSource);
        }

        static public AudioPlayResult PlayVoice(string v)
        {
            if (string.IsNullOrEmpty(v)) return AudioPlayResult.Empty;
            var volume = AudioConfig.masterVolume * AudioConfig.voiceVolume;
            foreach (var ad in voiceDatas)
            {
                if (ad.key == v)
                {
                    foreach (var source in audioSources)
                    {
                        if (!source.isPlaying)
                        {
                            return PlayWithSource(ad, source, volume);
                        }
                    }
                    return PlayWithSource(ad, audioSources[0], volume);
                }
            }
            return AudioPlayResult.Empty;
        }

        static public void PlaySFXLowPriority(string v)
        {
            var volume = AudioConfig.masterVolume * AudioConfig.sfxVolume;
            foreach (var ad in audioList.audioDatas)
            {
                if (ad.key == v)
                {
                    
                    PlayWithSource(ad, audioSources[audioSources.Count - 1], volume);
                }
            }
        }

        static public void PlaySFX(string v)
        {
            var volume = AudioConfig.masterVolume * AudioConfig.sfxVolume;
            foreach (var ad in audioList.audioDatas)
            {
                if (ad.key == v)
                {
                    foreach (var source in audioSources)
                    {
                        if (!source.isPlaying)
                        {
                            PlayWithSource(ad, source, volume);
                            return;
                        }
                    }
                    PlayWithSource(ad, audioSources[0], volume);
                }
            }

        }

        private static AudioPlayResult PlayWithSource(AudioDataList.AudioUnitData ad, AudioSource audioSource, float volume)
        {
            audioSource.volume = volume * ad.volumeMultiplier;
            audioSource.Stop();
            audioSource.clip = ad.IsMultipleFile ? ad.RandomAudioFile() : ad.sourceFile;
            audioSource.Play();
            return new AudioPlayResult(audioSource.clip.length, audioSource);
        }

        public struct AudioPlayResult
        {
            public static readonly AudioPlayResult Empty = new AudioPlayResult(-1, null);
            public float length;
            public AudioSource audioSource;
            public AudioClip audioClip;

            public AudioPlayResult(float length, AudioSource audioSource)
            {
                this.length = length;
                this.audioSource = audioSource;
                this.audioClip = audioSource == null ? null : audioSource.clip;
            }
        }
    }



}