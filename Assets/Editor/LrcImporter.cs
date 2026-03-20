using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.IO;

[ScriptedImporter(1, "lrc")]
public class LrcImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var text = File.ReadAllText(ctx.assetPath);
        var ta = new TextAsset(text);
        ta.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
        ctx.AddObjectToAsset("text", ta);
        ctx.SetMainObject(ta);
    }
}
