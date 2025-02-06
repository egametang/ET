using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Link
{
    internal class LinkXmlWriter
    {
        public void Write(string outputLinkXmlFile, HashSet<TypeRef> refTypes)
        {
            string parentDir = Directory.GetParent(outputLinkXmlFile).FullName;
            Directory.CreateDirectory(parentDir);
            var writer = System.Xml.XmlWriter.Create(outputLinkXmlFile,
                new System.Xml.XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true});

            writer.WriteStartDocument();
            writer.WriteStartElement("linker");

            var typesByAssembly = refTypes.GroupBy(t => t.DefinitionAssembly.Name.String).ToList();
            typesByAssembly.Sort((a, b) => String.Compare(a.Key, b.Key, StringComparison.Ordinal));

            foreach(var assembly in typesByAssembly)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", assembly.Key);
                List<string> assTypeNames = assembly.Select(t => t.FullName).ToList();
                assTypeNames.Sort(string.CompareOrdinal);
                foreach(var typeName in assTypeNames)
                {
#if UNITY_2023_1_OR_NEWER
                    if (typeName == "UnityEngine.Debug")
                    {
                        continue;
                    }
#endif
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", typeName);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
