namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System.Collections.Generic;
  using System.Xml;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Helpers;

  internal class GlobalVariablesReplacer
  {
    internal static void ReplaceGlobalVariables(XmlNode rootNode)
    {
      XmlNodeList list = rootNode.SelectNodes(".//sc.variable");
      var variables = new Dictionary<string,string>();
      foreach (XmlAttribute attribute in rootNode.Attributes)
      {
        string name = attribute.Name;
        string attributeValue = attribute.Value;
        attributeValue = string.IsNullOrEmpty(attributeValue) ? string.Empty : attributeValue;
        if (name.Length > 0)
        {
          string str3 = "$(" + name + ")";
          variables[str3] = attributeValue;
        }
      }
      for (int i = 0; i < list.Count; i++)
      {
        string str4 = XmlUtil.GetAttribute("name", list[i]);
        string str5 = XmlUtil.GetAttribute("value", list[i]);
        if (str4.Length > 0)
        {
          string str6 = "$(" + str4 + ")";
          variables[str6] = str5;
        }
      }
      if (variables.Count != 0)
      {
        ReplaceGlobalVariables(rootNode, variables);
      }
    }

    private static void ReplaceGlobalVariables(XmlNode node, Dictionary<string,string> variables)
    {
      foreach (XmlAttribute attribute in node.Attributes)
      {
        string str = attribute.Value;
        if (str.IndexOf('$') >= 0)
        {
          foreach (string str2 in variables.Keys)
          {
            str = str.Replace(str2, variables[str2]);
          }
          attribute.Value = str;
        }
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
