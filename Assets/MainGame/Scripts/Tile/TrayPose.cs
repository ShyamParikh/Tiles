using System;
using DG.Tweening;
using UnityEngine;

// How a tile looks once it sits in the tray, and how it gets there.
[Serializable]
public class TrayPose
{
    [Tooltip("Size in the tray as a multiple of the size on the board. (1,1,1) = unchanged, (0.8,0.8,0.8) = 20% smaller. Different X / Y / Z values squash or stretch it.")]
    public Vector3 scale = Vector3.one;

    [Tooltip("On = tiles turn to the rotation below when they reach the tray. Off = they keep the rotation they had on the board.")]
    public bool setRotation;
    [Tooltip("Rotation of a tile inside its slot, in degrees (the slot's own axes).")]
    public Vector3 rotation = Vector3.zero;

    [Tooltip("How long a tile takes to grow and turn into this pose. It happens while the tile flies in.")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutQuad;
}