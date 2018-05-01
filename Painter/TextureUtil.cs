using UnityEngine;


namespace Painter
{
    public static class Extensions
    {       
        public static Texture2D MakeReadable(this Texture texture)
        {
            RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0);
            Graphics.Blit(texture, temporary);
            Texture2D result = temporary.ToTexture2D();
            RenderTexture.ReleaseTemporary(temporary);
            return result;
        }
        public static Texture2D ToTexture2D(this RenderTexture rt)
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D texture2D = new Texture2D(rt.width, rt.height);
            texture2D.ReadPixels(new Rect(0f, 0f, (float)rt.width, (float)rt.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = active;
            return texture2D;
        }

public static void UpdateACI(this Material material, bool invert)
{
    try
    {
        var aci = material.GetTexture("_ACIMap").MakeReadable();
        var xys = material.GetTexture("_XYSMap").MakeReadable();
        var aciPixels = aci.GetPixels();
        var xysPixels = xys.GetPixels();
        if(invert)
        {
            for (int i = 0; i < aciPixels.Length; i++)
            {
                var invertedColorG = Mathf.GammaToLinearSpace(aciPixels[i].g);
                var invertedSpecularB = Mathf.GammaToLinearSpace(xysPixels[i].b);
                var finalColorG = 1 - Mathf.LinearToGammaSpace(invertedColorG * invertedSpecularB);
                aciPixels[i] = new Color(aciPixels[i].r, finalColorG, aciPixels[i].b);
            }
        }
        else
        {
            for (int k = 0; k < aciPixels.Length; k++)
            {
                var g = Mathf.LinearToGammaSpace(1 - Mathf.GammaToLinearSpace(xysPixels[k].b));
                aciPixels[k] = new Color(aciPixels[k].r, g, aciPixels[k].b);
            }
        }
        Texture2D newTexture = new Texture2D(aci.width, aci.height, aci.format, true);  
        newTexture.SetPixels(aciPixels);
        newTexture.Apply();
        newTexture.Compress(true);
        material.SetTexture("_ACIMap", newTexture);
        Object.Destroy(aci);
    }
    catch (System.Exception e)
    {
        Debug.LogWarning(e);
    }
}
    }
}