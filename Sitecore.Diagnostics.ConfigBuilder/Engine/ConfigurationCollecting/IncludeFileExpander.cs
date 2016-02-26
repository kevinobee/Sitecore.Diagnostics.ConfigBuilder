namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System;
  using System.Collections;
  using System.IO.Abstractions;
  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers;

  internal class IncludeFileExpander
  {
    private readonly IFileSystem FileSystem;

    internal IncludeFileExpander([NotNull ]IFileSystem fileSystem)
    {
      Assert.ArgumentNotNull(fileSystem, "fileSystem");

      this.FileSystem = fileSystem;
    }

    internal void ExpandIncludeFiles([NotNull] XmlNode rootNode, [NotNull] Hashtable cycleDetector, [NotNull] PathMapper pathMapper)
    {
      Assert.ArgumentNotNull(rootNode, "rootNode");
      Assert.ArgumentNotNull(cycleDetector, "cycleDetector");
      Assert.ArgumentNotNull(pathMapper, "pathMapper");

      if (rootNode.LocalName == "sc.include")
      {
        this.ExpandIncludeFile(rootNode, cycleDetector, pathMapper);
      }
      else
      {
        var list = rootNode.SelectNodes(".//sc.include");
        for (var i = 0; i < list.Count; i++)
        {
          this.ExpandIncludeFile(list[i], cycleDetector, pathMapper);
        }
      }
    }

    private void ExpandIncludeFile([NotNull] XmlNode xmlNode, [NotNull] Hashtable cycleDetector, [NotNull] PathMapper pathMapper)
    {
      Assert.ArgumentNotNull(xmlNode, "xmlNode");
      Assert.ArgumentNotNull(cycleDetector, "cycleDetector");
      Assert.ArgumentNotNull(pathMapper, "pathMapper");

      var filePath = GetAttribute("file", xmlNode, null).ToLowerInvariant();
      if (filePath.Length == 0)
      {
        return;
      }

      if (cycleDetector.ContainsKey(filePath))
      {
        throw new InvalidOperationException(
          string.Format(
            "Cycle detected in configuration include files. The file '{0}' is being included directly or indirectly in a way that causes a cycle to form.",
            filePath));
      }

      XmlDocument document = XmlUtil.LoadXml(this.FileSystem, filePath, pathMapper);
      if (document.DocumentElement == null)
      {
        return;
      }

      var parentNode = xmlNode.ParentNode;
      var newChild = xmlNode.OwnerDocument.ImportNode(document.DocumentElement, true);
      parentNode.ReplaceChild(newChild, xmlNode);
      cycleDetector.Add(filePath, string.Empty);
      this.ExpandIncludeFiles(newChild, cycleDetector, pathMapper);
      cycleDetector.Remove(filePath);
      while (newChild.FirstChild != null)
      {
        parentNode.AppendChild(newChild.FirstChild);
      }

      foreach (XmlNode child in newChild.ChildNodes)
      {
        parentNode.AppendChild(child);
      }

      XmlUtil.TransferAttributes(newChild, parentNode);
      parentNode.RemoveChild(newChild);
    }

    [CanBeNull]
    private static string ReplaceVariables([NotNull] string value, [NotNull] XmlNode node, [CanBeNull] string[] parameters)
    {
      Assert.ArgumentNotNull(value, "value");
      Assert.ArgumentNotNull(node, "node");

      node = node.ParentNode;
      while (node != null && node.NodeType == XmlNodeType.Element && value.IndexOf("$(", StringComparison.InvariantCulture) >= 0)
      {
        foreach (XmlAttribute attribute in node.Attributes)
        {
          var oldValue = "$(" + attribute.LocalName + ")";
          value = value.Replace(oldValue, attribute.Value);
        }

        value = value.Replace("$(name)", node.LocalName);
        node = node.ParentNode;
      }

      if (parameters == null)
      {
        return value;
      }

      for (var i = 0; i < parameters.Length; i++)
      {
        value = value.Replace("$(" + i + ")", parameters[i]);
      }

      return value;
    }

    [CanBeNull]
    private static string GetAttribute([NotNull] string name, [NotNull] XmlNode node, [CanBeNull] string[] parameters)
    {
      Assert.ArgumentNotNull(name, "name");
      Assert.ArgumentNotNull(node, "node");

      return ReplaceVariables(XmlUtil.GetAttribute(name, node), node, parameters);
    }
  }
}