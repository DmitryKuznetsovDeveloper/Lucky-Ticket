using UI.ParamsTween;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "BasePopupConfig", menuName = "Configs/Animations/BasePopupConfig", order = 0)]
    public sealed class BasePopupConfig : ScriptableObject
    {
        public PopupStateParams ShowParams;
        public PopupStateParams HideParams;
    }
}

[System.Serializable]
public sealed class PopupStateParams
{
    public Color BgColorTarget;
    public Color BgColorFrom;
    [Range(0, 1)] public float RootFadeTarget;
    [Range(0, 1)] public float RootFadeFrom;
    public float ScaleFactorFrom;
    public float ScaleFactorTarget;
    public TweenParamsOutIn Params;
}