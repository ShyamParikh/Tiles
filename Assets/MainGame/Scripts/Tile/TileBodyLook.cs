using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum TileColorMode
{
    MaterialInstance,   // each tinted tile gets its own material copy: colors can never be shared
    PropertyBlock       // lighter, but some shaders / pipelines ignore it
}

/// <summary>Body colors, materials and blocked dimming of one tile. Plain class owned by Tile.</summary>
public class TileBodyLook
{
    private static readonly string[] FallbackProperties = { "_BaseColor", "_Color", "_TintColor", "_MainColor" };

    private class Part
    {
        public MeshRenderer renderer;
        public Material[] baseMaterials;
        public Material[] instances;
        public string[] properties;   // null entry = that slot has no color property
        public Color[] originals;
        public Color[] actives;
    }

    private readonly MeshRenderer body;
    private readonly List<MeshRenderer> extras;
    private readonly string preferredProperty;
    private readonly TileColorMode mode;
    private readonly Color dimColor;
    private readonly Material[] originalMaterials;
    private readonly ShadowCastingMode originalShadows;
    private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    private readonly List<Part> parts = new List<Part>();

    private bool hasColor, blocked, warned, materialReplaced;
    private Color color;
    private int colorSlot = -1;

    public TileBodyLook(MeshRenderer body, List<MeshRenderer> extras, string preferredProperty, TileColorMode mode, Color dimColor)
    {
        this.body = body;
        this.extras = extras;
        this.preferredProperty = preferredProperty;
        this.mode = mode;
        this.dimColor = dimColor;

        if (body != null)
        {
            originalMaterials = body.sharedMaterials;
            originalShadows = body.shadowCastingMode;
        }
        Build();
    }

    public void SetColor(Color value, int slot = -1)
    {
        hasColor = true;
        color = value;
        colorSlot = slot;

        bool targeted = false;
        foreach (Part part in parts)
        {
            for (int i = 0; i < part.properties.Length; i++)
            {
                if (part.properties[i] == null) continue;

                bool hit = slot < 0 || i == slot;
                part.actives[i] = hit ? value : part.originals[i];
                targeted |= hit;
            }
        }

        if (!targeted && !warned)
        {
            warned = true;
            Debug.LogWarning($"[TILE] Color requested for material slot {slot}, but it has no color property. Set 'Body Color Property' to your shader's exact property name (Shader Graph: the Reference name, Exposed ticked).", body);
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

    /// <summary>Replaces the body material in one slot (or all slots when slot is -1).</summary>
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
        else
        {
            Debug.LogWarning($"[TILE] Material slot {slot} does not exist (body has {mats.Length}).", body);
        }
        Rebuild();
    }

    /// <summary>Puts back the prefab's own materials. Does nothing if no material was replaced.</summary>
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

    public void SetShadows(bool on)
    {
        if (body != null) body.shadowCastingMode = on ? originalShadows : ShadowCastingMode.Off;
    }

    /// <summary>Frees the per-tile material copies. Call when the tile is destroyed.</summary>
    public void Release()
    {
        Release(false);
    }

    private void Release(bool restoreBase)
    {
        foreach (Part part in parts)
        {
            if (part.instances == null) continue;

            if (restoreBase && part.renderer != null) part.renderer.sharedMaterials = part.baseMaterials;
            foreach (Material instance in part.instances)
            {
                if (instance != null) Object.Destroy(instance);
            }
            part.instances = null;
        }
    }

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
            if (extra != null && extra != body) AddPart(extra);
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

    private void Apply()
    {
        bool dim = blocked && dimColor != Color.white;

        foreach (Part part in parts)
        {
            for (int i = 0; i < part.properties.Length; i++)
            {
                if (part.properties[i] == null) continue;

                bool tinted = hasColor && (colorSlot < 0 || i == colorSlot);
                Color shown = dim ? part.actives[i] * dimColor : part.actives[i];

                if (mode == TileColorMode.PropertyBlock) ApplyBlock(part, i, shown, tinted || dim);
                else ApplyInstance(part, i, shown, tinted || dim);
            }
        }
    }

    private void ApplyBlock(Part part, int slot, Color shown, bool overridden)
    {
        if (!overridden)
        {
            part.renderer.SetPropertyBlock(null, slot);
            return;
        }

        part.renderer.GetPropertyBlock(block, slot);
        block.SetColor(part.properties[slot], shown);
        part.renderer.SetPropertyBlock(block, slot);
    }

    private void ApplyInstance(Part part, int slot, Color shown, bool overridden)
    {
        if (!overridden && part.instances == null) return;

        // Renderer.materials clones every material and assigns the clones to this renderer.
        if (part.instances == null) part.instances = part.renderer.materials;

        Material instance = part.instances[slot];
        if (instance != null) instance.SetColor(part.properties[slot], overridden ? shown : part.originals[slot]);
    }
}