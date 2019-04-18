using UnityEditor;

namespace Tests
{
    public class AssetModificationProcessorBase : UnityEditor.AssetModificationProcessor
    {

        protected virtual void OnStatusUpdated()
        {

        }

        protected virtual bool IsOpenForEdit(string assetPath, string message)
        {
            return false;
        }

        protected virtual AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            return AssetDeleteResult.DidNotDelete;
        }

        protected virtual AssetMoveResult OnWillMoveAsset(string fromPath,
                                                          string toPath)
        {
            return AssetMoveResult.DidNotMove;
        }

        protected virtual string[] OnWillSaveAssets(string[] assets)
        {
            return assets;
        }
    }
}