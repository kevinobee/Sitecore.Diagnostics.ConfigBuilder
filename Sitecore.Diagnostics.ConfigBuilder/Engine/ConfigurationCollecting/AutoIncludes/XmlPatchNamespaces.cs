namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using Sitecore.Diagnostics.Base.Annotations;

  internal class XmlPatchNamespaces
  {
    [NotNull]
    public string PatchNamespace { get; set; }

    [NotNull]
    public string SetNamespace { get; set; }

    [NotNull]
    public string RoleNamespace { get; set; }
  }
}
