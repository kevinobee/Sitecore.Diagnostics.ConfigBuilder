namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.Xml;

  internal class XmlPatcher
  {
    private XmlPatchNamespaces ns;

    internal XmlPatcher(string setNamespace, string patchNamespace)
    {
      XmlPatchNamespaces namespaces = new XmlPatchNamespaces
      {
        SetNamespace = setNamespace,
        PatchNamespace = patchNamespace
      };
      this.ns = namespaces;
    }

    internal void Merge(System.Xml.XmlNode target, XmlReaderSource patch)
    {
      XmlPatchUtils.MergeNodes(target, patch, this.ns);
    }

    internal void Merge(System.Xml.XmlNode target, XmlReader reader)
    {
      XmlPatchUtils.MergeNodes(target, new XmlReaderSource(reader), this.ns);
    }
  }
}
