namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.Xml;
  using Sitecore.Diagnostics.Annotations;

  internal class XmlPatcher
  {
    [NotNull]
    private readonly XmlPatchNamespaces Namespaces;

    internal XmlPatcher([NotNull] string setNamespace, [NotNull] string patchNamespace)
    {
      Assert.ArgumentNotNull(setNamespace, "setNamespace");
      Assert.ArgumentNotNull(patchNamespace, "patchNamespace");

      var namespaces = new XmlPatchNamespaces
      {
        SetNamespace = setNamespace,
        PatchNamespace = patchNamespace
      };

      this.Namespaces = namespaces;
    }

    internal void Merge([NotNull] XmlNode target, [NotNull] XmlReaderSource patch)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(patch, "patch");

      XmlPatchUtils.MergeNodes(target, patch, this.Namespaces);
    }

    internal void Merge([NotNull] XmlNode target, [NotNull] XmlReader reader)
    {
      Assert.ArgumentNotNull(target, "target");
      Assert.ArgumentNotNull(reader, "reader");

      XmlPatchUtils.MergeNodes(target, new XmlReaderSource(reader), this.Namespaces);
    }
  }
}
