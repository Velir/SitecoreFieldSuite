using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FieldSuite.Controls.GeneralLinks;

namespace FieldSuite.Util
{
	public class XmlUtil
	{
		public static string XmlSerializeToString(object objectInstance)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(GeneralLinkItem));
			StringBuilder builder = new StringBuilder();

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;

			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("", "");

			using (XmlWriter stringWriter = XmlWriter.Create(builder, settings))
			{
				serializer.Serialize(stringWriter, objectInstance, ns);
				return builder.ToString();
			}
		}

		public static T XmlDeserializeFromString<T>(string objectData)
		{
			return (T)XmlDeserializeFromString(objectData, typeof(T));
		}

		public static object XmlDeserializeFromString(string objectData, Type type)
		{
			var serializer = new XmlSerializer(type);
			object result;

			using (TextReader reader = new StringReader(objectData))
			{
				result = serializer.Deserialize(reader);
			}

			return result;
		}
	}
}
