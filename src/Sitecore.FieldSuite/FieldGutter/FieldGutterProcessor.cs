using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.FieldGutter
{
	public class FieldGutterProcessor : IFieldGutterProcessor
	{
		protected List<IFieldGutter> _fieldGutterItems;

		/// <summary>
		/// Max Number of Field Items for the Processor to run against
		/// </summary>
		/// <returns></returns>
		public virtual Int32 MaxCount { get; set; }

		/// <summary>
		/// Process Item's Field Gutter
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public virtual string Process(FieldGutterArgs args)
		{
			if (args == null || args.InnerItem == null)
			{
				return string.Empty;
			}

			//list check
			List<IFieldGutter> fieldGutters = FieldGutterItems;
			if (fieldGutters == null || fieldGutters.Count == 0)
			{
				return string.Empty;
			}

			string outputHtml = string.Empty;
			foreach (IFieldGutter fieldGutter in fieldGutters)
			{
				if (fieldGutter == null)
				{
					continue;
				}

				//validate item
				string html = fieldGutter.Execute(args);
				if (string.IsNullOrEmpty(html))
				{
					continue;
				}

				//add output of validation to the master validation list
				outputHtml += html;
			}

			//return all output validation
			return string.Format("<div id=\"{1}_{2}_fieldGutterDiv\">{0}</div>", outputHtml, args.FieldId, args.InnerItem.ID);
		}

		/// <summary>
		/// Returns the Gutter Items
		/// </summary>
		protected virtual List<IFieldGutter> FieldGutterItems
		{
			get
			{
				if (HttpContext.Current.Cache["FieldSuite.FieldGutter.Items"] != null)
				{
					return (List<IFieldGutter>)HttpContext.Current.Cache["FieldSuite.FieldGutter.Items"];
				}

				XmlNode itemComparerNode = Factory.GetConfigNode("fieldSuite/fields/fieldGutter");
				if (itemComparerNode == null || itemComparerNode.ChildNodes.Count == 0)
				{
					return null;
				}

				_fieldGutterItems = new List<IFieldGutter>();
				foreach (XmlNode node in itemComparerNode.ChildNodes)
				{
					IFieldGutter fieldGutter = GetItem_FromXmlNode(node);
					if (fieldGutter == null)
					{
						continue;
					}

					_fieldGutterItems.Add(fieldGutter);
				}

				HttpContext.Current.Cache["FieldSuite.FieldGutter.Items"] = _fieldGutterItems;
				return _fieldGutterItems;
			}
		}

		/// <summary>
		/// Uses reflection to instantiate the IFieldGutter class
		/// </summary>
		/// <param name="nameSpace"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		protected virtual IFieldGutter GetItem_FromReflection(string nameSpace, string assemblyName)
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
			IFieldGutter fieldGutter = (IFieldGutter)Activator.CreateInstance(type);
			if (fieldGutter == null)
			{
				return null;
			}

			return fieldGutter;
		}

		protected virtual Assembly GetAssembly(string AssemblyName)
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

		protected virtual IFieldGutter GetItem_FromXmlNode(XmlNode validationNode)
		{
			if (validationNode.Name != "gutterItem")
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

			//return from cache
			if (HttpContext.Current.Cache["FieldSuite.FieldGutter." + nameSpace] != null)
			{
				return (IFieldGutter)HttpContext.Current.Cache["FieldSuite.FieldGutter." + nameSpace];
			}
			
			IFieldGutter fieldGutter = GetItem_FromReflection(nameSpace, assemblyName);
			if (fieldGutter == null)
			{
				return null;
			}

			//set max count
			fieldGutter.MaxCount = 0;
			if (validationNode.Attributes["maxcount"] != null && !string.IsNullOrEmpty(validationNode.Attributes["maxcount"].Value))
			{
				Int32 maxCount = 0;
				if (Int32.TryParse(validationNode.Attributes["maxcount"].Value, out maxCount))
				{
					fieldGutter.MaxCount = maxCount;
				}
			}

			HttpContext.Current.Cache["FieldSuite.FieldGutter." + nameSpace] = fieldGutter;
			return fieldGutter;
		}
	}
}