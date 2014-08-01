using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.Commons.Extensions;

namespace FieldSuite.ImageMapping
{
	public class FieldSuiteImageFactory
	{
		/// <summary>
		/// Returns the Interface for this item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static IFieldSuiteImage GetFieldSuiteImage(Item item)
		{
			if (item.IsNull())
			{
				return null;
			}

			XmlNode node = Factory.GetConfigNode("fieldSuite/fields/imagesField");
			if (node == null)
			{
				return null;
			}

			List<IFieldSuiteImage> mappings = GetFieldSuiteImageMappings(node, item);
			if (mappings == null || mappings.Count == 0)
			{
				return null;
			}

			return mappings.FirstOrDefault();
		}

		/// <summary>
		/// Returns Template Mappings
		/// </summary>
		/// <param name="node"></param>
		/// <param name="currentItem"></param>
		/// <returns></returns>
		public static List<IFieldSuiteImage> GetFieldSuiteImageMappings(XmlNode node, Item currentItem)
		{
			List<IFieldSuiteImage> imageMappings = new List<IFieldSuiteImage>();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				IFieldSuiteImage fieldSuiteImage = GetItem_FromXmlNode(childNode, currentItem);
				if (fieldSuiteImage == null)
				{
					continue;
				}

				imageMappings.Add(fieldSuiteImage);
			}
			return imageMappings;
		}

		/// <summary>
		/// Retrieve object from xml node
		/// </summary>
		/// <param name="node"></param>
		/// <param name="currentItem"></param>
		/// <returns></returns>
		private static IFieldSuiteImage GetItem_FromXmlNode(XmlNode node, Item currentItem)
		{
			if (node.Name != "mapping")
			{
				return null;
			}

			//only get for the current template
			if (node.Attributes["template"] == null || !node.Attributes["template"].Value.Contains(currentItem.TemplateID.ToString()))
			{
				return null;
			}

			if (node.Attributes["type"] != null && !string.IsNullOrEmpty(node.Attributes["type"].Value))
			{
				return GetItem_FromReflection(node, currentItem);
			}

			return null;
		}

		/// <summary>
		/// Return Velir Image from reflection
		/// </summary>
		/// <param name="node"></param>
		/// <param name="currentItem"></param>
		/// <param name="titleField"></param>
		/// <param name="imagefield"></param>
		/// <returns></returns>
		private static IFieldSuiteImage GetItem_FromReflection(XmlNode node, Item currentItem)
		{
			//verify we can break up the type string into a namespace and assembly name
			string[] split = node.Attributes["type"].Value.Split(',');
			if (split.Length == 0)
			{
				return null;
			}

			string Namespace = split[0];
			string AssemblyName = split[1];

			// load the assemly
			Assembly assembly = GetAssembly(AssemblyName);

			// Walk through each type in the assembly looking for our class
			Type type = assembly.GetType(Namespace);
			if (type == null || !type.IsClass)
			{
				return null;
			}

			FieldSuiteImageArgs args = new FieldSuiteImageArgs();
			args.InnerItem = currentItem;
			args.Node = node;

			object[] parameters = new object[1];
			parameters[0] = args;

			//cast to validator class
			IFieldSuiteImage fieldSuiteImage = (IFieldSuiteImage)Activator.CreateInstance(type, parameters);

			//validate
			return fieldSuiteImage;
		}

		/// <summary>
		/// Retrieve the Assembly
		/// </summary>
		/// <param name="AssemblyName"></param>
		/// <returns></returns>
		private static Assembly GetAssembly(string AssemblyName)
		{
			//try and find it in the currently loaded assemblies
			AppDomain appDomain = AppDomain.CurrentDomain;
			foreach (Assembly assembly in appDomain.GetAssemblies())
			{
				if (assembly.FullName == AssemblyName)
					return assembly;
			}

			//load assembly
			return appDomain.Load(AssemblyName);
		}
	}
}