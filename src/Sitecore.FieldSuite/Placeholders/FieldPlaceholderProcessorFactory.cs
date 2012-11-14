using System;
using System.Reflection;
using System.Web;
using System.Xml;
using Sitecore.Configuration;

namespace Sitecore.SharedSource.FieldSuite.Placeholders
{
	public class FieldPlaceholderProcessorFactory
	{
		/// <summary>
		/// Returns the Field Gutter Processor
		/// </summary>
		public static IFieldPlaceholderProcessor GetProcessor()
		{
			if (HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Processor"] != null)
			{
				return (IFieldPlaceholderProcessor)HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Processor"];
			}

			XmlNode fieldGutterNode = Factory.GetConfigNode("fieldSuite/fields/fieldPlaceholder");
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
				IFieldPlaceholderProcessor processor = (IFieldPlaceholderProcessor)Activator.CreateInstance(type);
				if (processor == null)
				{
					continue;
				}

				HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Processor"] = processor;
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
