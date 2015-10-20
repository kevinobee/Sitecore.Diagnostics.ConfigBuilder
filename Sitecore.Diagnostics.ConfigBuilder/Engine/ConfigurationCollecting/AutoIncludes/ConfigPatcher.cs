namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.IO;
  using System.Text;
  using System.Xml;
  using Sitecore.Diagnostics.Annotations;

  internal class ConfigPatcher
  {
    internal const string ConfigurationNamespace = "http://www.sitecore.net/xmlconfig/";

    internal const string SetNamespace = "http://www.sitecore.net/xmlconfig/set/";

    [NotNull]
    private readonly XmlPatcher Patcher;

    [NotNull]
    private readonly XmlNode Root;

    internal ConfigPatcher([NotNull] XmlNode node)
    {
      Assert.ArgumentNotNull(node, "node");

      this.Root = node;
      this.Patcher = new XmlPatcher(SetNamespace, ConfigurationNamespace);
    }

    internal void ApplyPatch(TextReader patch)
    {
      this.ApplyPatch(patch, string.Empty);
    }

    internal void ApplyPatch(string filename)
    {
      using (StreamReader reader = new StreamReader(filename, Encoding.UTF8))
      {
        this.ApplyPatch(reader, Path.GetFileName(filename));
      }
    }

    internal void ApplyPatch(TextReader patch, string sourceName)
    {
      var reader = new XmlTextReader(patch)
      {
        WhitespaceHandling = WhitespaceHandling.None
      };
      reader.MoveToContent();
      reader.ReadStartElement("configuration");
      this.Patcher.Merge(this.Root, new XmlReaderSource(reader, sourceName));
      reader.ReadEndElement();
    }

    internal XmlNode Document
    {
      get
      {
        return this.Root;
      }
    }
  }
}
