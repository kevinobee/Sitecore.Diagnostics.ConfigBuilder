namespace Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers
{
  using System;
  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;

  internal static class XmlUtil
  {
    [NotNull]
    internal static XmlDocument LoadXmlFile([NotNull] string filename, [NotNull] PathMapper pathMapper)
    {
      Assert.ArgumentNotNull(filename, "filename");
      Assert.ArgumentNotNull(pathMapper, "pathMapper");

      var document = new XmlDocument();
      var mappedPath = pathMapper.MapPath(filename);
      document.Load(mappedPath);

      return document;
    }

    internal static void TransferAttributes([NotNull] XmlNode source, [CanBeNull] XmlNode target)
    {
      Assert.ArgumentNotNull(source, "source");

      foreach (XmlAttribute attribute in source.Attributes)
      {
        SetAttribute(attribute.Name, attribute.Value, target);
      }
    }

    internal static void SetAttribute([NotNull] string name, [NotNull] string value, [CanBeNull] XmlNode node)
    {
      Assert.ArgumentNotNull(name, "name");
      Assert.ArgumentNotNull(value, "value");

      if (node == null || node.Attributes == null)
      {
        return;
      }

      if (node is XmlDocument)
      {
        node = ((XmlDocument)node).DocumentElement;
      }

      var attribute = node.Attributes[name];
      if (attribute != null)
      {
        attribute.Value = value;
      }
      else
      {
        var prefix = string.Empty;
        var namespaceURI = string.Empty;
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


    [NotNull]
    internal static string GetAttribute([NotNull] string name, [CanBeNull] XmlNode node)
    {
      Assert.ArgumentNotNull(name, "name");

      if (node == null || node.Attributes == null)
      {
        return string.Empty;
      }

      var node2 = node.Attributes[name];
      if (node2 != null)
      {
        return node2.Value;
      }

      return string.Empty;
    }
  }
}
