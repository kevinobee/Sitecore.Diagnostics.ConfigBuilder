namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System.Collections.Generic;
  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers;

  internal static class GlobalVariablesReplacer
  {
    internal static void ReplaceGlobalVariables([NotNull] XmlNode rootNode)
    {
      Assert.ArgumentNotNull(rootNode, "rootNode");

      var list = rootNode.SelectNodes(".//sc.variable");
      var variables = new Dictionary<string, string>();
      foreach (XmlAttribute attribute in rootNode.Attributes)
      {
        string name = attribute.Name;
        string attributeValue = attribute.Value;
        attributeValue = string.IsNullOrEmpty(attributeValue) ? string.Empty : attributeValue;
        if (name.Length <= 0)
        {
          continue;
        }

        var key = "$(" + name + ")";
        variables[key] = attributeValue;
      }

      for (var i = 0; i < list.Count; i++)
      {
        var name = XmlUtil.GetAttribute("name", list[i]);
        var value = XmlUtil.GetAttribute("value", list[i]);
        if (name.Length <= 0)
        {
          continue;
        }

        var key = "$(" + name + ")";
        variables[key] = value;
      }

      if (variables.Count != 0)
      {
        ReplaceGlobalVariables(rootNode, variables);
      }
    }

    private static void ReplaceGlobalVariables([NotNull] XmlNode node, [NotNull] Dictionary<string,string> variables)
    {
      Assert.ArgumentNotNull(node, "node");
      Assert.ArgumentNotNull(variables, "variables");

      foreach (XmlAttribute attribute in node.Attributes)
      {
        var value = attribute.Value;
        if (value.IndexOf('$') < 0)
        {
          continue;
        }

        foreach (string str2 in variables.Keys)
        {
          value = value.Replace(str2, variables[str2]);
        }

        attribute.Value = value;
      }

      foreach (XmlNode node2 in node.ChildNodes)
      {
        if (node2.NodeType == XmlNodeType.Element)
        {
          ReplaceGlobalVariables(node2, variables);
        }
      }
    }
  }
}
