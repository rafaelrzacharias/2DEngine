using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace GameStateManager
{
    // A private class that represents the properties of a sound instance.
    public class ActiveSound
    {
        public string Name;
        public SoundEffectInstance Instance;
        public AudioEmitter Emitter;
    }


    // Audio manager keeps track of what 3D sounds are playing, updating
    // their settings as the camera and entities move around the world, and
    // automatically disposing sound effect instances after they finish playing.
    public static class Audio
    {

        // The scaling factor to be used in the emitters and listener position.
        private const int scalingFactor = 40;

        private static bool isInitialized;

        // Keep track of all the 3D sounds that are currently playing and who is emitting them.
        public static Dictionary<string, List<ActiveSound>> activeSounds;

        // The emitter describes an entity which is making a 3D sound.
        private static AudioEmitter audioEmitter;

        // The listener describes the ear which is hearing 3D sounds.
        private static AudioListener audioListener;


        // Initializes the static AudioManager.
        public static void Initialize()
        {
            if (isInitialized == false)
            {
                audioListener = new AudioListener();
                audioEmitter = new AudioEmitter();
                activeSounds = new Dictionary<string, List<ActiveSound>>();

                foreach (KeyValuePair<string, SoundEffect> soundEffects in Resources.SoundEffects)
                    activeSounds.Add(soundEffects.Key, new List<ActiveSound>());

                isInitialized = true;
            }
        }


        // Plays a song by name. Specify volume and if the song should loop.
        public static void PlaySong(string songName, bool shouldLoop = false, float volume = 1f)
        {
            MediaPlayer.Volume = volume;
            MediaPlayer.IsRepeating = shouldLoop;
            MediaPlayer.Play(Resources.GetSong(songName));
        }


        // Pauses or resumes the currently loaded song.
        public static void PauseOrResumeSong()
        {
            switch(MediaPlayer.State)
            {
                case MediaState.Playing:
                    MediaPlayer.Pause();
                    break;
                case MediaState.Paused:
                    MediaPlayer.Resume();
                    break;
            }
        }


        // Stops the currently loaded song.
        public static void StopSong()
        {
            MediaPlayer.Stop();
        }


        // Changes the volume of the currently loaded song.
        public static float SongVolume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value; }
        }


        // Plays a sound, if the emiiter who requested it is not playing it already.
        public static void PlaySound(string soundName, AudioEmitter emitter, bool shouldLoop = false)
        {
            if (activeSounds.ContainsKey(soundName))
            {
                bool isNewEmitter = true;
                int emitterIndex = 0;

                for (int i = 0; i < activeSounds[soundName].Count; i++)
                {
                    if (activeSounds[soundName][i].Emitter == emitter)
                    {
                        isNewEmitter = false;
                        break;
                    }

                    emitterIndex++;
                }

                if (isNewEmitter)
                {
                    ActiveSound activeSound = new ActiveSound();

                    // Fill in the instance and emitter fields.
                    activeSound.Instance = Resources.GetSoundEffect(soundName).CreateInstance();
                    activeSound.Instance.IsLooped = shouldLoop;
                    activeSound.Emitter = emitter;
                    activeSound.Name = soundName;

                    // Add this sound to the list of active sounds, and play it
                    activeSounds[soundName].Add(activeSound);
                    activeSounds[soundName][emitterIndex].Instance.Play();
                }
            }
        }


        // Pauses a sound by name, from a given emitter
        public static void PauseOrResumeSound(string soundName, AudioEmitter emitter)
        {
            if (activeSounds.ContainsKey(soundName))
            {
                for (int i = 0; i < activeSounds[soundName].Count; i++)
                {
                    ActiveSound activeSound = activeSounds[soundName][i];
                    
                    if (activeSound.Emitter == emitter && activeSound.Instance.State == SoundState.Playing)
                        activeSound.Instance.Pause();
                    else if (activeSound.Emitter == emitter && activeSound.Instance.State == SoundState.Paused)
                        activeSound.Instance.Resume();
                }
            }
        }


        // Stops a sound by name. Specify if it should be stopped immediately or after its done playing.
        public static void StopSound(string soundName, AudioEmitter emitter, bool stopImmediately = false)
        {
            if (activeSounds.ContainsKey(soundName))
            {
                for (int i = 0; i < activeSounds[soundName].Count; i++)
                {
                    ActiveSound activeSound = activeSounds[soundName][i];

                    if (activeSound.Emitter == emitter)
                    {
                        if (activeSound.Instance.State == SoundState.Playing && stopImmediately == false)
                        {
                            activeSound.Instance.IsLooped = false;
                            activeSound.Instance.Dispose();
                        }
                        else
                            activeSound.Instance.Stop(true);
                    }
                }
            }
        }


        // Pauses or resumes all sounds.
        public static void PauseOrResumeAllSounds()
        {
            foreach (List<ActiveSound> sounds in activeSounds.Values)
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    SoundEffectInstance activeSound = sounds[i].Instance;

                    if (activeSound.State == SoundState.Playing)
                    {
                        activeSound.IsLooped = false;
                        activeSound.Pause();
                    }
                    else if (activeSound.State == SoundState.Paused)
                        activeSound.Resume();
                }
            }
        }


        // Stops all sounds. Specify if it should be stopped immediately or after its done playing.
        public static void StopAllSounds(bool stopImmediately)
        {
            foreach (List<ActiveSound> sounds in activeSounds.Values)
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    SoundEffectInstance activeSound = sounds[i].Instance;

                    if (activeSound.State == SoundState.Playing && stopImmediately == false)
                    {
                        activeSound.IsLooped = false;
                        activeSound.Dispose();
                    }
                    else
                        activeSound.Stop(true);
                }
            }
        }


        // Updates the state of the active 3D sounds and dispose them if they are stopped.
        public static void Update(GameTime gameTime)
        {
            // Loop over all currently active 3D sounds.
            foreach (List<ActiveSound> sounds in activeSounds.Values)
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    ActiveSound activeSound = sounds[i];

                    // If the sound is stopped, dispose it and remove from activeSounds
                    if (activeSound.Instance.State == SoundState.Stopped)
                    {
                        activeSound.Instance.Dispose();
                        sounds.RemoveAt(i);
                    }
                    else if (activeSound.Instance.State == SoundState.Playing)
                    {
                        // If the sound is still playing, update its settings
                        audioEmitter.Position = activeSound.Emitter.Position / scalingFactor;                        
                        activeSound.Instance.Apply3D(audioListener, audioEmitter);
                    }
                }
            }
        }


        // Tell the AudioManager about the new camera position.
        // This needs to be called from Game1.Update()
        public static void UpdateListener(/*Camera2D camera*/)
        {
            audioListener.Position = new Vector3(ScreenManager.Viewport.Width / 2f, 
                ScreenManager.Viewport.Height / 2f, 0f) / scalingFactor;
        }
    }
}