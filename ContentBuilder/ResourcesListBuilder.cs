using System;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content;
using GameStateManager;

namespace ContentBuilder
{
    // The Content Importer’s main task is to read the contents
    // of the file and pass it to the content processor.
    [ContentImporter(".xml", DefaultProcessor = "ResourcesListProcessor", DisplayName = "ResourcesList Importer")]
    public class ResourcesListImporter : ContentImporter<XmlDocument>
    {
        public override XmlDocument Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing RES file: " + filename);

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            return doc;
        }
    }


    // The ContentProcessor’s task is to construct the usable object
    // so it can be passed to the ContentWriter.
    [ContentProcessor(DisplayName = "ResourcesList Processor")]
    public class ResourcesListProcessor : ContentProcessor<XmlDocument, ResourcesList>
    {
        public ResourcesListProcessor() { }

        public override ResourcesList Process(XmlDocument input, ContentProcessorContext context)
        {
            try
            {
                context.Logger.LogMessage("Started processing RES file: " + context.OutputFilename);
                context.Logger.LogMessage("Processing songs...");

                XmlNodeList songsList = input.GetElementsByTagName("Songs");
                string[] songs = new string[songsList.Count];

                for (int i = 0; i < songs.Length; i++)
                    songs[i] = songsList[i].Name;

                context.Logger.LogMessage("Processing soundEffects...");

                XmlNodeList soundEffectsList = input.GetElementsByTagName("SoundEffects");
                string[] soundEffects = new string[soundEffectsList.Count];

                for (int i = 0; i < soundEffects.Length; i++)
                    soundEffects[i] = soundEffectsList[i].Name;

                context.Logger.LogMessage("Processing fonts...");

                XmlNodeList fontsList = input.GetElementsByTagName("Fonts");
                string[] fonts = new string[fontsList.Count];

                for (int i = 0; i < fonts.Length; i++)
                    fonts[i] = fontsList[i].Name;

                context.Logger.LogMessage("Processing textures...");

                XmlNodeList texturesList = input.GetElementsByTagName("Textures");
                string[] textures = new string[texturesList.Count];

                for (int i = 0; i < textures.Length; i++)
                    textures[i] = texturesList[i].Name;

                context.Logger.LogMessage("ResourcesList processed successfully!");

                ResourcesList resourcesList = new ResourcesList();
                resourcesList.Songs = songs;
                resourcesList.SoundEffects = soundEffects;
                resourcesList.Fonts = fonts;
                resourcesList.Textures = textures;

                return resourcesList;
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error: " + ex);
                throw;
            }
        }
    }


    // The ContentWriter writes the values into the content file.
    [ContentTypeWriter]
    public class ResourcesListWriter : ContentTypeWriter<ResourcesList>
    {
        protected override void Write(ContentWriter output, ResourcesList value)
        {
            int length = value.Songs.Length;
            output.Write(length);

            for (int i = 0; i < length; i++)
                output.Write(value.Songs[i].ToString());

            length = value.SoundEffects.Length;
            output.Write(length);

            for (int i = 0; i < length; i++)
                output.Write(value.SoundEffects[i].ToString());

            length = value.Fonts.Length;
            output.Write(length);

            for (int i = 0; i < length; i++)
                output.Write(value.Fonts[i].ToString());

            length = value.Textures.Length;
            output.Write(length);

            for (int i = 0; i < length; i++)
                output.Write(value.Textures[i].ToString());
        }


        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(ResourcesList).AssemblyQualifiedName;
        }


        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(ResourcesListReader).AssemblyQualifiedName;
        }
    }


    // The ContentReader is able to construct the object
    // from the datastream the ContentWriter had written.
    public class ResourcesListReader : ContentTypeReader<ResourcesList>
    {
        protected override ResourcesList Read(ContentReader input, ResourcesList existingInstance)
        {
            int length = input.ReadInt32();
            string[] songs = new string[length];

            for (int i = 0; i < length; i++)
                songs[i] = input.ReadString();

            length = input.ReadInt32();
            string[] soundEffects = new string[length];

            for (int i = 0; i < length; i++)
                soundEffects[i] = input.ReadString();

            length = input.ReadInt32();
            string[] fonts = new string[length];

            for (int i = 0; i < length; i++)
                fonts[i] = input.ReadString();

            length = input.ReadInt32();
            string[] textures = new string[length];

            for (int i = 0; i < length; i++)
                textures[i] = input.ReadString();

            ResourcesList resourcesList = new ResourcesList();
            resourcesList.Songs = songs;
            resourcesList.SoundEffects = soundEffects;
            resourcesList.Fonts = fonts;
            resourcesList.Textures = textures;

            return resourcesList;
        }
    }
}