using System.Collections.Generic;
using UnityEngine;
using BalloonPop.Core;
using BalloonPop.Save;

namespace BalloonPop.Audio
{
    [System.Serializable]
    public class AudioEntry
    {
        public string Key;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Library")]
        [SerializeField] private List<AudioEntry> musicTracks;
        [SerializeField] private List<AudioEntry> sfxClips;

        private const string MusicVolKey = "vol_music";
        private const string SfxVolKey = "vol_sfx";

        private Dictionary<string, AudioEntry> musicMap;
        private Dictionary<string, AudioEntry> sfxMap;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            musicMap = new Dictionary<string, AudioEntry>();
            if (musicTracks != null) foreach (var m in musicTracks) musicMap[m.Key] = m;
            sfxMap = new Dictionary<string, AudioEntry>();
            if (sfxClips != null) foreach (var s in sfxClips) sfxMap[s.Key] = s;

            GenerateProceduralSfx();
            ApplyVolumes();
        }

        private void GenerateProceduralSfx()
        {
            if (!sfxMap.ContainsKey("pop"))
                sfxMap["pop"] = MakeSfx("pop", GeneratePopClip(0.12f, 900f, 400f), 0.6f);
            if (!sfxMap.ContainsKey("pop_big"))
                sfxMap["pop_big"] = MakeSfx("pop_big", GeneratePopClip(0.18f, 700f, 250f), 0.7f);
            if (!sfxMap.ContainsKey("bomb"))
                sfxMap["bomb"] = MakeSfx("bomb", GenerateBombClip(), 0.85f);
            if (!sfxMap.ContainsKey("combo"))
                sfxMap["combo"] = MakeSfx("combo", GenerateChimeClip(), 0.55f);
            if (!sfxMap.ContainsKey("win"))
                sfxMap["win"] = MakeSfx("win", GenerateWinClip(), 0.7f);
            if (!sfxMap.ContainsKey("lose"))
                sfxMap["lose"] = MakeSfx("lose", GenerateLoseClip(), 0.6f);
            if (!sfxMap.ContainsKey("achievement"))
                sfxMap["achievement"] = MakeSfx("achievement", GenerateAchievementClip(), 0.6f);

            if (!musicMap.ContainsKey("menu"))
                musicMap["menu"] = MakeSfx("menu", GenerateMenuMusicClip(), 0.32f);
            if (!musicMap.ContainsKey("game"))
                musicMap["game"] = MakeSfx("game", GenerateGameMusicClip(), 0.28f);
        }

