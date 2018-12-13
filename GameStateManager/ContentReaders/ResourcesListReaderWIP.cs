using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;


namespace GameStateManager
{
    //// Read a ResourceList object from the content pipeline.
    //public class ResourceChestReader : ContentTypeReader<ResourcesList>
    //{
    //    protected override ResourcesList Read(ContentReader input, ResourcesList existingInstance)
    //    {
    //        ResourcesList resourceList = existingInstance;

    //        if (resourceList == null)
    //            resourceList = new ResourcesList();

    //        //input.ReadRawObject(resourceList as ResourcesList);
    //        resourceList.Songs.AddRange(input.ReadObject<List<Song>>());

    //        for (int i = 0; i < resourceList.Songs.Count; i++)
    //        {
    //            resourceList.Songs[i] = input.ContentManager.Load<Song>(
    //                "Songs/" + resourceList.Songs[i].ToString());
    //        }

    //        resourceList.SoundEffects.AddRange(input.ReadObject<List<SoundEffect>>());

    //        for (int i = 0; i < resourceList.SoundEffects.Count; i++)
    //        {
    //            resourceList.SoundEffects[i] = input.ContentManager.Load<SoundEffect>(
    //                "SoundEffects/" + resourceList.SoundEffects[i].ToString());
    //        }

    //        resourceList.Fonts.AddRange(input.ReadObject<List<SpriteFont>>());

    //        for (int i = 0; i < resourceList.Fonts.Count; i++)
    //        {
    //            resourceList.Fonts[i] = input.ContentManager.Load<SpriteFont>(
    //                "SpriteFont/" + resourceList.Fonts[i].ToString());
    //        }

    //        resourceList.Textures.AddRange(input.ReadObject<List<Texture2D>>());

    //        for (int i = 0; i < resourceList.Textures.Count; i++)
    //        {
    //            resourceList.Textures[i] = input.ContentManager.Load<Texture2D>(
    //                "Textures/" + resourceList.Textures[i].ToString());
    //        }

    //        return resourceList;
    //    }
    //}
}