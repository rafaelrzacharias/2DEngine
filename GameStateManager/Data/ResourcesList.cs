using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace GameStateManager
{
    public class ResourcesList
    {
        [ContentSerializer]
        public List<string> Songs { get; set; }

        [ContentSerializer]
        public List<string> SoundEffects { get; set; }

        [ContentSerializer]
        public List<string> Fonts { get; set; }

        [ContentSerializer]
        public List<string> Textures { get; set; }


        // Parameterless constructor for the content pipeline.
        public ResourcesList() { }
    }


    // The ContentReader is able to construct the object
    // from the datastream the ContentWriter had written.
    public class ResourcesListReader : ContentTypeReader<ResourcesList>
    {
        protected override ResourcesList Read(ContentReader input, ResourcesList existingInstance)
        {
            int length = input.ReadInt32();
            List<string> songs = new List<string>(length);

            for (int i = 0; i < length; i++)
                songs.Add(input.ReadString());

            length = input.ReadInt32();
            List<string> soundEffects = new List<string>(length);

            for (int i = 0; i < length; i++)
                soundEffects.Add(input.ReadString());

            length = input.ReadInt32();
            List<string> fonts = new List<string>(length);

            for (int i = 0; i < length; i++)
                fonts.Add(input.ReadString());

            length = input.ReadInt32();
            List<string> textures = new List<string>(length);

            for (int i = 0; i < length; i++)
                textures.Add(input.ReadString());

            ResourcesList resourcesList = new ResourcesList();
            resourcesList.Songs = songs;
            resourcesList.SoundEffects = soundEffects;
            resourcesList.Fonts = fonts;
            resourcesList.Textures = textures;

            return resourcesList;
        }
    }



}