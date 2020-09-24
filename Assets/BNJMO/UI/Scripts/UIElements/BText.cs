using UnityEngine;
using UnityEngine.UI;
using BNJMO;
using TMPro;
using Sirenix.OdinInspector;

public class BText : BUIElement
{
    public bool WriteTextUppercase  { get { return writeTextUppercase; } set { writeTextUppercase = value; } }
    public string Text              { get { return text; } }

    [BoxGroup("BText", centerLabel:true)]
    [BoxGroup("BText")] [SerializeField] [TextArea] private string text = "BText";
    [BoxGroup("BText")] [SerializeField] private bool writeTextUppercase = false;
    [BoxGroup("BText")] [SerializeField] private bool overrideUINameFromText = false;
    [BoxGroup("BText")] [SerializeField] private Color color = Color.white;

    private Text textUI;
    private TextMesh textMesh;
    private TMP_Text textMeshPro;


    protected override void OnValidate()
    {
        objectNamePrefix = "T_";

        if (overrideUINameFromText)
        {
            UIElementName = text;
        }

        base.OnValidate();

        if (CanValidate() == false)
        {
            return;
        }

        // Revalidate Text
        textUI = GetComponent<Text>();
        textMesh = GetComponent<TextMesh>();
        textMeshPro = GetComponent<TMP_Text>();
      
        SetText(text);
        SetColor(color);
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        textUI = GetComponent<Text>();
        textMesh = GetComponent<TextMesh>();
        textMeshPro = GetComponent<TMP_Text>();
        if ((textUI == null) && (textMesh == null) && (textMeshPro == null))
        {
            LogConsoleError("No Text, TextMesh or TextMeshPro component found on this gameobject!");
        }
    }


    protected override void OnUIElementShown()
    {
        base.OnUIElementShown();

        if (textUI)
        {
            textUI.enabled = true;
        }
        if (textMesh)
        {
            textMesh.GetComponent<MeshRenderer>().enabled = true;
        }
        if (textMeshPro)
        {
            textMeshPro.enabled = true;
        }
    }

    protected override void OnUIElementHidden()
    {
        base.OnUIElementHidden();

        if (textUI)
        {
            textUI.enabled = false;
        }
        if (textMesh)
        {
            textMesh.GetComponent<MeshRenderer>().enabled = false;
        }
        if (textMeshPro)
        {
            textMeshPro.enabled = false;
        }
    }

    public void SetText(string newText)
    {
        if (writeTextUppercase == true)
        {
            newText = newText.ToUpper();
        }
        text = newText;

        if (textUI)
        {
            textUI.text = newText;
        }
        if (textMesh)
        {
            textMesh.text = newText;
        }
        if (textMeshPro)
        {
            textMeshPro.text = newText;
        }
    }

    public void SetColor(Color newColor)
    {
        color = newColor;

        if (textUI)
        {
            textUI.color = newColor;
        }
        if (textMesh)
        {
            textMesh.color = newColor;
        }
        if (textMeshPro)
        {
            textMeshPro.color = newColor;
        }
    }


}
