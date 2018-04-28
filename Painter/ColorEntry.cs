using System;
using System.Collections.Generic;

namespace Painter
{
    [Serializable]
    public class ColorEntry
    {
        public ushort Key;
        public SerializableColor Value;

        public ColorEntry()
        {
        }

        public ColorEntry(ushort key, SerializableColor value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator ColorEntry(KeyValuePair<ushort, SerializableColor> keyValuePair)
        {
            return new ColorEntry(keyValuePair.Key, keyValuePair.Value);
        }

        public static implicit operator KeyValuePair<ushort, SerializableColor>(ColorEntry entry)
        {
            return new KeyValuePair<ushort, SerializableColor>(entry.Key, entry.Value);
        }
    }
}
