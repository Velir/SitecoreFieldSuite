using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Data.Items;

namespace FieldSuite.FieldGutter
{
	public class FieldGutterProcessorFactory
	{
		/// <summary>
		/// Returns the Field Gutter Processor
		/// </summary>
		public static IFieldGutterProcessor GetProcessor()
		{
			if (HttpContext.Current.Cache["FieldSuite.FieldGutter.Processor"] != null)
			{
				return (IFieldGutterProcessor)HttpContext.Current.Cache["FieldSuite.FieldGutter.Processor"];
			}

			XmlNode fieldGutterNode = Factory.GetConfigNode("fieldSuite/fields/fieldGutter");
			if (fieldGutterNode == null || fieldGutterNode.ChildNodes.Count == 0)
			{
				return null;
			}

			foreach (XmlNode node in fieldGutterNode.ChildNodes)
			{
				if (node.Name != "processor")
				{
					continue;
				}

				string fullNameSpace = node.Attributes["type"].Value;

				//check to verify that xml was not malformed
				if (string.IsNullOrEmpty(fullNameSpace))
				{
					continue;
				}

				//verify we can break up the type string into a namespace and assembly name
				string[] split = fullNameSpace.Split(',');
				if (split.Length == 0)
				{
					continue;
				}

				string nameSpace = split[0];
				string assemblyName = split[1];

				// load the assemly
				Assembly assembly = GetAssembly(assemblyName);

				// Walk through each type in the assembly looking for our class
				Type type = assembly.GetType(nameSpace);
				if (type == null || !type.IsClass)
				{
					continue;
				}

				//cast to processor interface class
				IFieldGutterProcessor processor = (IFieldGutterProcessor)Activator.CreateInstance(type);
				if (processor == null)
				{
					continue;
				}

				//set max count
				processor.MaxCount = 0;
				Int32 maxCount;
				if (node.Attributes["maxcount"] != null && !string.IsNullOrEmpty(node.Attributes["maxcount"].Value) && Int32.TryParse(node.Attributes["maxcount"].Value, out maxCount))
				{
					processor.MaxCount = maxCount;
				}

				HttpContext.Current.Cache["FieldSuite.FieldGutter.Processor"] = processor;
				return processor;
			}

			return null;
		}

		/// <summary>
		/// Using Reflection, returns the Assembly
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
