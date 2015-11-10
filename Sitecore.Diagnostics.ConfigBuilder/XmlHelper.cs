namespace Sitecore.Diagnostics.ConfigBuilder
{
  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;

  public static class XmlHelper
  {
    internal static void ReplaceElement([NotNull] XmlElement oldElement, [NotNull] XmlElement newElement)
    {
      Assert.ArgumentNotNull(oldElement, "oldElement");
      Assert.ArgumentNotNull(newElement, "newElement");

      // replace inner XML
      oldElement.InnerXml = newElement.InnerXml;

      // remove existing attributes
      oldElement.RemoveAllAttributes();

      // add attributes
      foreach (XmlAttribute att in newElement.Attributes)
      {
        oldElement.SetAttribute(att.Name, att.Value);
      }
    }
  }
}