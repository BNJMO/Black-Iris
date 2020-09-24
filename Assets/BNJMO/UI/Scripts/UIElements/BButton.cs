using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BNJMO
{
    [SelectionBase]
    public class BButton : BUIElement, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<BButton> ButtonHighlighted;
        public event Action<BButton> ButtonPressed;
        public event Action<BButton> ButtonUnhighlighted;
        public event Action<BButton, bool> ButtonReleased;
        public event Action<BButton> ButtonHoveredEnter;
        public event Action<BButton> ButtonHoveredExit;

        public bool IsHighlighted { get; private set; }
        public bool IsDisabled { get; private set; } = false;
        public bool UseImageSpecialColors { get { return useImageSpecialColors; } set { useImageSpecialColors = value; } }
        public string ButtonName { get { return buttonText; } }
        public BButton LeftButton { get { return leftButton; } set { leftButton = value; } }
        public BButton RightButton { get { return rightButton; } set { rightButton = value; } }
        public BButton UpperButton { get { return upperButton; } set { upperButton = value; } }
        public BButton BottomButton { get { return buttomButton; } set { buttomButton = value; } }

        [BoxGroup("BButton", centerLabel: true)]
        [SerializeField] private bool automaticallyFindChildBUIElements = true;

        [FoldoutGroup("BButton/Text")] [SerializeField] [TextArea] private string buttonText = "Button";
        [FoldoutGroup("BButton/Text")] [SerializeField] private bool writeButtonTextUppercase = false;
        [FoldoutGroup("BButton/Text")] [SerializeField] private bool overrideUIName = true;
        [FoldoutGroup("BButton/Text")] [SerializeField] private Color textNormalColor = Color.black;
        [FoldoutGroup("BButton/Text")] [SerializeField] private Color textDisabledColor = Color.black;
        [FoldoutGroup("BButton/Text")] [SerializeField] private bool useTextSpecialColors = true;
        [FoldoutGroup("BButton/Text")] [SerializeField] [DisableIf("@this.useTextSpecialColors == false")] private Color textHoveredColor = Color.black;
        [FoldoutGroup("BButton/Text")] [SerializeField] [DisableIf("@this.useTextSpecialColors == false")] private Color textHighlightedColor = Color.black;
        [FoldoutGroup("BButton/Text")] [SerializeField] [DisableIf("@this.useTextSpecialColors == false")] private Color textPressedColor = Color.black;
        [FoldoutGroup("BButton/Text")] [SerializeField] private BText bTextReference;
        [FoldoutGroup("BButton/Text")]
        [Button("Derive Button Name From UIElement Name")]
        private void Button_DeriveName()
        {
            buttonText = UIElementName;
            Revalidate();
        }

        [FoldoutGroup("BButton/Image")] [SerializeField] private Sprite buttonImage;
        [FoldoutGroup("BButton/Image")] [SerializeField] private Color imageNormalColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [FoldoutGroup("BButton/Image")] [SerializeField] private Color imageDisabledColor = new Color(1.0f, 1.0f, 1.0f, 0.125f);
        [FoldoutGroup("BButton/Image")] [SerializeField] private bool useImageSpecialColors = true;
        [FoldoutGroup("BButton/Image")] [SerializeField] [DisableIf("@this.useImageSpecialColors == false")] private Color imageHoveredColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
        [FoldoutGroup("BButton/Image")] [SerializeField] [DisableIf("@this.useImageSpecialColors == false")] private Color imageHighlightedColor = new Color(0.61f, 0.96f, 1.0f, 1.0f);
        [FoldoutGroup("BButton/Image")] [SerializeField] [DisableIf("@this.useImageSpecialColors == false")] private Color ImagePressedColor = new Color(0.46f, 0.67f, 0.69f, 1.0f);
        [FoldoutGroup("BButton/Image")] [SerializeField] private BImage bImageReference;

        [FoldoutGroup("BButton/Sounds")] [SerializeField] private AudioClip onPressedSound;
        [FoldoutGroup("BButton/Sounds")] [SerializeField] private AudioClip onSuccessfullyReleasedSound;

        [FoldoutGroup("BButton/Navigation")] [SerializeField] private BButton leftButton;
        [FoldoutGroup("BButton/Navigation")] [SerializeField] private BButton buttomButton;
        [FoldoutGroup("BButton/Navigation")] [SerializeField] private BButton rightButton;
        [FoldoutGroup("BButton/Navigation")] [SerializeField] private BButton upperButton;

        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonHighlightedUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonPressedUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonUnhighlightedUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonReleasedUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonCancelReleasedUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonHoveredEnterUEvent;
        [FoldoutGroup("BButton/Events")] [SerializeField] public UnityEvent ButtonHoveredExitUEvent;

        protected override void OnValidate()
        {
            objectNamePrefix = "B_";

            if (overrideUIName == true)
            {
                UIElementName = buttonText;
            }

            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            // Update Children BUIElements references
            if (automaticallyFindChildBUIElements == true)
            {
                bTextReference = GetComponentInChildren<BText>();
                bImageReference = GetComponentInChildren<BImage>();
            }

            // Update BText
            if (bTextReference)
            {
                bTextReference.WriteTextUppercase = writeButtonTextUppercase;
                bTextReference.SetText(buttonText);
                bTextReference.SetColor(textNormalColor);
                bTextReference.UIElementName = UIElementName;
            }

            // Update BImage
            if (bImageReference)
            {
                bImageReference.SetSprite(buttonImage);
                bImageReference.SetColor(imageNormalColor);
                bImageReference.UIElementName = UIElementName;
            }
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            if (bTextReference == null)
            {
                LogConsoleError("No BText found attached to this BButton!");
            }

            if (bImageReference == null)
            {
                LogConsoleError("No BImage found attached to this BButton!");
            }
        }

        protected override void OnUIElementHidden()
        {
            base.OnUIElementHidden();

            IsHighlighted = false;

            if (IsDisabled == true)
            {
                return;
            }

            if (bImageReference)
            {
                bImageReference.SetColor(imageNormalColor);
            }
        }

        public void DisableButton()
        {
            IsDisabled = true;

            if (bImageReference)
            {
                bImageReference.SetColor(imageDisabledColor);
            }

            if (bTextReference)
            {
                bTextReference.SetColor(textDisabledColor);
            }
        }

        public void EnableButton()
        {
            IsDisabled = false;

            if (bImageReference)
            {
                bImageReference.SetColor(imageNormalColor);
            }

            if (bTextReference)
            {
                bTextReference.SetColor(textNormalColor);
            }
        }

        #region Children BUIElements
        public void SetButtonImage(Sprite newSprite)
        {
            if (bImageReference)
            {
                bImageReference.SetSprite(newSprite);
            }
        }

        public void SetButtonText(string newText)
        {
            if (bTextReference)
            {
                bTextReference.SetText(newText);
            }
        }

        public void SetButtonImageColor(Color newColor)
        {
            if (bImageReference)
            {
                bImageReference.SetColor(newColor);
                imageNormalColor = newColor;
            }
        }

        public void SetButtonTextColor(Color newColor)
        {
            if (bTextReference)
            {
                bTextReference.SetColor(newColor);
                textNormalColor = newColor;
            }
        }


        #endregion

        #region Button Press
        public void OnHighlighted()
        {
            if (IsDisabled == true)
            {
                LogConsoleWarning("Trying to highlight a disabled button!");
                return;
            }

            IsHighlighted = true;

            if (bImageReference
                && useImageSpecialColors == true)
            {
                bImageReference.SetColor(imageHighlightedColor);
            }

            if (bTextReference
                && useTextSpecialColors == true)
            {
                bTextReference.SetColor(textHighlightedColor);
            }

            if (parentBMenu)
            {
                parentBMenu.OnBButtonHighlighted(this);
            }

            InvokeEventIfBound(ButtonHighlighted, this);

            if (ButtonHighlightedUEvent != null)
            {
                ButtonHighlightedUEvent.Invoke();
            }

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_ButtonHighlighted.Invoke(new BEHandle<BButton>(this), MotherOfManagers.Instance.DebugUIButtonsEvents);
            }
        }

        public void OnReleased(bool cursorInside)
        {
            if (IsDisabled == true)
            {
                return;
            }

            if (bImageReference
                && useImageSpecialColors == true)
            {
                bImageReference.SetColor(imageHighlightedColor);
            }

            if (bTextReference
                && useTextSpecialColors == true)
            {
                bTextReference.SetColor(textHighlightedColor);
            }

            if ((onPressedSound)
                && (cursorInside == true))
            {
                AudioManager.Instance.SpawnSoundObject(onSuccessfullyReleasedSound);
            }

            InvokeEventIfBound(ButtonReleased, this, cursorInside);

            if (ButtonReleasedUEvent != null)
            {
                ButtonReleasedUEvent.Invoke();
            }

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_ButtonReleased.Invoke(new BEHandle<BButton, bool>(this, cursorInside), MotherOfManagers.Instance.DebugUIButtonsEvents);
            }
        }

        public void OnPressed()
        {
            if (IsDisabled == true)
            {
                return;
            }

            if (bImageReference
                && useImageSpecialColors == true)
            {
                bImageReference.SetColor(ImagePressedColor);
            }

            if (bTextReference
                && useTextSpecialColors == true)
            {
                bTextReference.SetColor(textPressedColor);
            }

            if (onPressedSound)
            {
                AudioManager.Instance.SpawnSoundObject(onPressedSound);
            }

            if (parentBMenu)
            {
                parentBMenu.OnBButtonPressed(this);
            }

            InvokeEventIfBound(ButtonPressed, this);

            if (ButtonPressedUEvent != null)
            {
                ButtonPressedUEvent.Invoke();
            }

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_ButtonPressed.Invoke(new BEHandle<BButton>(this), MotherOfManagers.Instance.DebugUIButtonsEvents);
            }
        }

        public void OnUnhighlighted()
        {
            if (IsDisabled == true)
            {
                return;
            }

            IsHighlighted = false;

            if (bImageReference)
            {
                bImageReference.SetColor(imageNormalColor);
            }

            if (bTextReference)
            {
                bTextReference.SetColor(textNormalColor);
            }

            InvokeEventIfBound(ButtonUnhighlighted, this);

            if (ButtonUnhighlightedUEvent != null)
            {
                ButtonUnhighlightedUEvent.Invoke();
            }

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_ButtonUnhighlighted.Invoke(new BEHandle<BButton>(this), MotherOfManagers.Instance.DebugUIButtonsEvents);
            }
        }

        public void OnHoveredEnter()
        {
            if (IsDisabled == true)
            {
                return;
            }

            OnHighlighted();

            if (bImageReference
                && useImageSpecialColors == true)
            {
                bImageReference.SetColor(imageHoveredColor);
            }

            if (bTextReference
                && useTextSpecialColors == true)
            {
                bTextReference.SetColor(textHoveredColor);
            }

            InvokeEventIfBound(ButtonHoveredEnter, this);

            if (ButtonHoveredEnterUEvent != null)
            {
                ButtonHoveredEnterUEvent.Invoke();
            }
        }

        public void OnHoveredExit()
        {
            if (IsDisabled == true)
            {
                return;
            }

            if (bImageReference
                && useImageSpecialColors == true)
            {
                bImageReference.SetColor(imageHighlightedColor);
            }

            if (bTextReference
                && useTextSpecialColors == true)
            {
                bTextReference.SetColor(textHighlightedColor);
            }

            InvokeEventIfBound(ButtonHoveredExit, this);

            if (ButtonHoveredExitUEvent != null)
            {
                ButtonHoveredExitUEvent.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPressed();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            bool cursorInside = ((eventData.pointerCurrentRaycast.gameObject) && (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<BButton>() == this));
            OnReleased(cursorInside);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoveredEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoveredExit();
        }
        #endregion

        #region Navigation
        public BButton GetNextButton(EButtonDirection buttonDirection)
        {
            BButton buttonToReturn = null;
            switch (buttonDirection)
            {
                case EButtonDirection.LEFT:
                    buttonToReturn = leftButton;
                    break;

                case EButtonDirection.RIGHT:
                    buttonToReturn = rightButton;
                    break;

                case EButtonDirection.UP:
                    buttonToReturn = upperButton;
                    break;

                case EButtonDirection.DOWN:
                    buttonToReturn = buttomButton;
                    break;
            }
            return buttonToReturn;
        }

        public void UnPopulateNavigationButtons()
        {
            LogConsole("Unpopulating buttons");
            LeftButton = null;
            RightButton = null;
            UpperButton = null;
            BottomButton = null;
        }
        #endregion
    }
}
