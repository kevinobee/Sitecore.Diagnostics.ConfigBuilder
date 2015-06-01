namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.Xml;

  internal interface IXmlNode
  {
    // Properties
    string LocalName { get; }
    string NamespaceURI { get; }
    XmlNodeType NodeType { get; }
    string Prefix { get; }
    string Value { get; }
  }
}