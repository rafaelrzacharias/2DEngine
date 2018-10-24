using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace GameStateManager
{
    public class AudioStats : DebugScreen
    {
        private string activeSong;
        private string songState;


        // Initializes the AudioStats.
        public override void Initialize()
        {
            base.Initialize();
            Area.Width = (int)(ScreenManager.Viewport.Width * 0.5f);

            // Register 'audio' command if debug command is registered as a service.
            IConsoleHost host = ScreenManager.Game.Services.GetService<IConsoleHost>();

            if (host != null)
                host.RegisterCommand("audio", "Audio Stats", CommandExecute);
        }


        // Prints a list of all audio sources currently playing, their states,
        // total number of sounds loaded, list of emitters, etc.
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();

            int k = 0;
            foreach (List<ActiveSound> sounds in Audio.activeSounds.Values)
                k += sounds.Count;

            Area.Height = Font.LineSpacing * (Resources.Songs.Count + Audio.activeSounds.Count + k + 7);
            SpriteBatch.Draw(Texture, Area, AreaColor);

            TextPosition.Y = 0f;
            SpriteBatch.DrawString(Font, "========== AudioStats ==========", TextPosition, Color.Yellow);
            TextPosition.Y += 2f * Font.LineSpacing;
            
            SpriteBatch.DrawString(Font, "Songs Loaded: " + Resources.Songs.Count, TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            if (MediaPlayer.Queue.ActiveSong != null)
                activeSong = MediaPlayer.Queue.ActiveSong.Name;

            foreach (Song music in Resources.Songs.Values)
            {
                if (music.Name == activeSong)
                {
                    switch (MediaPlayer.State)
                    {
                        case MediaState.Playing:
                            songState = "Playing";
                            break;
                        case MediaState.Paused:
                            songState = "Paused";
                            break;
                        default:
                            songState = "Stopped";
                            break;
                    }
                }
                else
                    songState = "Stopped";

                SpriteBatch.DrawString(Font, "Name: " + music.Name + ", Disposed: " + music.IsDisposed.ToString() + 
                    ", State: " + songState, TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Sounds Loaded: " + Audio.activeSounds.Count, TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            foreach (KeyValuePair<string, SoundEffect> pair in Resources.SoundEffects)
            {
                SpriteBatch.DrawString(Font, "Name: " + pair.Key, TextPosition, Color.White);
                TextPosition.Y += Font.LineSpacing;
            }
            TextPosition.Y += Font.LineSpacing;

            SpriteBatch.DrawString(Font, "Active Sounds: " + k, TextPosition, Color.Yellow);
            TextPosition.Y += Font.LineSpacing;

            foreach (List<ActiveSound> activeSounds in Audio.activeSounds.Values)
            {
                foreach (ActiveSound sound in activeSounds)
                {
                    SpriteBatch.DrawString(Font, "Name: " + sound.Name + ", Emitter: " + sound.Emitter.Position.ToString() + ", State: " +
                        sound.Instance.State.ToString(), TextPosition, Color.White);
                    TextPosition.Y += Font.LineSpacing;
                }
            }

            SpriteBatch.End();
        }
    }
}