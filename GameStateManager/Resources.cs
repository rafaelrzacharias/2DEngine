using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameStateManager
{
    public static class Resources
    {
        public static Dictionary<string, Song> Songs { get; private set; }
        public static Dictionary<string, SoundEffect> SoundEffects { get; private set; }
        public static Dictionary<string, SpriteFont> Fonts { get; private set; }
        public static Dictionary<string, Texture2D> Textures { get; private set; }


        // Initializes all dictionaries and populates them with the available resources.
        public static void Initialize(ContentManager content)
        {
            if (content != null)
            {
                Textures = new Dictionary<string, Texture2D>(1000);
                Fonts = new Dictionary<string, SpriteFont>(100);
                Songs = new Dictionary<string, Song>(100);
                SoundEffects = new Dictionary<string, SoundEffect>(1000);

                using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "Content\\resources.txt"))
                {
                    string path = string.Empty;
                    bool readingSongs = false;
                    bool readingSoundEffects = false;
                    bool readingSpriteFonts = false;
                    bool readingTextures = false;

                    while (reader.EndOfStream == false)
                    {
                        string assetName = reader.ReadLine();

                        if (assetName == "[Songs]")
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\Songs\\";
                            readingSongs = true;
                            readingSoundEffects = false;
                            readingSpriteFonts = false;
                            readingTextures = false;
                            assetName = reader.ReadLine();
                        }
                        else if (assetName == "[SoundEffects]")
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\SoundEffects\\";
                            readingSongs = false;
                            readingSoundEffects = true;
                            readingSpriteFonts = false;
                            readingTextures = false;
                            assetName = reader.ReadLine();
                        }
                        else if (assetName == "[SpriteFonts]")
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\SpriteFonts\\";
                            readingSongs = false;
                            readingSoundEffects = false;
                            readingSpriteFonts = true;
                            readingTextures = false;
                            assetName = reader.ReadLine();
                        }
                        else if (assetName == "[Textures]")
                        {
                            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\Textures\\";
                            readingSongs = false;
                            readingSoundEffects = false;
                            readingSpriteFonts = false;
                            readingTextures = true;
                            assetName = reader.ReadLine();
                        }

                        if (string.IsNullOrEmpty(assetName))
                            continue;

                        if (readingSongs)
                        {
                            if (Songs.ContainsKey(assetName) == false)
                            {
                                Song song = content.Load<Song>(path + assetName);
                                Songs.Add(assetName, song);
                            }
                        }
                        else if (readingSoundEffects)
                        {
                            if (SoundEffects.ContainsKey(assetName) == false)
                            {
                                SoundEffect soundEffect = content.Load<SoundEffect>(path + assetName);
                                SoundEffects.Add(assetName, soundEffect);
                            }
                        }
                        else if (readingSpriteFonts)
                        {
                            if (Fonts.ContainsKey(assetName) == false)
                            {
                                SpriteFont font = content.Load<SpriteFont>(path + assetName);
                                Fonts.Add(assetName, font);
                            }
                        }
                        else if (readingTextures)
                        {
                            if (Textures.ContainsKey(assetName) == false)
                            {
                                Texture2D texture = content.Load<Texture2D>(path + assetName);
                                Textures.Add(assetName, texture);
                            }
                        }
                    }
                }
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