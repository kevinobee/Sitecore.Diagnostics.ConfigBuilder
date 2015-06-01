namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Xml;
  using Sitecore.Diagnostics;

  internal class XmlPatchUtils
  {
    internal static void AssignAttributes(XmlNode target, IEnumerable<IXmlNode> attributes)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(attributes, "attributes");
      foreach (IXmlNode node in attributes)
      {
        Assert.IsNotNull(target.Attributes, "attributes");
        XmlAttribute attribute = target.Attributes[node.LocalName, node.NamespaceURI];
        if (attribute == null)
        {
          Assert.IsNotNull(target.OwnerDocument, "document");
          attribute = target.OwnerDocument.CreateAttribute(MakeName(node.Prefix, node.LocalName), node.NamespaceURI);
          target.Attributes.Append(attribute);
        }
        attribute.Value = node.Value;
      }
    }

    private static void AssignSource(XmlNode target, object source, XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(ns, "ns");
      IXmlSource source2 = source as IXmlSource;
      if (source2 != null)
      {
        string sourceName = source2.SourceName;
        if (!string.IsNullOrEmpty(sourceName))
        {
          string prefixOfNamespace = target.OwnerDocument.GetPrefixOfNamespace(ns.PatchNamespace);
          if (string.IsNullOrEmpty(prefixOfNamespace))
          {
            prefixOfNamespace = "patch";
            XmlNode documentElement = target.OwnerDocument.DocumentElement;
            XmlAttribute node = target.OwnerDocument.CreateAttribute("xmlns:" + prefixOfNamespace);
            node.Value = ns.PatchNamespace;
            documentElement.Attributes.Append(node);
          }
          XmlAttribute attribute2 = target.Attributes["source", ns.PatchNamespace];
          if (attribute2 == null)
          {
            attribute2 = target.OwnerDocument.CreateAttribute(prefixOfNamespace, "source", ns.PatchNamespace);
            target.Attributes.Append(attribute2);
          }
          attribute2.Value = sourceName;
        }
      }
    }

    internal static void CopyAttributes(XmlNode target, IXmlElement patch, XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");
      var source1 = from a in patch.GetAttributes()
                                     where (a.NamespaceURI != ns.PatchNamespace) && (a.NamespaceURI != "http://www.w3.org/2000/xmlns/")
                                     select new XmlNodeInfo { NodeType = a.NodeType, NamespaceURI = (a.NamespaceURI == ns.SetNamespace) ? string.Empty : a.NamespaceURI, LocalName = a.LocalName, Value = a.Value, Prefix = a.Prefix };
      var source = source1.ToArray();
      if (source.Any())
      {
        AssignAttributes(target, source);
        AssignSource(target, patch, ns);
      }
    }

    private static bool InsertChild(XmlNode parent, XmlNode child, InsertOperation operation)
    {
      Assert.ArgumentNotNull(parent, "parent");
      Assert.ArgumentNotNull(child, "child");
      if (operation == null)
      {
        parent.AppendChild(child);
        return true;
      }
      XmlNode refChild = parent.SelectSingleNode(operation.Reference);
      if (refChild == null)
      {
        parent.AppendChild(child);
        return false;
      }
      switch (operation.Disposition)
      {
        case 'a':
          parent.InsertAfter(child, refChild);
          return true;

        case 'b':
          parent.InsertBefore(child, refChild);
          return true;

        case 'i':
          parent.InsertBefore(child, refChild);
          parent.RemoveChild(refChild);
          return true;
      }
      throw new Exception("Insert operation is not implemented");
    }

    internal static bool IsXmlPatch(string value)
    {
      Assert.ArgumentNotNull(value, "value");
      return (value.IndexOf("p:p=\"1\"", StringComparison.InvariantCulture) >= 0);
    }

    private static string MakeName(string prefix, string localName)
    {
      Assert.ArgumentNotNull(localName, "localName");
      if (!string.IsNullOrEmpty(prefix))
      {
        return (prefix + ":" + localName);
      }
      return localName;
    }

    private static void MergeChildren(XmlNode target, IXmlElement patch, XmlPatchNamespaces ns, bool targetWasInserted)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");
      string data = null;
      Stack<InsertOperation> stack = new Stack<InsertOperation>();
      foreach (IXmlElement element in patch.GetChildren())
      {
        XmlNamespaceManager nsManager;
        if (element.NodeType == XmlNodeType.Text)
        {
          target.InnerText = element.Value;
        }
        else if (element.NodeType == XmlNodeType.Comment)
        {
          data = element.Value;
        }
        else if (element.NodeType == XmlNodeType.Element)
        {
          if (element.NamespaceURI == ns.PatchNamespace)
          {
            ProcessConfigNode(target, element);
          }
          else
          {
            List<IXmlNode> source = new List<IXmlNode>();
            List<IXmlNode> attributes = new List<IXmlNode>();
            InsertOperation operation = null;
            foreach (IXmlNode node in element.GetAttributes())
            {
              if (node.NamespaceURI == ns.PatchNamespace)
              {
                string str3;
                if (((str3 = node.LocalName) != null) && ((((str3 == "b") || (str3 == "before")) || ((str3 == "a") || (str3 == "after"))) || ((str3 == "i") || (str3 == "instead"))))
                {
                  operation = new InsertOperation
                  {
                    Reference = node.Value,
                    Disposition = node.LocalName[0]
                  };
                }
              }
              else if (node.NamespaceURI == ns.SetNamespace)
              {
                XmlNodeInfo item = new XmlNodeInfo
                {
                  NodeType = node.NodeType,
                  NamespaceURI = string.Empty,
                  LocalName = node.LocalName,
                  Prefix = string.Empty,
                  Value = node.Value
                };
                attributes.Add(item);
              }
              else if (node.Prefix != "xmlns")
              {
                XmlNodeInfo info2 = new XmlNodeInfo
                {
                  NodeType = node.NodeType,
                  NamespaceURI = node.NamespaceURI,
                  LocalName = node.LocalName,
                  Prefix = node.Prefix,
                  Value = node.Value
                };
                source.Add(info2);
              }
            }
            nsManager = new XmlNamespaceManager(new NameTable());
            string[] strArray = source.Select<IXmlNode, string>(delegate(IXmlNode a)
            {
              if ((a.Prefix != null) && string.IsNullOrEmpty(nsManager.LookupPrefix(a.Prefix)))
              {
                nsManager.AddNamespace(a.Prefix, a.NamespaceURI);
              }
              return ("@" + MakeName(a.Prefix, a.LocalName) + "=\"" + a.Value + "\"");
            }).ToArray<string>();
            if ((element.Prefix != null) && string.IsNullOrEmpty(nsManager.LookupPrefix(element.Prefix)))
            {
              nsManager.AddNamespace(element.Prefix, element.NamespaceURI);
            }
            XmlNode child = null;
            bool flag = false;
            if (!targetWasInserted)
            {
              string xpath = MakeName(element.Prefix, element.LocalName);
              if (strArray.Length > 0)
              {
                xpath = xpath + "[" + string.Join(" and ", strArray) + "]";
              }
              child = target.SelectSingleNode(xpath, nsManager);
            }
            if (child == null)
            {
              Assert.IsNotNull(target.OwnerDocument, "document");
              child = target.OwnerDocument.CreateElement(MakeName(element.Prefix, element.LocalName), element.NamespaceURI);
              flag = true;
              if (!InsertChild(target, child, operation) && (operation != null))
              {
                operation.Node = child;
                stack.Push(operation);
              }
              AssignAttributes(child, source);
            }
            else if ((operation != null) && !InsertChild(target, child, operation))
            {
              operation.Node = child;
              stack.Push(operation);
            }
            if (data != null)
            {
              Assert.IsNotNull(child.OwnerDocument, "document");
              XmlComment newChild = child.OwnerDocument.CreateComment(data);
              Assert.IsNotNull(child.ParentNode, "parent");
              child.ParentNode.InsertBefore(newChild, child);
              data = null;
            }
            AssignAttributes(child, attributes);
            MergeChildren(child, element, ns, flag);
            if ((flag || attributes.Any<IXmlNode>()) && !targetWasInserted)
            {
              AssignSource(child, element, ns);
            }
          }
        }
      }
      while (stack.Count > 0)
      {
        InsertOperation operation3 = stack.Pop();
        Assert.IsNotNull(operation3.Node.ParentNode, "parent");
        InsertChild(operation3.Node.ParentNode, operation3.Node, operation3);
      }
    }

    internal static void MergeNodes(XmlNode target, IXmlElement patch, XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");
      if ((target.NamespaceURI == patch.NamespaceURI) && (target.LocalName == patch.LocalName))
      {
        CopyAttributes(target, patch, ns);
        MergeChildren(target, patch, ns, false);
      }
    }

    private static void ProcessConfigNode(XmlNode target, IXmlElement command)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(command, "command");
      Dictionary<string, string> dictionary = command.GetAttributes().ToDictionary<IXmlNode, string, string>(a => a.LocalName, a => a.Value);
      string localName = command.LocalName;
      if (localName != null)
      {
        if (!(localName == "a") && !(localName == "attribute"))
        {
          if (!(localName == "d") && !(localName == "delete"))
          {
            return;
          }
        }
        else
        {
          string str;
          string str2;
          if (!dictionary.TryGetValue("ns", out str))
          {
            str = null;
          }
          Assert.IsNotNull(target.Attributes, "attributes");
          XmlAttribute node = target.Attributes[str, dictionary["name"]];
          if (node == null)
          {
            Assert.IsNotNull(target.OwnerDocument, "document");
            node = target.OwnerDocument.CreateAttribute(dictionary["name"], str);
            target.Attributes.Append(node);
          }
          if (!dictionary.TryGetValue("value", out str2))
          {
            str2 = string.Empty;
          }
          foreach (IXmlElement element in command.GetChildren())
          {
            str2 = element.Value ?? str2;
          }
          node.Value = str2;
          return;
        }
        Assert.IsNotNull(target.ParentNode, "parent");
        target.ParentNode.RemoveChild(target);
      }
    }

    // Nested Types
    private class InsertOperation
    {
      // Properties
      internal char Disposition { get; set; }

      internal XmlNode Node { get; set; }

      public string Reference { get; set; }

      internal bool Succeeded { get; set; }
    }

  }
}
