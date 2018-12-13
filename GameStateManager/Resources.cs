using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;


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
            //if (content != null)
            //{
                //Songs = new Dictionary<string, Song>(100);
                //SoundEffects = new Dictionary<string, SoundEffect>(1000);
                //Fonts = new Dictionary<string, SpriteFont>(100);
                //Textures = new Dictionary<string, Texture2D>(1000);

                for (int i = 0; i < resourcesList.Songs.Length; i++)
                {
                    string song = resourcesList.Songs[i].ToString();

                    if (Songs.ContainsKey(song) == false)
                        Songs.Add(song, content.Load<Song>(resourcesList.Songs[i]));
                }

                for (int i = 0; i < resourcesList.SoundEffects.Length; i++)
                {
                    string soundEffect = resourcesList.SoundEffects[i].ToString();

                    if (SoundEffects.ContainsKey(soundEffect) == false)
                        SoundEffects.Add(soundEffect, content.Load<SoundEffect>(resourcesList.SoundEffects[i]));
                }

                for (int i = 0; i < resourcesList.Fonts.Length; i++)
                {
                    string font = resourcesList.Fonts[i].ToString();

                    if (Fonts.ContainsKey(font) == false)
                        Fonts.Add(font, content.Load<SpriteFont>(resourcesList.Fonts[i]));
                }

                for (int i = 0; i < resourcesList.Textures.Length; i++)
                {
                    string texture = resourcesList.Textures[i].ToString();

                    if (Textures.ContainsKey(texture) == false)
                        Textures.Add(texture, content.Load<Texture2D>(resourcesList.Textures[i]));
                }


                //using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "Content\\resources.txt"))
                //{
                //    string path = string.Empty;
                //    bool readingSongs = false;
                //    bool readingSoundEffects = false;
                //    bool readingSpriteFonts = false;
                //    bool readingTextures = false;

                //    while (reader.EndOfStream == false)
                //    {
                //        string assetName = reader.ReadLine();

                //        if (assetName == "[Songs]")
                //        {
                //            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\Songs\\";
                //            readingSongs = true;
                //            readingSoundEffects = false;
                //            readingSpriteFonts = false;
                //            readingTextures = false;
                //            assetName = reader.ReadLine();
                //        }
                //        else if (assetName == "[SoundEffects]")
                //        {
                //            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\SoundEffects\\";
                //            readingSongs = false;
                //            readingSoundEffects = true;
                //            readingSpriteFonts = false;
                //            readingTextures = false;
                //            assetName = reader.ReadLine();
                //        }
                //        else if (assetName == "[SpriteFonts]")
                //        {
                //            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\SpriteFonts\\";
                //            readingSongs = false;
                //            readingSoundEffects = false;
                //            readingSpriteFonts = true;
                //            readingTextures = false;
                //            assetName = reader.ReadLine();
                //        }
                //        else if (assetName == "[Textures]")
                //        {
                //            path = AppDomain.CurrentDomain.BaseDirectory + "Content\\Textures\\";
                //            readingSongs = false;
                //            readingSoundEffects = false;
                //            readingSpriteFonts = false;
                //            readingTextures = true;
                //            assetName = reader.ReadLine();
                //        }

                //        if (string.IsNullOrEmpty(assetName))
                //            continue;

                //        if (readingSongs)
                //        {
                //            if (Songs.ContainsKey(assetName) == false)
                //            {
                //                Song song = content.Load<Song>(path + assetName);
                //                Songs.Add(assetName, song);
                //            }
                //        }
                //        else if (readingSoundEffects)
                //        {
                //            if (SoundEffects.ContainsKey(assetName) == false)
                //            {
                //                SoundEffect soundEffect = content.Load<SoundEffect>(path + assetName);
                //                SoundEffects.Add(assetName, soundEffect);
                //            }
                //        }
                //        else if (readingSpriteFonts)
                //        {
                //            if (Fonts.ContainsKey(assetName) == false)
                //            {
                //                SpriteFont font = content.Load<SpriteFont>(path + assetName);
                //                Fonts.Add(assetName, font);
                //            }
                //        }
                //        else if (readingTextures)
                //        {
                //            if (Textures.ContainsKey(assetName) == false)
                //            {
                //                Texture2D texture = content.Load<Texture2D>(path + assetName);
                //                Textures.Add(assetName, texture);
                //            }
                //        }
                //    }
                //}
            //}
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