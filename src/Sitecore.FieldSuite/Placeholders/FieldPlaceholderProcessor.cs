using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.Placeholders
{
	public class FieldPlaceholderProcessor : IFieldPlaceholderProcessor
	{
		private static List<IFieldPlaceholder> _fieldPlaceholderItems;

		/// <summary>
		/// Returns the Placeholder Items
		/// </summary>
		private static List<IFieldPlaceholder> FieldPlaceholderItems
		{
			get
			{
				if (HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Items"] != null)
				{
					return (List<IFieldPlaceholder>)HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Items"];
				}

				XmlNode itemComparerNode = Factory.GetConfigNode("fieldSuite/fields/fieldPlaceholder");
				if (itemComparerNode == null || itemComparerNode.ChildNodes.Count == 0)
				{
					return null;
				}

				_fieldPlaceholderItems = new List<IFieldPlaceholder>();
				foreach (XmlNode node in itemComparerNode.ChildNodes)
				{
					IFieldPlaceholder fieldPlaceholder = GetItem_FromXmlNode(node);
					if (fieldPlaceholder == null)
					{
						continue;
					}

					_fieldPlaceholderItems.Add(fieldPlaceholder);
				}

				HttpContext.Current.Cache["FieldSuite.FieldPlaceholder.Items"] = _fieldPlaceholderItems;
				return _fieldPlaceholderItems;
			}
		}

		/// <summary>
		/// Process Field Placeholders
		/// </summary>
		/// <param name = "args"></param>
		/// <returns></returns>
		public string Process(FieldPlaceholderArgs args)
		{
			if (args == null)
			{
				return string.Empty;
			}

			//list check
			List<IFieldPlaceholder> fieldPlaceholders = FieldPlaceholderItems;
			if (fieldPlaceholders == null || fieldPlaceholders.Count == 0)
			{
				return string.Empty;
			}

			foreach (IFieldPlaceholder fieldPlaceholder in fieldPlaceholders)
			{
				if (fieldPlaceholder == null)
				{
					continue;
				}

				//validate item
				string processedClickEvent = fieldPlaceholder.Execute(args);
				if (string.IsNullOrEmpty(processedClickEvent))
				{
					continue;
				}

				//add output of validation to the master validation list
				args.ClickEvent = processedClickEvent;
			}

			//return all output validation
			return args.ClickEvent;
		}

		/// <summary>
		/// Uses reflection to instantiate the IFieldPlaceholder class
		/// </summary>
		/// <param name="nameSpace"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		private static IFieldPlaceholder GetItem_FromReflection(string nameSpace, string assemblyName)
		{
			// load the assemly
			Assembly assembly = GetAssembly(assemblyName);

			// Walk through each type in the assembly looking for our class
			Type type = assembly.GetType(nameSpace);
			if (type == null || !type.IsClass)
			{
				return null;
			}

			//cast to validator class
			IFieldPlaceholder fieldPlaceholder = (IFieldPlaceholder)Activator.CreateInstance(type);
			if (fieldPlaceholder == null)
			{
				return null;
			}

			return fieldPlaceholder;
		}

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

		private static IFieldPlaceholder GetItem_FromXmlNode(XmlNode validationNode)
		{
			if (validationNode.Name != "placeholderItem")
			{
				return null;
			}

			string fullNameSpace = validationNode.Attributes["type"].Value;

			//check to verify that xml was not malformed
			if (string.IsNullOrEmpty(fullNameSpace))
			{
				return null;
			}

			//verify we can break up the type string into a namespace and assembly name
			string[] split = fullNameSpace.Split(',');
			if (split.Length == 0)
			{
				return null;
			}

			string nameSpace = split[0];
			string assemblyName = split[1];

			IFieldPlaceholder fieldPlaceholder = GetItem_FromReflection(nameSpace, assemblyName);
			if (fieldPlaceholder == null)
			{
				return null;
			}

			//set max count
			fieldPlaceholder.Key = null;
			if (validationNode.Attributes["key"] != null && !string.IsNullOrEmpty(validationNode.Attributes["key"].Value))
			{
				if (!string.IsNullOrEmpty(validationNode.Attributes["key"].Value))
				{
					fieldPlaceholder.Key = validationNode.Attributes["key"].Value;
				}
			}

			return fieldPlaceholder;
		}
	}
}