namespace Sitecore.Diagnostics.ConfigBuilder
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Xml;
  using System.Xml.Linq;
  using System.Xml.XPath;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;

  public static class Normalizer 
  {
    [NotNull]
    private static string GetAttributeValueSafe([NotNull] string attributeName, [NotNull] XElement e)
    {
      Assert.ArgumentNotNull(attributeName, "attributeName");
      Assert.ArgumentNotNull(e, "e");

      try
      {
        XName name = XName.Get(attributeName);
        XAttribute attribute = e.Attribute(name);
        return attribute != null ? attribute.Value : string.Empty;
      }
      catch
      {
        return string.Empty;
      }
    }

    [NotNull]
    private static string GetResourceTextFile([NotNull] string filename)
    {
      Assert.ArgumentNotNull(filename, "filename");

      using (var stream = typeof(Normalizer).Assembly.GetManifestResourceStream("Sitecore.Diagnostics.ConfigBuilder." + filename))
      {
        using (var sr = new StreamReader(stream))
        {
          return sr.ReadToEnd();
        }
      }
    }

    public static void Normalize([NotNull] string filePath, [NotNull] string outputFilePath)
    {
      Assert.ArgumentNotNull(filePath, "filePath");
      Assert.ArgumentNotNull(outputFilePath, "outputFilePath");

      // code was lost and then restored from reflector
      var document = Normalize(filePath);
      document.Save(outputFilePath);
    }

    [NotNull]
    public static XmlDocument Normalize([NotNull] XmlDocument xmlDocument)
    {
      Assert.ArgumentNotNull(xmlDocument, "xmlDocument");

      var childElements = xmlDocument.SelectNodes("/configuration/sitecore/settings").OfType<XmlElement>();
      var settingsElements = childElements.Where(x => x.Name.Equals("settings", StringComparison.OrdinalIgnoreCase)).ToArray();
      if (settingsElements.Length > 1)
      {
        var first = settingsElements.First();
        var rest = settingsElements.Skip(1).ToArray();
        foreach (var settingsElement in rest)
        {
          var list = new List<XmlNode>();
          foreach (XmlNode childNode in settingsElement.ChildNodes)
          {
            list.Add(childNode);
          }

          foreach (XmlNode settingElement in list)
          {
            first.AppendChild(settingElement);
          }

          settingsElement.ParentNode.RemoveChild(settingsElement);
        }
      }

      var document = LoadDocument(xmlDocument);
      var normalizeXmlDocument = new XmlDocument();
      normalizeXmlDocument.LoadXml(GetFile("Normalize.xml"));
      foreach (XmlElement element in normalizeXmlDocument.DocumentElement.ChildNodes)
      {
        //if ((element != null) && (element.Name == "rule"))
        {
          try
          {
            IEnumerable enumerable;
            string expression = element.Attributes["path"].Value;
            XmlAttribute attribute = element.Attributes["attributeNames"];
            string str3 = (attribute != null) ? attribute.Value : string.Empty;
            try
            {
              enumerable = document.XPathSelectElements(expression);
            }
            catch (Exception)
            {
              continue;
            }
            foreach (XElement element2 in enumerable)
            {
              if (element2 != null)
              {
                IEnumerable<XElement> source = from n in element2.Nodes()
                  where n.NodeType == XmlNodeType.Element
                  select n as XElement
                  into e
                  orderby e.Name.LocalName
                  select e;
                Func<XElement, string> keySelector = null;
                Func<XElement, string> func2 = null;
                foreach (string str4 in str3.Split(new char[]
                {
                  '|'
                }))
                {
                  if (!string.IsNullOrEmpty(str4))
                  {
                    if (str4.ToLower() != "$innerxml$")
                    {
                      if (keySelector == null)
                      {
                        if (func2 == null)
                        {
                          string name = str4;
                          func2 = e => GetAttributeValueSafe(name, e);
                        }
                        keySelector = func2;
                      }
                      source = source.OrderBy(keySelector);
                    }
                    else
                    {
                      source = from e in source
                        orderby e.Value
                        select e;
                    }
                  }
                }
                element2.ReplaceNodes(source);
              }
            }
          }
          catch (Exception)
          {
          }
        }
      }

      var xx = new XmlDocument();
      xx.LoadXml(document.ToString());

      return xx;
    }

    [NotNull]
    private static XDocument LoadDocument([NotNull] XmlDocument xmlDocument)
    {
      Assert.ArgumentNotNull(xmlDocument, "xmlDocument");
      var settings = new XmlReaderSettings
      {
        IgnoreComments = true,
        IgnoreWhitespace = true
      };

      using (var stringReader = new StringReader(xmlDocument.OuterXml))
      {
        var reader = XmlReader.Create(stringReader, settings);

        return XDocument.Load(reader);
      }
    }

    [NotNull]
    public static XmlDocument Normalize([NotNull] string filePath)
    {
      Assert.ArgumentNotNull(filePath, "filePath");
      Assert.IsTrue(!string.IsNullOrEmpty(filePath) && File.Exists(filePath), "File does not exist");

      var tmp = Path.GetTempFileName();
      if (File.Exists(tmp))
      {
        File.Delete(tmp);
      }
      using (var file = File.OpenWrite(tmp))
      {
        var txt = new StringBuilder();
        foreach (var line in File.ReadAllLines(filePath))
        {
          if (line.Length > 2 && line[0] == '-' && line[1] == ' ')
          {
            txt.AppendLine(line.Substring(2));
            continue;
          }

          txt.AppendLine(line);
        }

        using (var writer = new StreamWriter(file))
        {
          string replace = txt.ToString();
          writer.Write(replace);
          writer.Close();
        }

        file.Close();
      }

      // Fix for issue SCB-2 to merge different <settings> elements
      var xmlDocument = new XmlDocument();
      xmlDocument.Load(tmp);

      return Normalize(xmlDocument);
    }

    [NotNull]
    private static string GetFile([NotNull] string fileName)
    {
      Assert.ArgumentNotNull(fileName, "fileName");
      string normalizeXmlFullPath = Path.Combine(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]),
        "Normalize.xml");
      if (File.Exists(normalizeXmlFullPath))
      {
        return File.ReadAllText(normalizeXmlFullPath);
      }

      return GetResourceTextFile(fileName);
    }
  }
}

