namespace Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers
{
  using System;
  using System.Xml;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;

  internal class XmlUtil
  {
    internal static XmlDocument LoadXmlFile(string filename, PathMapper pathMapper)
    {
      XmlDocument document = new XmlDocument();
      var mappedPath = pathMapper.MapPath(filename);
      document.Load(mappedPath);
      return document;
    }

    internal static void TransferAttributes(XmlNode source, XmlNode target)
    {
      foreach (XmlAttribute attribute in source.Attributes)
      {
        SetAttribute(attribute.Name, attribute.Value, target);
      }
    }

    internal static void SetAttribute(string name, string value, XmlNode node)
    {
      if ((node != null) && (node.Attributes != null))
      {
        if (node is XmlDocument)
        {
          node = ((XmlDocument)node).DocumentElement;
        }
        XmlAttribute attribute = node.Attributes[name];
        if (attribute != null)
        {
          attribute.Value = value;
        }
        else
        {
          string prefix = string.Empty;
          string namespaceURI = string.Empty;
          if (name.StartsWith("sc:", StringComparison.InvariantCulture))
          {
            prefix = "sc";
            namespaceURI = "Sitecore";
            name = name.Substring(3);
          }
          if (namespaceURI.Length > 0)
          {
            attribute = node.OwnerDocument.CreateAttribute(prefix, name, namespaceURI);
          }
          else
          {
            attribute = node.OwnerDocument.CreateAttribute(name);
          }
          attribute.Value = value;
          node.Attributes.Append(attribute);
        }
      }
    }


    internal static string GetAttribute(string name, XmlNode node)
    {
      if ((node != null) && (node.Attributes != null))
      {
        XmlNode node2 = node.Attributes[name];
        if (node2 != null)
        {
          return node2.Value;
        }
      }
      return string.Empty;
    }
  }
}
