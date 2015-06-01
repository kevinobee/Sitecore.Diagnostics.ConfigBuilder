namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.Xml;

  internal class XmlNodeInfo : IXmlNode
  {
    public string LocalName { get; set; }

    public string NamespaceURI { get; set; }

    public XmlNodeType NodeType { get; set; }

    public string Prefix { get; set; }

    public string Value { get; set; }
  }
}
