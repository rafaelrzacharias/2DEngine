using Microsoft.Xna.Framework.Content;

namespace GameStateManager
{
    public class ResourcesList
    {
        [ContentSerializer]
        public string[] Songs { get; set; }

        [ContentSerializer]
        public string[] SoundEffects { get; set; }

        [ContentSerializer]
        public string[] Fonts { get; set; }

        [ContentSerializer]
        public string[] Textures { get; set; }


        // Parameterless constructor for the content pipeline.
        public ResourcesList() { }
    }
}