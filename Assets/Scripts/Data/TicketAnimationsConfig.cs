using System;
using UI.ParamsTween;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "TicketAnimationsConfig", menuName = "Configs/Animations/TicketAnimationsConfig", order = 0)]
    public sealed class TicketAnimationsConfig : ScriptableObject
    {
        public TicketSettings Normal;
        public TicketSettings Selected;
    }
}

[Serializable]
public sealed class TicketSettings
{
    public Color FrameColor;
    public float ScaleFactor;
    public TweenParamsOut Params;
}