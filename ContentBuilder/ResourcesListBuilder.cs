using System;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using GameStateManager;

namespace ContentBuilder
{
    // The Content Importer’s main task is to read the contents
    // of the file and pass it to the content processor.
    [ContentImporter(".res", DefaultProcessor = "ResourcesListProcessor", DisplayName = "ResourcesList Importer")]
    public class ResourcesListImporter : ContentImporter<XmlDocument>
    {
        public override XmlDocument Import(string filename, ContentImporterContext context)
        {
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
                List<string> songs = new List<string>();
                List<string> soundEffects = new List<string>();
                List<string> fonts = new List<string>();
                List<string> textures = new List<string>();

                XmlNode root = input.DocumentElement.FirstChild;
                XmlNodeList nodes = root.ChildNodes;

                foreach (XmlNode node in nodes)
                {
                    if (node.Name == "Songs")
                    {
                        XmlNodeList songsNode = node.ChildNodes;

                        for (int i = 0; i < songsNode.Count; i++)
                            songs.Add(songsNode[i].InnerText);
                    }

                    if (node.Name == "SoundEffects")
                    {
                        XmlNodeList soundEffectsNode = node.ChildNodes;

                        for (int i = 0; i < soundEffectsNode.Count; i++)
                            soundEffects.Add(soundEffectsNode[i].InnerText);
                    }

                    if (node.Name == "Fonts")
                    {
                        XmlNodeList fontsNode = node.ChildNodes;

                        for (int i = 0; i < fontsNode.Count; i++)
                            fonts.Add(fontsNode[i].InnerText);
                    }

                    if (node.Name == "Textures")
                    {
                        XmlNodeList texturesNode = node.ChildNodes;

                        for (int i = 0; i < texturesNode.Count; i++)
                            textures.Add(texturesNode[i].InnerText);
                    }
                }           


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
}