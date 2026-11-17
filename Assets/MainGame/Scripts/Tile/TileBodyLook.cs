using System.Collections.Generic;
using UnityEngine;

// A tile's body colors and materials. Every tinted tile gets its own copy of the material,
// so one tile's color can never leak onto another.
public class TileBodyLook
{
    private static readonly string[] FallbackProperties = { "_BaseColor", "_Color", "_TintColor", "_MainColor" };
    private static bool warned;

    // One renderer and everything needed to tint its material slots.
    private class Part
    {
        public MeshRenderer renderer;
        public Material[] baseMaterials;   // the shared materials, kept so they can be restored
        public Material[] copies;          // our private copies, made the first time a color is needed
        public string[] properties;        // color property per slot (null = that slot has none)
        public Color[] originals;
        public Color[] actives;
    }

    private readonly MeshRenderer body;
    private readonly List<MeshRenderer> extras;
    private readonly string preferredProperty;
    private readonly Color dimColor;
    private readonly Material[] originalMaterials;
    private readonly List<Part> parts = new List<Part>();

    private bool hasColor, blocked, materialReplaced;
    private Color color;
    private int colorSlot = -1;

    public TileBodyLook(MeshRenderer body, List<MeshRenderer> extras, string preferredProperty, Color dimColor)
    {
        this.body = body;
        this.extras = extras;
        this.preferredProperty = preferredProperty;
        this.dimColor = dimColor;
        originalMaterials = body != null ? body.sharedMaterials : null;
        Build();
    }

    // Tints one material slot, or every slot when slot is -1.
    public void SetColor(Color value, int slot = -1)
    {
        hasColor = true;
        color = value;
        colorSlot = slot;

        bool hit = false;
        foreach (Part part in parts)
        {
            for (int i = 0; i < part.properties.Length; i++)
            {
                if (part.properties[i] == null) continue;

                bool targeted = slot < 0 || i == slot;
                part.actives[i] = targeted ? value : part.originals[i];
                hit |= targeted;
            }
        }

        if (!hit && !warned)
        {
            warned = true;
            Debug.LogWarning("[TILE] A color was set but the material has no color property. Set 'Body Color Property' to your shader's property name (Shader Graph: its Reference name, with Exposed ticked).", body);
        }
        Apply();
    }

    public void ClearColor()
    {
        if (!hasColor) return;

        hasColor = false;
        colorSlot = -1;
        foreach (Part part in parts) System.Array.Copy(part.originals, part.actives, part.originals.Length);
        Apply();
    }

    // Replaces the body material in one slot, or in every slot when slot is -1.
    public void SetMaterial(Material material, int slot = -1)
    {
        if (material == null || body == null) return;

        Release(true);
        Material[] mats = body.sharedMaterials;

        if (slot < mats.Length)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                if (slot < 0 || i == slot) mats[i] = material;
            }
            body.sharedMaterials = mats;
            materialReplaced = true;
        }
        Rebuild();
    }

    // Puts the prefab's own materials back. Does nothing if none were replaced.
    public void ResetMaterial()
    {
        if (!materialReplaced || body == null) return;

        materialReplaced = false;
        Release(true);
        body.sharedMaterials = originalMaterials;
        Rebuild();
    }

    public void SetBlocked(bool value)
    {
        blocked = value;
        Apply();
    }

    // Frees our material copies. Call when the tile is destroyed.
    public void Release()
    {
        Release(false);
    }

    private void Release(bool restoreShared)
    {
        foreach (Part part in parts)
        {
            if (part.copies == null) continue;

            if (restoreShared && part.renderer != null) part.renderer.sharedMaterials = part.baseMaterials;
            foreach (Material copy in part.copies)
            {
                if (copy != null) Object.Destroy(copy);
            }
            part.copies = null;
        }
    }

    // After the materials changed: read them again and re-apply the current color.
    private void Rebuild()
    {
        Build();
        if (hasColor) SetColor(color, colorSlot);
        else Apply();
    }

    private void Build()
    {
        parts.Clear();
        AddPart(body);
        foreach (MeshRenderer extra in extras)
        {
            if (extra != body) AddPart(extra);
        }
    }

    private void AddPart(MeshRenderer renderer)
    {
        if (renderer == null) return;

        Material[] mats = renderer.sharedMaterials;
        Part part = new Part
        {
            renderer = renderer,
            baseMaterials = mats,
            properties = new string[mats.Length],
            originals = new Color[mats.Length],
            actives = new Color[mats.Length]
        };

        for (int i = 0; i < mats.Length; i++)
        {
            string property = FindProperty(mats[i]);
            if (property == null) continue;

            part.properties[i] = property;
            part.originals[i] = part.actives[i] = mats[i].GetColor(property);
        }
        parts.Add(part);
    }

    private string FindProperty(Material material)
    {
        if (material == null) return null;
        if (!string.IsNullOrEmpty(preferredProperty) && material.HasProperty(preferredProperty)) return preferredProperty;

        foreach (string property in FallbackProperties)
        {
            if (material.HasProperty(property)) return property;
        }
        return null;
    }

    // Pushes the current color (and the dimming of covered tiles) onto the materials.
    private void Apply()
    {
        bool dim = blocked && dimColor != Color.white;

        foreach (Part part in parts)
        {
            for (int i = 0; i < part.properties.Length; i++)
            {
                if (part.properties[i] == null) continue;

                bool tinted = hasColor && (colorSlot < 0 || i == colorSlot);
                if (!tinted && !dim && part.copies == null) continue;   // nothing to change, so no copy is needed

                // Renderer.materials hands out private copies of every material on the renderer.
                if (part.copies == null) part.copies = part.renderer.materials;
                part.copies[i].SetColor(part.properties[i], dim ? part.actives[i] * dimColor : part.actives[i]);
            }
        }
    }
}