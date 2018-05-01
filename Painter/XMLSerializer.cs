using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Painter
{
    [XmlRoot("PainterColorizer")]
    public class PainterColorizer
    {
        [XmlIgnore]
        private static readonly string configurationPath = Path.Combine(DataLocation.localApplicationData, "PainterColorizer.xml");
        public List<string> Colorized = new List<string>();
        public List<string> Inverted = new List<string>();
        public PainterColorizer() { }
        public void OnPreSerialize() { }
        public void OnPostDeserialize() { }

        public void Save()
        {
            var fileName = configurationPath;
            var config = Painter.instance.Colorizer;
            var serializer = new XmlSerializer(typeof(PainterColorizer));

            using (var writer = new StreamWriter(fileName))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
        }

        public static PainterColorizer Load()
        {
            var fileName = configurationPath;
            var serializer = new XmlSerializer(typeof(PainterColorizer));

            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    var config = serializer.Deserialize(reader) as PainterColorizer;                    
                    return config;
                }
            }
            catch (Exception)
            {
                return new PainterColorizer();
            }
        }
    }
}
