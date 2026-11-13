using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.utility
{
    public enum SpriteAspectMode
    {
        None,
        WidthControlsHeight,
        HeightControlsWidth,
        FitInParent,
        EnvelopeParent
    }

    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRenderAspectRatio : MonoBehaviour
    {
        #region private variables
        [SerializeField] private SpriteAspectMode aspectMode = SpriteAspectMode.None;
        [SerializeField] private Vector2 referenceSize = new Vector2(1f, 1f);

        private SpriteRenderer spriteRenderer;
        private float aspectRatio;
        #endregion

        #region unity callback
        private void Awake()
        {
            Debug.Log("width " + Screen.width);
            Debug.Log("height " + Screen.height);

            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            UpdateAspectRatio();
        }

        private void OnEnable()
        {
            UpdateAspectRatio();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateAspectRatio();
        }
#endif

        private void OnDrawGizmosSelected()
        {
            Vector3 center = GetSpriteCenterPosition();

            if (aspectMode == SpriteAspectMode.FitInParent || aspectMode == SpriteAspectMode.EnvelopeParent)
            {
                Gizmos.color = Color.green;
                DrawBoundaryGizmo(center, referenceSize);
            }
        }

        private Vector3 GetSpriteCenterPosition()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return transform.position;

            // Get sprite bounds center which accounts for pivot
            Bounds bounds = spriteRenderer.bounds;
            return bounds.center;
        }

        private void DrawBoundaryGizmo(Vector3 center, Vector2 size)
        {
            Vector3 halfSize = new Vector3(size.x * 0.5f, size.y * 0.5f, 0);

            Vector3 topLeft = center + new Vector3(-halfSize.x, halfSize.y, 0);
            Vector3 topRight = center + new Vector3(halfSize.x, halfSize.y, 0);
            Vector3 bottomLeft = center + new Vector3(-halfSize.x, -halfSize.y, 0);
            Vector3 bottomRight = center + new Vector3(halfSize.x, -halfSize.y, 0);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
        #endregion

        #region public methods
        [Button]
        public void UpdateAspectRatio()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            Sprite sprite = spriteRenderer.sprite;
            float spriteWidth = sprite.bounds.size.x;
            float spriteHeight = sprite.bounds.size.y;
            aspectRatio = spriteWidth / spriteHeight;

            Vector3 scale = transform.localScale;

            switch (aspectMode)
            {
                case SpriteAspectMode.None:
                    break;

                case SpriteAspectMode.WidthControlsHeight:
                    scale.y = scale.x / aspectRatio;
                    break;

                case SpriteAspectMode.HeightControlsWidth:
                    scale.x = scale.y * aspectRatio;
                    break;

                case SpriteAspectMode.FitInParent:
                    FitInReference(ref scale, spriteWidth, spriteHeight);
                    break;

                case SpriteAspectMode.EnvelopeParent:
                    EnvelopeReference(ref scale, spriteWidth, spriteHeight);
                    break;
            }

            transform.localScale = scale;
        }
        #endregion

        #region private methods
        private void FitInReference(ref Vector3 scale, float spriteWidth, float spriteHeight)
        {
            float refRatio = referenceSize.x / referenceSize.y;

            if (aspectRatio > refRatio)
            {
                // Sprite is wider - fit by width
                scale.x = referenceSize.x / spriteWidth;
                scale.y = scale.x;
            }
            else
            {
                // Sprite is taller - fit by height
                scale.y = referenceSize.y / spriteHeight;
                scale.x = scale.y;
            }
        }

        private void EnvelopeReference(ref Vector3 scale, float spriteWidth, float spriteHeight)
        {
            float refRatio = referenceSize.x / referenceSize.y;

            if (aspectRatio > refRatio)
            {
                // Sprite is wider - envelope by height
                scale.y = referenceSize.y / spriteHeight;
                scale.x = scale.y;
            }
            else
            {
                // Sprite is taller - envelope by width
                scale.x = referenceSize.x / spriteWidth;
                scale.y = scale.x;
            }
        }
        #endregion
    }
}