/*

        var xml = document.ToString();

        var document2 = new XmlDocument();
        document2.LoadXml(xml);
        MergeElements(document2.DocumentElement);
        document2.Save(filePath + ".normalized.xml");
      }
    }

    private static void MergeElements(XmlNode parent)
    {
        
       { a, a, a[1], a[1], a[2], b, c, c[1] } => {
         a = {
           -, 1, 2
         },
         b = {
           -
         },
         c = { 
           -, 1
         }
       }
         

      IEnumerable<XmlElement> children = parent.ChildNodes.OfType<XmlElement>();

      foreach (var groupRaw in children.GroupBy(x => x.Name))
      {
        var group = groupRaw.ToArray();
        if (group.Length > 1)
        {
          foreach (var groupRaw2 in group.GroupBy(x => XmlHelper.GetAttributesQuantificator(x)))
          {
            var group2 = groupRaw2.ToArray();
            if (group2.Length > 1)
            {
              // move all children fron element 2..n into element 1 and then delete it
              var key = groupRaw2.Key;
              var destination = group2.First();
              var source = group2.Skip(1).ToArray();
              foreach (var element in source)
              {
                foreach (var element2 in element.ChildNodes.OfType<XmlNode>())
                {
                  if (element2 is XmlText)
                  {
                    continue;
                  }

                  XmlHelper.MoveElement(element2, destination);
                }
              }

            }
          }
        }
      }

      foreach (XmlNode child in parent.ChildNodes)
      {
        MergeElements(child);
  }
}

       */