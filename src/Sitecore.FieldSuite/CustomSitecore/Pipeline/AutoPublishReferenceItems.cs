using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.Publish;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.Pipeline
{
	public class AutoPublishReferenceItems : PublishProcessor
	{
		private static ILog _logger;

		public static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(AutoPublishReferenceItems));
				}
				return _logger;
			}
		}

		/// <summary>
		/// Templates to be AutoPublished
		/// </summary>
		/// <returns></returns>
		private List<string> AllowedTemplates
		{
			get
			{
				XmlNodeList velirNodes = Factory.GetConfigNode("fieldSuite").ChildNodes;
				if (velirNodes.Count == 0)
				{
					return null;
				}

				foreach (XmlNode node in velirNodes)
				{
					if (node.Name != "add" || node.Attributes["key"] == null)
					{
						continue;
					}

					XmlAttribute keyAttribute = node.Attributes["key"];
					if(keyAttribute == null || keyAttribute.Value != "AutoPublishFieldValues.Templates")
					{
						continue;
					}

					XmlAttribute valueAttribute = node.Attributes["value"];
					if(valueAttribute == null)
					{
						//unable to find attribute
						Logger.Warn("Sitecore.SharedSource.Commons - AutoPublishReferenceItems - Not able to read the value attribute of AutoPublishFieldValues.Templates in the config file");
						return null;
					}

					if (string.IsNullOrEmpty(valueAttribute.Value))
					{
						return null;
					}

					return valueAttribute.Value.Split('|').ToList();
				}

				return null;
			}
		}

		/// <summary>
		/// AutoPublishing
		/// </summary>
		/// <returns></returns>
		private bool AutoPublish
		{
			get
			{
				XmlNodeList velirNodes = Factory.GetConfigNode("fieldSuite").ChildNodes;
				if (velirNodes.Count == 0)
				{
					return false;
				}

				foreach (XmlNode node in velirNodes)
				{
					if (node.Name != "add" || node.Attributes["key"] == null)
					{
						continue;
					}

					XmlAttribute keyAttribute = node.Attributes["key"];
					if (keyAttribute == null || keyAttribute.Value != "AutoPublishFieldValues")
					{
						continue;
					}

					XmlAttribute valueAttribute = node.Attributes["value"];
					if (valueAttribute == null)
					{
						//unable to find attribute
						Logger.Warn("Sitecore.SharedSource.Commons - AutoPublishReferenceItems - Not able to read the value attribute of AutoPublishFieldValues.Templates in the config file");
						return false;
					}

					if (string.IsNullOrEmpty(valueAttribute.Value))
					{
						return false;
					}

					if (valueAttribute.Value == "1")
					{
						return true;
					}

					break;
				}

				return false;
			}
		}

		// Methods
		private IEnumerable<PublishingCandidate> GetSourceItems(PublishOptions options)
		{
			if (options.Mode == PublishMode.Incremental)
			{
				return PublishQueue.GetPublishQueue(options);
			}
			return PublishQueue.GetContentBranch(options);
		}

		public override void Process(PublishContext context)
		{
			Assert.ArgumentNotNull(context, "context");

			//orginal call to get source items
			List<PublishingCandidate> sourceItems = this.GetSourceItems(context.PublishOptions).ToList();

			//auto publish if enabled, the root item matches the allowable templates
			//verify item is not null and it is a content item
			Item item = context.PublishOptions.RootItem;
			if (item.IsNotNull() && item.Paths.IsContentItem && AutoPublish && AllowableTemplate(item.TemplateID.ToString()))
			{
				List<PublishingCandidate> additionalItems = GetAdditionalPublishingCandidates(context);
				if (additionalItems.Count > 0)
				{
					try
					{
						sourceItems.AddRange(additionalItems);
					}
					catch (Exception e)
					{
						Logger.Error("FieldSuite - AutoPublishReferenceItems - Error Adding to the Publishing Queue");
						Logger.Error("FieldSuite - AutoPublishReferenceItems - ItemId:" + item.ID);
						Logger.Error("FieldSuite - AutoPublishReferenceItems - TemplateId: " + item.TemplateID);
						Logger.Error("FieldSuite - AutoPublishReferenceItems - Path: " + item.Paths.FullPath);
						Logger.Error(e.InnerException);
						Logger.Error(e.Message);
						return;
					}
				}
			}

			//add to context
			context.Queue.Add(sourceItems);
		}

		/// <summary>
		/// Determines if auto-publishing is available for this template
		/// </summary>
		/// <param name="templateId"></param>
		/// <returns></returns>
		private bool AllowableTemplate(string templateId)
		{
			if (string.IsNullOrEmpty(templateId))
			{
				return false;
			}

			//if no template is specified, its understood as allow all
			List<string> allowableTemplates = AllowedTemplates;
			if (allowableTemplates == null)
			{
				return true;
			}

			return allowableTemplates.Contains(templateId);
		}

		/// <summary>
		/// Returns the additional items to be published from the velir images field
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public List<PublishingCandidate> GetAdditionalPublishingCandidates(PublishContext context)
		{
			List<PublishingCandidate> additionalItems = new List<PublishingCandidate>();

			Item item = context.PublishOptions.RootItem;
			TemplateItem template = item.Template;
			foreach (TemplateFieldItem field in template.Fields)
			{
				//verify not a standard/system field
				if (field.InnerItem.IsNotNull() && field.InnerItem.Paths.FullPath.ToLower().StartsWith("/sitecore/templates/system"))
				{
					continue;
				}

				//get field value
				string fieldValue = item[field.Name];
				if (string.IsNullOrEmpty(fieldValue))
				{
					continue;
				}

				//split into guid array
				string[] values = fieldValue.Split('|');
				if (values.Length == 0)
				{
					continue;
				}

				foreach (string fieldValueItemId in values)
				{
					if (string.IsNullOrEmpty(fieldValueItemId))
					{
						continue;
					}

					ID additionalItemId;
					if (!ID.TryParse(fieldValueItemId, out additionalItemId))
					{
						continue;
					}

					PublishOptions options = new PublishOptions(context.PublishOptions.SourceDatabase,
																context.PublishOptions.TargetDatabase,
																PublishMode.Smart,
																item.Language, context.PublishOptions.PublishDate);
					PublishingCandidate publishingCandidate = new PublishingCandidate(additionalItemId, options);
					additionalItems.Add(publishingCandidate);
				}
			}

			return additionalItems;
		}
	}
}