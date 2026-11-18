/*
 * TileBodyLook
 * ------------
 * Handles everything about a tile's body colors and materials, in a plain class that Tile owns. Every tinted
 * tile gets its own copy of the material (made the first time a color is needed), so one tile's color can
 * never leak onto another. It finds the shader's color property (_BaseColor and friends), tints one material
 * slot or all of them, replaces body materials, dims the body when the tile is covered, and frees its material
 * copies when the tile is destroyed. A one-time warning appears if the material has no color property.
 */
using System.Collections.Generic;
using UnityEngine;

public class TileBodyLook
{
    #region Nested Types

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

    #endregion

    #region Private Variables

    private static readonly string[] FallbackProperties = { "_BaseColor", "_Color", "_TintColor", "_MainColor" };
    private static bool warned;

    private readonly MeshRenderer body;
    private readonly List<MeshRenderer> extras;
    private readonly string preferredProperty;
    private readonly Color dimColor;
    private readonly Material[] originalMaterials;
    private readonly List<Part> parts = new List<Part>();

    private bool hasColor, blocked, materialReplaced;
    private Color color;
    private int colorSlot = -1;

    #endregion

    #region Setup

    // Reads the renderers once. dimColor is what a covered tile is multiplied by (white = no dimming).
    public TileBodyLook(MeshRenderer body, List<MeshRenderer> extras, string preferredProperty, Color dimColor)
    {
        this.body = body;
        this.extras = extras;
        this.preferredProperty = preferredProperty;
        this.dimColor = dimColor;
        originalMaterials = body != null ? body.sharedMaterials : null;
        Build();
    }

    #endregion

    #region Colors

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

    // Goes back to the material's own colors.
    public void ClearColor()
    {
        if (!hasColor) return;

        hasColor = false;
        colorSlot = -1;
        foreach (Part part in parts) System.Array.Copy(part.originals, part.actives, part.originals.Length);
        Apply();
    }

    // Dims the body when the tile is covered, restores it when it is free.
    public void SetBlocked(bool value)
    {
        blocked = value;
        Apply();
    }

    #endregion

    #region Materials

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

    // Frees our material copies. Call when the tile is destroyed.
    public void Release()
    {
        Release(false);
    }

    // Destroys the copies, and optionally puts the shared materials back on the renderers.
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

    #endregion

    #region Reading And Applying

    // After the materials changed: read them again and re-apply the current color.
    private void Rebuild()
    {
        Build();
        if (hasColor) SetColor(color, colorSlot);
        else Apply();
    }

    // Reads every renderer's materials and colors.
    private void Build()
    {
        parts.Clear();
        AddPart(body);
        foreach (MeshRenderer extra in extras)
        {
            if (extra != body) AddPart(extra);
        }
    }

    // Reads one renderer.
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

    // The shader's color property: the preferred one if it exists, otherwise a common fallback.
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

    #endregion
}