using System;
using System.IO;
using RecompOne.Runtime.Assets;
using RecompOne.Runtime.Assets.Textures;
using RecompOne.Runtime.Modding;

namespace ControllerGlyphs;

public static class GlyphTextures
{
    public const string Owner = "controller-glyphs";
    const int Priority = 20000;
    
    static readonly string[] SmallTiles =
    {
        "6a9f4788cedbcf05", "8ab16f4effc1f9f5", "8f418c3a60416116", "b6cbee7a16e4b299",
    };
    
    static readonly string?[] ButtonTiles =
    {
        null, null, "ecb0a1c1f613c04e", "5890b2be853cc87c",
    };
    
    const string DpadTile = "daae5534d0f23957";
    const string StartTile = "3b996bf962a51b4a";
    
    public static void Apply(string? assets, string folder, string?[] faces, bool start)
    {
        if (assets == null)
        {
            Clear();
            return;
        }
        
        string source = Path.Combine(assets, folder);
        string name = folder;
        foreach (var face in faces) name += "-" + (face ?? "none");
        if (start) name += "-start";
        string target = Path.Combine(ModLoader.Root, ".cache", Owner, name);
        
        try
        {
            Directory.CreateDirectory(target);
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i] == null) continue;
                Copy(source, faces[i] + ".png", target, SmallTiles[i]);
                if (ButtonTiles[i] != null) Copy(source, faces[i] + "_button.png", target, ButtonTiles[i]!);
            }
            Copy(source, "dpad.png", target, DpadTile);
            if (start) Copy(source, "start.png", target, StartTile);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{Owner}] cannot prepare '{folder}': {ex.Message}");
            Clear();
            return;
        }
        
        AssetReplacerManager.Instance.LoadTextureFolder(Owner, target, Priority);
        if (start) ExpandSides(StartTile);
    }
    
    public static void Clear()
    {
        AssetReplacerManager.Instance.UnloadTextureFolder(Owner);
    }
    
    static void ExpandSides(string hash)
    {
        var asset = AssetReplacerManager.Instance.ResolveTexture(Convert.ToUInt64(hash, 16), 0);
        if (asset == null || asset.PackId != Owner) return;
        asset.Expand = TextureExpand.Left | TextureExpand.Right;
        asset.AspectChecked = false;
        TextureResolver.Invalidate();
    }
    
    static void Copy(string source, string file, string target, string hash)
    {
        string from = Path.Combine(source, file);
        if (File.Exists(from)) File.Copy(from, Path.Combine(target, hash + ".png"), true);
    }
}
