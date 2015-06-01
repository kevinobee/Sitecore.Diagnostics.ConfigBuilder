namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System;
  using System.Collections;
  using System.Xml;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers;

  internal class IncludeFileExpander
  {
    internal static void ExpandIncludeFiles(XmlNode rootNode, Hashtable cycleDetector, PathMapper pathMapper)
    {
      if (rootNode.LocalName == "sc.include")
      {
        ExpandIncludeFile(rootNode, cycleDetector, pathMapper);
      }
      else
      {
        XmlNodeList list = rootNode.SelectNodes(".//sc.include");
        for (int i = 0; i < list.Count; i++)
        {
          ExpandIncludeFile(list[i], cycleDetector, pathMapper);
        }
      }
    }

    private static void ExpandIncludeFile(XmlNode xmlNode, Hashtable cycleDetector, PathMapper pathMapper)
    {
      string filePath = GetAttribute("file", xmlNode, null).ToLowerInvariant();
      if (filePath.Length != 0)
      {
        if (cycleDetector.ContainsKey(filePath))
        {
          throw new InvalidOperationException(
            string.Format(
              "Cycle detected in configuration include files. The file '{0}' is being included directly or indirectly in a way that causes a cycle to form.",
              filePath));
        }

        XmlDocument document = XmlUtil.LoadXmlFile(filePath,pathMapper);
        if (document.DocumentElement != null)
        {
          XmlNode parentNode = xmlNode.ParentNode;
          XmlNode newChild = xmlNode.OwnerDocument.ImportNode(document.DocumentElement, true);
          parentNode.ReplaceChild(newChild, xmlNode);
          cycleDetector.Add(filePath, string.Empty);
          ExpandIncludeFiles(newChild, cycleDetector, pathMapper);
          cycleDetector.Remove(filePath);
          while (newChild.FirstChild != null)
          {
            parentNode.AppendChild(newChild.FirstChild);
          }
          foreach (XmlNode node3 in newChild.ChildNodes)
          {
            parentNode.AppendChild(node3);
          }
          XmlUtil.TransferAttributes(newChild, parentNode);
          parentNode.RemoveChild(newChild);
        }
      }
    }

    private static string ReplaceVariables(string value, XmlNode node, string[] parameters)
    {
      node = node.ParentNode;
      while (((node != null) && (node.NodeType == XmlNodeType.Element)) && (value.IndexOf("$(", StringComparison.InvariantCulture) >= 0))
      {
        foreach (XmlAttribute attribute in node.Attributes)
        {
          string oldValue = "$(" + attribute.LocalName + ")";
          value = value.Replace(oldValue, attribute.Value);
        }
        value = value.Replace("$(name)", node.LocalName);
        node = node.ParentNode;
      }
      if (parameters != null)
      {
        for (int i = 0; i < parameters.Length; i++)
        {
          value = value.Replace("$(" + i + ")", parameters[i]);
        }
      }
      return value;
    }

    private static string GetAttribute(string name, XmlNode node, string[] parameters)
    {
      return ReplaceVariables(XmlUtil.GetAttribute(name, node), node, parameters);
    }
  }
}