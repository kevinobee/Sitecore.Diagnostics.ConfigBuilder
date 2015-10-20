namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Xml;
  using Sitecore.Diagnostics;
  using Sitecore.Diagnostics.Annotations;

  internal static class XmlPatchUtils
  {
    [NotNull]
    private static readonly object SyncRoot = new object();

    [NotNull]
    private static readonly List<string> definedRoles = new List<string>();

    [PublicAPI]
    [NotNull]
    public static IEnumerable<string> DefinedRoles
    {
      get
      {
        return definedRoles.ToArray();
      }
    }

    internal static void AssignAttributes([NotNull] XmlNode target, [NotNull] IEnumerable<IXmlNode> attributes)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(attributes, "attributes");

      foreach (IXmlNode node in attributes)
      {
        Assert.IsNotNull(target.Attributes, "attributes");

        var attribute = target.Attributes[node.LocalName, node.NamespaceURI];
        if (attribute == null)
        {
          Assert.IsNotNull(target.OwnerDocument, "document");

          attribute = target.OwnerDocument.CreateAttribute(MakeName(node.Prefix, node.LocalName), node.NamespaceURI);
          target.Attributes.Append(attribute);
        }

        attribute.Value = node.Value;
      }
    }

    private static void AssignSource([NotNull] XmlNode target, [NotNull] object source, [NotNull] XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(ns, "ns");

      var source2 = source as IXmlSource;
      if (source2 == null)
      {
        return;
      }

      var sourceName = source2.SourceName;
      if (string.IsNullOrEmpty(sourceName))
      {
        return;
      }

      var prefixOfNamespace = target.OwnerDocument.GetPrefixOfNamespace(ns.PatchNamespace);
      if (string.IsNullOrEmpty(prefixOfNamespace))
      {
        prefixOfNamespace = "patch";
        XmlNode documentElement = target.OwnerDocument.DocumentElement;
        XmlAttribute node = target.OwnerDocument.CreateAttribute("xmlns:" + prefixOfNamespace);
        node.Value = ns.PatchNamespace;
        documentElement.Attributes.Append(node);
      }

      var attribute2 = target.Attributes["source", ns.PatchNamespace];
      if (attribute2 == null)
      {
        attribute2 = target.OwnerDocument.CreateAttribute(prefixOfNamespace, "source", ns.PatchNamespace);
        target.Attributes.Append(attribute2);
      }

      attribute2.Value = sourceName;
    }

    internal static void CopyAttributes([NotNull] XmlNode target, [NotNull] IXmlElement patch, [NotNull] XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");

      var source1 = from a in patch.GetAttributes()
                    where (a.NamespaceURI != ns.PatchNamespace && a.NamespaceURI != ns.RoleNamespace) && (a.NamespaceURI != "http://www.w3.org/2000/xmlns/")
                    select new XmlNodeInfo { NodeType = a.NodeType, NamespaceURI = (a.NamespaceURI == ns.SetNamespace) ? string.Empty : a.NamespaceURI, LocalName = a.LocalName, Value = a.Value, Prefix = a.Prefix };
      var source = source1.ToArray();
      if (!source.Any())
      {
        return;
      }

      AssignAttributes(target, source);
      AssignSource(target, patch, ns);
    }

    private static bool InsertChild([NotNull] XmlNode parent, [NotNull] XmlNode child, [CanBeNull] InsertOperation operation)
    {
      Assert.ArgumentNotNull(parent, "parent");
      Assert.ArgumentNotNull(child, "child");

      if (operation == null)
      {
        parent.AppendChild(child);

        return true;
      }

      var refChild = parent.SelectSingleNode(operation.Reference);
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

    internal static bool IsXmlPatch([NotNull] string value)
    {
      Assert.ArgumentNotNull(value, "value");

      return value.IndexOf("p:p=\"1\"", StringComparison.InvariantCulture) >= 0;
    }

    private static string MakeName([CanBeNull] string prefix, [NotNull] string localName)
    {
      Assert.ArgumentNotNull(localName, "localName");

      if (!string.IsNullOrEmpty(prefix))
      {
        return prefix + ":" + localName;
      }

      return localName;
    }

    private static void MergeChildren([NotNull] XmlNode target, [NotNull] IXmlElement patch, [NotNull] XmlPatchNamespaces ns, bool targetWasInserted)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");

      string data = null;
      var stack = new Stack<InsertOperation>();
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
            var source = new List<IXmlNode>();
            var attributes = new List<IXmlNode>();
            InsertOperation operation = null;
            var exit = false;
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
                var item = new XmlNodeInfo
                {
                  NodeType = node.NodeType,
                  NamespaceURI = string.Empty,
                  LocalName = node.LocalName,
                  Prefix = string.Empty,
                  Value = node.Value
                };
                attributes.Add(item);
              }
              else if (node.NamespaceURI == ns.RoleNamespace)
              {
                exit = !ProcessRolesNamespace(node);
              }
              else if (node.Prefix != "xmlns")
              {
                var info2 = new XmlNodeInfo
                {
                  NodeType = node.NodeType,
                  NamespaceURI = node.NamespaceURI,
                  LocalName = node.LocalName,
                  Prefix = node.Prefix,
                  Value = node.Value
                };
                source.Add(info2);
              }

              if (exit)
              {
                break;
              }
            }

            nsManager = new XmlNamespaceManager(new NameTable());
            var strArray = source.Select(delegate(IXmlNode a)
            {
              if ((a.Prefix != null) && string.IsNullOrEmpty(nsManager.LookupPrefix(a.Prefix)))
              {
                nsManager.AddNamespace(a.Prefix, a.NamespaceURI);
              }

              return ("@" + MakeName(a.Prefix, a.LocalName) + "=\"" + a.Value + "\"");
            }).ToArray();

            if ((element.Prefix != null) && string.IsNullOrEmpty(nsManager.LookupPrefix(element.Prefix)))
            {
              nsManager.AddNamespace(element.Prefix, element.NamespaceURI);
            }

            XmlNode child = null;
            var flag = false;
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
              var newChild = child.OwnerDocument.CreateComment(data);
              Assert.IsNotNull(child.ParentNode, "parent");

              child.ParentNode.InsertBefore(newChild, child);
              data = null;
            }

            AssignAttributes(child, attributes);
            MergeChildren(child, element, ns, flag);
            if ((flag || attributes.Any()) && !targetWasInserted)
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

    internal static bool ProcessRolesNamespace([NotNull] IXmlNode node)
    {
      Assert.ArgumentNotNull(node, "node");

      var name = node.LocalName;
      var value = node.Value;
      return ProcessRolesNamespace(name, value);
    }

    internal static bool ProcessRolesNamespace(string name, string value)
    {
      switch (name)
      {
        case "d":
        case "define":
          if (!string.IsNullOrEmpty(value))
          {
            var roles = value.Split("|,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
              lock (SyncRoot)
              {
                if (definedRoles.All(x => !role.Equals(x, StringComparison.OrdinalIgnoreCase)))
                {
                  definedRoles.Add(role);
                }
              }
            }
          }

          break;

        case "r":
        case "require":
          if (!string.IsNullOrEmpty(value) && definedRoles.All(x => !value.Equals(x, StringComparison.OrdinalIgnoreCase)))
          {
            return false;
          }

          break;
      }
      return true;
    }

    internal static void MergeNodes([NotNull] XmlNode target, [NotNull] IXmlElement patch, [NotNull] XmlPatchNamespaces ns)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");
      Assert.ArgumentNotNull(ns, "ns");

      foreach (var node in patch.GetAttributes())
      {
        if (node.NamespaceURI != ns.RoleNamespace)
        {
          continue;
        }

        if (!ProcessRolesNamespace(patch))
        {
          return;
        }
      }

      if ((target.NamespaceURI == patch.NamespaceURI) && (target.LocalName == patch.LocalName))
      {
        CopyAttributes(target, patch, ns);
        MergeChildren(target, patch, ns, false);
      }
    }

    private static void ProcessConfigNode([NotNull] XmlNode target, [NotNull] IXmlElement command)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(command, "command");

      var dictionary = command.GetAttributes().ToDictionary<IXmlNode, string, string>(a => a.LocalName, a => a.Value);
      var localName = command.LocalName;
      if (localName != null)
      {
        if (localName != "a" && localName != "attribute")
        {
          if (localName != "d" && localName != "delete")
          {
            return;
          }
        }
        else
        {
          string str;
          string str2;
          dictionary.TryGetValue("ns", out str);
          Assert.IsNotNull(target.Attributes, "attributes");

          var node = target.Attributes[str, dictionary["name"]];
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
