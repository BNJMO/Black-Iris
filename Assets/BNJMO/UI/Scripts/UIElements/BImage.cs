using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace BNJMO
{
    public class BImage : BUIElement
    {
        [BoxGroup("BImage", centerLabel: true)]
        [BoxGroup("BImage")] [SerializeField] private Sprite sprite = null;
        [BoxGroup("BImage")] [SerializeField] private Color color = Color.white;

        public Image UnityImage { get; private set; }
        public RawImage UnityRawImage { get; private set; }
        public SpriteRenderer UnitySpriteRenderer { get; private set; }

        protected override void OnValidate()
        {
            objectNamePrefix = "I_";

            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            // Revalidate Image
            UnityImage = GetComponent<Image>();
            UnityRawImage = GetComponent<RawImage>();
            UnitySpriteRenderer = GetComponent<SpriteRenderer>();
            SetSprite(sprite);
            SetColor(color);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            // Check Image is note null
            UnityImage = GetComponent<Image>();
            UnityRawImage = GetComponent<RawImage>();
            UnitySpriteRenderer = GetComponent<SpriteRenderer>();
            if ((UnityImage == null) && (UnityRawImage == null) && (UnitySpriteRenderer == null))
            {
                LogConsoleError("No Image, RawImage or SpriteRenderer component found on this gameobject!");
            }
        }

        protected override void OnUIElementShown()
        {
            base.OnUIElementShown();

            if (UnityImage)
            {
                UnityImage.enabled = true;
            }
            if (UnityRawImage)
            {
                UnityRawImage.enabled = true;
            }
            if (UnitySpriteRenderer)
            {
                UnitySpriteRenderer.enabled = true;
            }
        }

        protected override void OnUIElementHidden()
        {
            base.OnUIElementHidden();

            if (UnityImage)
            {
                UnityImage.enabled = false;
            }
            if (UnityRawImage)
            {
                UnityRawImage.enabled = false;
            }
            if (UnitySpriteRenderer)
            {
                UnitySpriteRenderer.enabled = false;
            }
        }

        public void SetSprite(Sprite newSprite)
        {
            if (newSprite == null)
            {
                return;
            }
            sprite = newSprite;

            if (UnityImage)
            {
                UnityImage.sprite = newSprite;
            }
            if (UnityRawImage)
            {
                UnityRawImage.texture = newSprite.texture;
            }
            if (UnitySpriteRenderer)
            {
                UnitySpriteRenderer.sprite = newSprite;
            }
        }
                
        public void SetColor(Color newColor)
        {
            color = newColor;

            if (UnityImage)
            {
                UnityImage.color = newColor;
            }
            if (UnityRawImage)
            {
                UnityRawImage.color = newColor;
            }
            if (UnitySpriteRenderer)
            {
                UnitySpriteRenderer.color = newColor;
            }
        }

    }
}