        private static AudioClip GenerateAchievementClip()
        {
            float duration = 0.6f;
            int count = (int)(duration * SR);
            var samples = new float[count];
            float[] notes = { 659f, 880f, 1175f };
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                int n = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / (duration / notes.Length)));
                float subT = (t - n * (duration / notes.Length)) / (duration / notes.Length);
                float env = Mathf.Pow(1f - subT, 1.2f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * 0.55f;
            }
            var clip = AudioClip.Create("achievement", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateMenuMusicClip()
        {
            float duration = 16f;
            int count = (int)(duration * SR);
            var samples = new float[count];

            float[][] chords = {
                new[] { 220f, 277f, 330f },
                new[] { 196f, 247f, 294f },
                new[] { 247f, 311f, 370f },
                new[] { 220f, 277f, 330f },
            };
            float chordDur = duration / chords.Length;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                int chordIdx = Mathf.Min(chords.Length - 1, Mathf.FloorToInt(t / chordDur));
                float v = 0f;
                foreach (var f in chords[chordIdx])
                    v += Mathf.Sin(2f * Mathf.PI * f * t);
                v /= chords[chordIdx].Length;

                float melodyT = (t % 2f) / 2f;
                float[] melody = { 440f, 523f, 587f, 659f, 587f, 523f, 494f, 440f };
                int mIdx = Mathf.Min(melody.Length - 1, Mathf.FloorToInt(melodyT * melody.Length));
                float subT = (melodyT * melody.Length) - mIdx;
                float melEnv = Mathf.Pow(1f - subT, 1.5f);
                v += Mathf.Sin(2f * Mathf.PI * melody[mIdx] * t) * melEnv * 0.5f;

                samples[i] = v * 0.18f;
            }
            var clip = AudioClip.Create("menu_music", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateGameMusicClip()
        {
            float duration = 12f;
            int count = (int)(duration * SR);
            var samples = new float[count];

            float[][] chords = {
                new[] { 196f, 247f, 294f },
                new[] { 220f, 277f, 330f },
                new[] { 247f, 311f, 370f },
                new[] { 196f, 247f, 294f },
            };
            float chordDur = duration / chords.Length;

            float[] bassPattern = { 98f, 98f, 110f, 98f, 123f, 110f, 98f, 110f };

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                int chordIdx = Mathf.Min(chords.Length - 1, Mathf.FloorToInt(t / chordDur));
                float pad = 0f;
                foreach (var f in chords[chordIdx])
                    pad += Mathf.Sin(2f * Mathf.PI * f * t) * 0.25f;

                float beatT = (t * 2f) % 1f;
                int beat = Mathf.FloorToInt(t * 2f) % bassPattern.Length;
                float beatEnv = Mathf.Pow(1f - beatT, 2f);
                float bass = Mathf.Sin(2f * Mathf.PI * bassPattern[beat] * t) * beatEnv * 0.5f;

                samples[i] = (pad + bass) * 0.18f;
            }
            var clip = AudioClip.Create("game_music", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioEntry MakeSfx(string key, AudioClip clip, float volume)
        {
            return new AudioEntry { Key = key, Clip = clip, Volume = volume };
        }

        private const int SR = 22050;

        // ADSR envelope: attack=0.005s hızlı, decay=0.08s ana ses, sustain dB orta, release
        private static float Adsr(float t, float duration, float attack, float decay, float sustainLevel, float release)
        {
            if (t < attack) return Mathf.Clamp01(t / attack);
            if (t < attack + decay) return Mathf.Lerp(1f, sustainLevel, (t - attack) / decay);
            float relStart = duration - release;
            if (t >= relStart) return Mathf.Lerp(sustainLevel, 0f, (t - relStart) / release);
            return sustainLevel;
        }

        // 1-pole low-pass filter (rolling state)
        private static float LowPassStep(float input, ref float state, float alpha)
        {
            state += alpha * (input - state);
            return state;
        }

        private static AudioClip GeneratePopClip(float duration, float startFreq, float endFreq)
        {
            int count = (int)(duration * SR);
            var samples = new float[count];
            float lpState = 0f;
            float noisePhase = Random.Range(0f, 1000f);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                float n = t / duration;
                // Pitch süratle düşer
                float freq = Mathf.Lerp(startFreq, endFreq, Mathf.Sqrt(n));
                // Çoklu harmonik: fundamental + 2x + 3x (mafia/bubble tonu)
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f
                           + Mathf.Sin(2f * Mathf.PI * freq * 2.01f * t) * 0.20f
                           + Mathf.Sin(2f * Mathf.PI * freq * 3.02f * t) * 0.08f;
                // Yumuşak air noise
                float noise = (Mathf.PerlinNoise(t * 800f, noisePhase) - 0.5f) * 0.18f * (1f - n);
                float raw = tone + noise;
                // Düşük geçişle pürüzsüzleştir
                float smoothed = LowPassStep(raw, ref lpState, 0.45f);
                // ADSR
                float env = Adsr(t, duration, 0.005f, 0.04f, 0.35f, 0.08f);
                samples[i] = smoothed * env * 0.75f;
            }
            var clip = AudioClip.Create("pop", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateBombClip()
        {
            float duration = 0.7f;
            int count = (int)(duration * SR);
            var samples = new float[count];
            float lpState = 0f;
            float subState = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                float n = t / duration;
                // Sub-bass thump (40-80 Hz)
                float subFreq = Mathf.Lerp(80f, 38f, n);
                float sub = Mathf.Sin(2f * Mathf.PI * subFreq * t) * 0.9f;
                // Crack/atak: yüksek frekanslı kısa burst
                float crackEnv = Mathf.Pow(1f - n, 6f);
                float crack = (Random.value * 2f - 1f) * crackEnv * 0.7f;
                // Geniş bant noise (lp ile boğuk)
                float noise = (Random.value * 2f - 1f) * 0.5f;
                float boom = LowPassStep(noise, ref lpState, 0.15f);
                // Sub bass'a düşük frekanslı smooth
                float subSmooth = LowPassStep(sub, ref subState, 0.5f);
                // ADSR: keskin attack, uzun decay
                float env = Adsr(t, duration, 0.002f, 0.08f, 0.55f, 0.30f);
                samples[i] = (subSmooth * 0.6f + boom * 0.5f + crack) * env;
            }
            var clip = AudioClip.Create("bomb", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateChimeClip()
        {
            float duration = 0.55f;
            int count = (int)(duration * SR);
            var samples = new float[count];
            // Bell partial'ları (inharmonic, gerçek çan sesine yakın)
            float[] freqs = { 880f, 1320f, 1760f, 2640f };
            float[] amps  = { 1.0f, 0.55f, 0.32f, 0.18f };
            float[] decays = { 1.2f, 1.6f, 2.1f, 2.8f };
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                float v = 0f;
                for (int p = 0; p < freqs.Length; p++)
                {
                    float partialEnv = Mathf.Exp(-decays[p] * t);
                    v += Mathf.Sin(2f * Mathf.PI * freqs[p] * t) * amps[p] * partialEnv;
                }
                samples[i] = v * 0.32f;
            }
            var clip = AudioClip.Create("chime", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateWinClip()
        {
            // Yükselen major chord arpeggio + akor
            float duration = 1.4f;
            int count = (int)(duration * SR);
            var samples = new float[count];
            float[] notes = { 523f, 659f, 784f, 1047f }; // C5 E5 G5 C6
            float arpeggioDur = 0.10f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                float v = 0f;
                // Arpeggio bölümü
                int arpIdx = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t / arpeggioDur));
                if (t < notes.Length * arpeggioDur)
                {
                    float f = notes[arpIdx];
                    float local = (t - arpIdx * arpeggioDur) / arpeggioDur;
                    float arpEnv = Mathf.Pow(1f - local, 1.2f);
                    v += (Mathf.Sin(2f * Mathf.PI * f * t)
                       +  Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.4f) * arpEnv * 0.5f;
                }
                // Bütün akor uzun rezonans
                float chordStart = (notes.Length - 1) * arpeggioDur;
                if (t >= chordStart)
                {
                    float lt = t - chordStart;
                    float remain = duration - chordStart;
                    float env = Mathf.Pow(1f - lt / remain, 1.4f);
                    foreach (var f in notes)
                        v += Mathf.Sin(2f * Mathf.PI * f * t) * 0.18f * env;
                }
                samples[i] = v * 0.45f;
            }
            var clip = AudioClip.Create("win", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateLoseClip()
        {
            // Düşen mini akor (minor 3rd ile hüzünlü)
            float duration = 0.9f;
            int count = (int)(duration * SR);
            var samples = new float[count];
            // İki nota üst üste: ana + minor 3rd alt
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)SR;
                float n = t / duration;
                float pitch = Mathf.Lerp(440f, 180f, Mathf.Pow(n, 0.7f));
                float minor3 = pitch * 0.84f;
                float env = Mathf.Pow(1f - n, 1.5f);
                float v = Mathf.Sin(2f * Mathf.PI * pitch * t) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * minor3 * t) * 0.4f
                        + Mathf.Sin(2f * Mathf.PI * pitch * 0.5f * t) * 0.2f; // sub-bass
                samples[i] = v * env * 0.55f;
            }
            var clip = AudioClip.Create("lose", count, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnEnable()
        {
            GameEvents.OnMatchMade += OnMatch;
            GameEvents.OnLevelWon += OnWin;
            GameEvents.OnLevelLost += OnLose;
            GameEvents.OnComboChain += OnCombo;
        }

        private void OnDisable()
        {
            GameEvents.OnMatchMade -= OnMatch;
            GameEvents.OnLevelWon -= OnWin;
            GameEvents.OnLevelLost -= OnLose;
            GameEvents.OnComboChain -= OnCombo;
        }

        private void OnMatch(int count) => PlaySfx(count >= 4 ? "pop_big" : "pop");
        private void OnWin() => PlaySfx("win");
        private void OnLose() => PlaySfx("lose");
        private void OnCombo(int chain) { if (chain >= 2) PlaySfx("combo"); }

        public void PlayMusic(string key)
        {
            if (musicSource == null || !musicMap.TryGetValue(key, out var entry)) return;
            if (musicSource.clip == entry.Clip && musicSource.isPlaying) return;
            musicSource.clip = entry.Clip;
            musicSource.volume = entry.Volume * MusicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlaySfx(string key)
        {
            if (sfxSource == null || !sfxMap.TryGetValue(key, out var entry)) return;
            sfxSource.PlayOneShot(entry.Clip, entry.Volume * SfxVolume);
        }

        public float MusicVolume
        {
            get => SaveSystem.MusicVolume;
            set { SaveSystem.MusicVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        public float SfxVolume
        {
            get => SaveSystem.SfxVolume;
            set { SaveSystem.SfxVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        private void ApplyVolumes()
        {
            if (musicSource != null) musicSource.volume = MusicVolume;
            if (sfxSource != null) sfxSource.volume = SfxVolume;
        }
    }
}
