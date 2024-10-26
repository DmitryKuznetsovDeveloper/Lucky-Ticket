using UI.ParamsTween;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "BaseButtonConfig", menuName = "Configs/Animations/BaseButtonConfig", order = 0)]
    public class BaseButtonConfig : ScriptableObject
    {
        public Color BgColor;
        public float ScaleFactor;
        public TweenParamsOut Params;
    }
}