using ICities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Painter
{

    public class SerializableDataExtension : SerializableDataExtensionBase
    {
        private Painter Instance => Painter.instance;
        private static readonly string m_dataID = "PAINTER_COLOR_DATA";

        private List<ColorEntry> ColorData
        {
            get
            {
                var list = new List<ColorEntry>();
                if (Instance.Colors != null)
                    foreach (var item in Instance.Colors)
                        list.Add(item);
                return list;
            }
            set
            {
                var collection = new Dictionary<ushort, SerializableColor>();
                if (value != null)
                    foreach (var item in value)
                        collection.Add(item.Key, item.Value);
                Instance.Colors = collection;
            }
        }


        public override void OnSaveData()
        {
            base.OnSaveData();
            
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, ColorData);
                serializableDataManager.SaveData(m_dataID, memoryStream.ToArray());
            }
        }

        public override void OnLoadData()
        {
            base.OnLoadData();            
            var data = serializableDataManager.LoadData(m_dataID);
            if (data == null || data.Length == 0) return;
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(data))
            {
                ColorData = binaryFormatter.Deserialize(memoryStream) as List<ColorEntry>;
            }
        }
    }
}
