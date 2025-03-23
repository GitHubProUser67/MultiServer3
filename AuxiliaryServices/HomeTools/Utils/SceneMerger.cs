using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HomeTools.Utils
{
    public static class SceneMerger
    {
        public static void MergeSceneFiles(string sourceXmlPath, string targetXmlPath)
        {
            XDocument sourceDoc = XDocument.Load(sourceXmlPath);
            XDocument targetDoc = XDocument.Load(targetXmlPath);

            MergeGameObjectFolders(targetDoc, sourceDoc);
            MergeAssetFolders(targetDoc, sourceDoc);

            targetDoc.Save(Path.GetDirectoryName(sourceXmlPath) + "/" + Path.GetFileNameWithoutExtension(sourceXmlPath) + "_MERGED.SCENE");
        }

        // Method to merge <gap:gameObjectFolder> elements (including nested game objects).
        private static void MergeGameObjectFolders(XDocument targetDoc, XDocument sourceDoc)
        {
            IEnumerable<XElement> targetGameObjectFolders = targetDoc.Descendants().Where(e => e.Name.LocalName == "gameObjectFolder");
            IEnumerable<XElement> sourceGameObjectFolders = sourceDoc.Descendants().Where(e => e.Name.LocalName == "gameObjectFolder");

            if (targetGameObjectFolders == null || sourceGameObjectFolders == null)
            {
                CustomLogger.LoggerAccessor.LogError("[SceneMerger] - MergeGameObjectFolders - gameObjectFolder folder not found in one of the documents!");
                return;
            }

            // Loop through each <gap:gameObjectFolder> in the source XML.
            foreach (var sourceFolder in sourceGameObjectFolders)
            {
                // Check if the same element exists in the target XML.
                XElement targetFolder = targetGameObjectFolders.FirstOrDefault(x => x.Attribute("name")?.Value == sourceFolder.Attribute("name")?.Value);

                if (targetFolder == null)
                {
                    targetFolder = new XElement(sourceFolder);
                    targetDoc.Root.Add(targetFolder);
                }

                // Recursively copy all child elements from the source folder to the target folder.
                CopyChildrenRecursively(sourceFolder, targetFolder);
            }
        }

        // Method to merge <gap:assetFolder> elements (specifically "Assets" folder).
        private static void MergeAssetFolders(XDocument targetDoc, XDocument sourceDoc)
        {
            XElement targetAssetFolder = targetDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "assetFolder" && e.Attribute("name")?.Value == "Assets");
            XElement sourceAssetFolder = sourceDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "assetFolder" && e.Attribute("name")?.Value == "Assets");

            if (targetAssetFolder == null || sourceAssetFolder == null)
            {
                CustomLogger.LoggerAccessor.LogError("[SceneMerger] - MergeAssetFolders - Asset folder not found in one of the documents!");
                return;
            }

            // Loop through each <gap:asset> and <gap:folder> in the source XML.
            foreach (var assetOrFolder in sourceAssetFolder.Elements().Where(e => e.Name.LocalName == "asset" || e.Name.LocalName == "folder"))
            {
                // Check if the same element exists in the target XML.
                XElement existingAsset = targetAssetFolder.Elements().FirstOrDefault(x => x.Attribute("name")?.Value == assetOrFolder.Attribute("name")?.Value);

                if (existingAsset == null)
                    targetAssetFolder.Add(new XElement(assetOrFolder));
            }
        }

        // Method to recursively copy children of a source element to a target element.
        private static void CopyChildrenRecursively(XElement source, XElement target)
        {
            foreach (var child in source.Elements())
            {
                // Check if at least one element with the same name attribute exists in the target XML within the same child.
                if (!target.Elements().Any(x => x.Name == child.Name && x.Attribute("name")?.Value == child.Attribute("name")?.Value))
                {
                    XElement newChild = new XElement(child);
                    target.Add(newChild);

                    // If the child has nested children, recurse into them
                    if (child.HasElements)
                        CopyChildrenRecursively(child, newChild);
                }
            }
        }
    }
}
