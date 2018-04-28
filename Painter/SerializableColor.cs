using System;
using UnityEngine;

namespace Painter
{
    [Serializable]
    public class SerializableColor
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public SerializableColor()
        {

        }

        public SerializableColor(Color32 color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public static implicit operator Color32(SerializableColor color)
        {
            return new Color32(color.r, color.g, color.b, color.a);
        }

        public static implicit operator SerializableColor(Color32 color)
        {
            return new SerializableColor(color);
        }

        public static implicit operator Color(SerializableColor color)
        {
            return new Color32(color.r, color.g, color.b, color.a);
        }

        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color);
        }
    }
}
