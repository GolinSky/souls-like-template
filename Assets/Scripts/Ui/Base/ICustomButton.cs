using UnityEngine;

namespace System.Ui.Base
{
    public interface ICustomButton
    {
        InputTypes InputType { get; }
        
        bool HasText { get; }
        bool HasIcon { get; }
        bool HasAdditionalText { get; }
        
        void SetText(string text);
        string GetText();
        
        void SetIcon(Sprite icon);
        Sprite GetIcon();
        
        void SetAdditionalText(string text);
        string GetAdditionalText();
    }
}