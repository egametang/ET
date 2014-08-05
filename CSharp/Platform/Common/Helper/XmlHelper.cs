using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Helper
{
    public static class XmlHelper
    {
        /// <summary>
        /// 对象序列化成 XML String
        /// </summary>
        public static string XmlSerialize<T>(T obj)
        {
            string xmlString = string.Empty;
            var xmlSerializer = new XmlSerializer(typeof (T));
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, obj);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }

        /// <summary>
        /// XML String 反序列化成对象
        /// </summary>
        public static T XmlDeserialize<T>(string xmlString)
        {
            T t = default(T);
            var xmlSerializer = new XmlSerializer(typeof (T));
            Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
            using (var xmlReader = XmlReader.Create(xmlStream))
            {
                Object obj = xmlSerializer.Deserialize(xmlReader);
                t = (T) obj;
            }
            return t;
        }
    }
}