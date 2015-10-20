namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.Collections.Generic;

  internal interface IXmlElement : IXmlNode
  {
    IEnumerable<IXmlNode> GetAttributes();

    IEnumerable<IXmlElement> GetChildren();
  }
}
