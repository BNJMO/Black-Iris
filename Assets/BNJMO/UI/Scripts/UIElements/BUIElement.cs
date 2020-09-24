using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEditor;

namespace BNJMO
{
    public abstract class BUIElement : BBehaviour
    {
        public event Action<BUIElement> BUIElementEnabled;
        public event Action<BUIElement> BUIElementDisabled;

        public string UIElementName { get { return uIElementName; } set { uIElementName = value; } }
        public BFrame ParentBFrame { get { return parentBFrame; } }
        public BMenu ParentBMenu { get { return parentBMenu; } }
        public string UINameExtension { get; set; }
        public bool CanBeShown { get; set; } = true;

        [BoxGroup("BUIElement", centerLabel: true)] [SerializeField] protected string uIElementName = "";

        [FoldoutGroup("BUIElement/More")]
        [Title("Parent BIUElements (Automatically populated)")]
        [FoldoutGroup("BUIElement/More")] [SerializeField] protected BFrame parentBFrame;
        [FoldoutGroup("BUIElement/More")] [SerializeField] protected BMenu parentBMenu;

        [FoldoutGroup("BUIElement/More")]
        [Title("Configuration")]
        [FoldoutGroup("BUIElement/More")] [SerializeField] protected bool overrideGameOjbectName = true;
        [FoldoutGroup("BUIElement/More")] [SerializeField] protected bool propagateUINameToChildren = false;
        [FoldoutGroup("BUIElement/More")] [SerializeField] protected bool revalidateAllDirectChildren = true;
        [FoldoutGroup("BUIElement/More")] [Button("Revalidate")] private void Button_Revalidate() { Revalidate(); }
        [FoldoutGroup("BUIElement/More")] [Button("Enable UIElement")] private void Button_EnableUIElement() { ShowUIElement(); }
        [FoldoutGroup("BUIElement/More")] [Button("Disable UIElement")] private void Button_DisableUIElement() { HideUIElement(); }
        [FoldoutGroup("BUIElement/More")] [SerializeField] [ReadOnly] protected string objectNamePrefix = "C_";

        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            // Update GameObject name
            if (overrideGameOjbectName == true)
            {
                gameObject.name = objectNamePrefix + uIElementName + UINameExtension;
            }

            // Find BFrame and BMenu in parents
            //parentBFrame = null;
            parentBFrame = GetComponentInParent<BFrame>();

            //BFrame newParentFrame = GetComponentInParent<BFrame>();
            //if (newParentFrame != this)
            //{
            //    parentBFrame = GetComponentInParent<BFrame>();
            //}


            parentBMenu = GetComponentInParent<BMenu>();
            //parentBMenu = null;
            //BMenu newparentMenu = GetComponentInParent<BMenu>();
            //if (newparentMenu != this)
            //{
            //    parentBMenu = GetComponentInParent<BMenu>();
            //}

            // Revalidate children UI Elements (only direct children. Revalidation will be propagated if they have revalidateAllDirectChildren set to true)
            if (revalidateAllDirectChildren)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if ((transform)
                        && (transform.GetChild(i))
                        && (transform.GetChild(i).GetComponent<BUIElement>()))
                    {
                        if (propagateUINameToChildren)
                        {
                            transform.GetChild(i).GetComponent<BUIElement>().UIElementName = UIElementName;
                        }

                        transform.GetChild(i).GetComponent<BUIElement>().Revalidate();
                    }
                }
            }
        }

        public bool ShowUIElement(bool overrideActiveBMenuCheck = false)
        {
            // Check parent BMenu conditions if anyone set
            bool parentBMenuCheck = true;
            if (ParentBMenu)
            {
                parentBMenuCheck = (ParentBMenu.IsActive == true) || (overrideActiveBMenuCheck == true);
            }

            if (CanBeShown == true
                && parentBMenuCheck == true)
            {
                OnUIElementShown();
                InvokeEventIfBound(BUIElementEnabled, this);

                // Propagate hierarchically
                for (int i = 0; i < transform.childCount; i++)
                {
                    if ((transform)
                        && (transform.GetChild(i))
                        && (transform.GetChild(i).GetComponent<BUIElement>()))
                    {
                        transform.GetChild(i).GetComponent<BUIElement>().ShowUIElement();
                    }
                }
                    return true;
            }
            return false;
        }

        public void HideUIElement()
        {
            OnUIElementHidden();
            InvokeEventIfBound(BUIElementDisabled, this);

            // Propagate hierarchically
            for (int i = 0; i < transform.childCount; i++)
            {
                if ((transform)
                    && (transform.GetChild(i))
                    && (transform.GetChild(i).GetComponent<BUIElement>()))
                {
                    transform.GetChild(i).GetComponent<BUIElement>().HideUIElement();
                }
            }
        }

        protected virtual void OnUIElementShown()
        {
            // Override if necessary
        }

        protected virtual void OnUIElementHidden()
        {
            // Override if necessary
        }
    }
}