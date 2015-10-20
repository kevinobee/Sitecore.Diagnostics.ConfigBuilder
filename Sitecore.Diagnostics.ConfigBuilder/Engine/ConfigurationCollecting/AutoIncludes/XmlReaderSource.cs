namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System;
  using System.Collections.Generic;
  using System.Xml;
  using Sitecore.Diagnostics.Annotations;

  internal class XmlReaderSource : IXmlElement, IXmlSource
  {
    [CanBeNull]
    private readonly string sourceName;

    private readonly XmlReader Reader;

    private bool Incomplete;

    internal XmlReaderSource([NotNull] XmlReader reader)
      : this(reader, string.Empty)
    {
      Assert.ArgumentNotNull(reader, "reader");
    }

    internal XmlReaderSource([NotNull] XmlReader reader, [CanBeNull] string sourceName)
    {
      Assert.ArgumentNotNull(reader, "reader");

      this.Reader = reader;
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
        return this.Reader.NamespaceURI;
      }
    }

    public XmlNodeType NodeType
    {
      get
      {
        return this.Reader.NodeType;
      }
    }

    public string Prefix
    {
      get
      {
        return this.Reader.Prefix;
      }
    }


    [CanBeNull]
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
        return this.Reader.Value;
      }
    }

    public string LocalName
    {
      get
      {
        return this.Reader.LocalName;
      }
    }

    [NotNull]
    public IEnumerable<IXmlElement> GetChildren()
    {
      this.Incomplete = false;
      if (!this.Reader.IsEmptyElement)
      {
        this.Reader.ReadStartElement();
        while (this.Reader.NodeType != XmlNodeType.EndElement)
        {
          this.Incomplete = true;
          yield return this;
          if (!this.Incomplete)
          {
            continue;
          }

          this.Reader.ReadOuterXml();
          this.Incomplete = false;
        }

        this.Reader.ReadEndElement();
        yield break;
      }

      this.Reader.Read();
    }

    public IEnumerable<IXmlNode> GetAttributes()
    {
      bool successfullyMoved = this.Reader.MoveToFirstAttribute();
      while (successfullyMoved)
      {
        yield return this;
        successfullyMoved = this.Reader.MoveToNextAttribute();
      }
      this.Reader.MoveToElement();
    }
  }
}
