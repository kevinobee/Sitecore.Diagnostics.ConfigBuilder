namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System;
  using System.Collections.Generic;
  using System.Xml;

  internal class XmlReaderSource : IXmlElement, IXmlNode, IXmlSource
  {
    private string sourceName;
    private readonly XmlReader reader;
    private bool incomplete;

    internal XmlReaderSource(XmlReader reader)
      : this(reader, string.Empty)
    {
    }

    internal XmlReaderSource(XmlReader reader, string sourceName)
    {
      this.reader = reader;
      this.sourceName = sourceName;
      if (reader.NodeType != XmlNodeType.Element)
      {
        throw new Exception("Reader is in incorrect state");
      }
    }

    public string NamespaceURI
    {
      get
      {
        return this.reader.NamespaceURI;
      }
    }

    public XmlNodeType NodeType
    {
      get
      {
        return this.reader.NodeType;
      }
    }

    public string Prefix
    {
      get
      {
        return this.reader.Prefix;
      }
    }


    public string SourceName
    {
      get
      {
        return this.sourceName;
      }
    }

    public string Value
    {
      get
      {
        return this.reader.Value;
      }
    }

    public string LocalName
    {
      get
      {
        return this.reader.LocalName;
      }
    }

    public IEnumerable<IXmlElement> GetChildren()
    {
      this.incomplete = false;
      if (!this.reader.IsEmptyElement)
      {
        this.reader.ReadStartElement();
        while (this.reader.NodeType != XmlNodeType.EndElement)
        {
          this.incomplete = true;
          yield return this;
          if (this.incomplete)
          {
            this.reader.ReadOuterXml();
            this.incomplete = false;
          }
        }
        this.reader.ReadEndElement();
        yield break;
      }
      this.reader.Read();
    }

    public IEnumerable<IXmlNode> GetAttributes()
    {
      bool successfullyMoved = this.reader.MoveToFirstAttribute();
      while (successfullyMoved)
      {
        yield return this;
        successfullyMoved = this.reader.MoveToNextAttribute();
      }
      this.reader.MoveToElement();
    }
  }
}
