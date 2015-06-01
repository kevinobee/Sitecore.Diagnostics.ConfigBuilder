namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes
{
  using System.IO;
  using System.Text;
  using System.Xml;

  internal class ConfigPatcher
  {
    private XmlPatcher _patcher = new XmlPatcher("http://www.sitecore.net/xmlconfig/set/", "http://www.sitecore.net/xmlconfig/");
    private System.Xml.XmlNode _root;
    internal const string ConfigurationNamespace = "http://www.sitecore.net/xmlconfig/";
    internal const string SetNamespace = "http://www.sitecore.net/xmlconfig/set/";

    internal ConfigPatcher(System.Xml.XmlNode node)
    {
      this._root = node;
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
      XmlTextReader reader = new XmlTextReader(patch)
      {
        WhitespaceHandling = WhitespaceHandling.None
      };
      reader.MoveToContent();
      reader.ReadStartElement("configuration");
      this._patcher.Merge(this._root, new XmlReaderSource(reader, sourceName));
      reader.ReadEndElement();
    }

    internal System.Xml.XmlNode Document
    {
      get
      {
        return this._root;
      }
    }
  }
}
