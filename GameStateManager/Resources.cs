using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;


namespace GameStateManager
{
    public static class Resources
    {
        public static Dictionary<string, Song> Songs = new Dictionary<string, Song>(100);
        public static Dictionary<string, SoundEffect> SoundEffects = new Dictionary<string, SoundEffect>(1000);
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>(100);
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>(1000);


        // Initializes all dictionaries and populates them with the available resources.
        public static void Initialize(ContentManager content, ResourcesList resourcesList)
        {
            string contentFolder = AppDomain.CurrentDomain.BaseDirectory + "Content\\";

                for (int i = 0; i < resourcesList.Songs.Count; i++)
                {
                    string song = resourcesList.Songs[i];

                    if (Songs.ContainsKey(song) == false)
                        Songs.Add(song, content.Load<Song>(contentFolder + "Songs\\" + song));
                }

                for (int i = 0; i < resourcesList.SoundEffects.Count; i++)
                {
                    string soundEffect = resourcesList.SoundEffects[i];

                    if (SoundEffects.ContainsKey(soundEffect) == false)
                        SoundEffects.Add(soundEffect, content.Load<SoundEffect>(contentFolder + "SoundEffects\\" + soundEffect));
                }

                for (int i = 0; i < resourcesList.Fonts.Count; i++)
                {
                    string font = resourcesList.Fonts[i];

                    if (Fonts.ContainsKey(font) == false)
                        Fonts.Add(font, content.Load<SpriteFont>(contentFolder + "SpriteFonts\\" + font));
                }

                for (int i = 0; i < resourcesList.Textures.Count; i++)
                {
                    string texture = resourcesList.Textures[i];

                    if (Textures.ContainsKey(texture) == false)
                        Textures.Add(texture, content.Load<Texture2D>(contentFolder + "Textures\\" + texture));
                }
        }


        // Gets a font with a given name.
        public static SpriteFont GetFont(string fontName)
        {
            if (Fonts.ContainsKey(fontName))
                return Fonts[fontName];

            return Fonts["defaulFont"];
        }


        // Gets a texture with a given name.
        public static Texture2D GetTexture(string textureName)
        {
            if (Textures.ContainsKey(textureName))
                return Textures[textureName];

            return Textures["defaultTexture"];
        }


        // Gets a song with a given name.
        public static Song GetSong(string songName)
        {
            if (Songs.ContainsKey(songName))
                return Songs[songName];

            return Songs["defaultSong"];
        }


        // Gets a sound effect with a given name.
        public static SoundEffect GetSoundEffect(string soundEffectName)
        {
            if (SoundEffects.ContainsKey(soundEffectName))
                return SoundEffects[soundEffectName];

            return SoundEffects["defaultSoundEffect"];
        }
    }
}