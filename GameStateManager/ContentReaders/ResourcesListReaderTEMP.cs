using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace GameStateManager
{
    // Read a ResourceList object from the content pipeline.
    public class ResourceChestReaderTEMP : ContentTypeReader<ResourcesList>
    {
        protected override ResourcesList Read(ContentReader input, ResourcesList existingInstance)
        {
            ResourcesList resourceList = existingInstance;

            if (resourceList == null)
                resourceList = new ResourcesList();

            return resourceList;
        }
    }
}