namespace Sitecore.Diagnostics.ConfigBuilder
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes;

  public class ConfigBuilderEngine : IConfigBuilderEngine
  {
    #region IConfigBuilderEngine

    /// <summary>
    /// Takes Sitecore configuration and (using embedded Sitecore 7.1 configuration engine) depending on parameters makes and returns one of the following files (normalized or not): showconfig.xml or web.config.result.xml.
    /// </summary>
    /// <param name="webConfigFilePath">The path to the web.config file that is located side-by-side with App_Config folder and other files it may refer to.</param>
    /// <param name="outputFilePath">The path to the file where the result will be written to.</param>
    /// <param name="buildWebConfigResult">Indicates if showconfig.xml file should be merged into web.config file.</param>
    /// <param name="normalizeOutput">Indicates if the output file should be normalized the same way it is done in runtime.</param>
    public virtual void Build(string webConfigFilePath, string outputFilePath, bool buildWebConfigResult, bool normalizeOutput)
    {
      Assert.ArgumentNotNullOrEmpty(webConfigFilePath, "webConfigFilePath");
      Assert.ArgumentNotNull(outputFilePath, "outputFilePath");

      if (!File.Exists(webConfigFilePath))
      {
        throw new InvalidOperationException("The webConfigFilePath parameter points to missing file (" + webConfigFilePath + ")");
      }

      if (buildWebConfigResult)
      {
        using (var xmlTextWriter = this.OpenWriter(outputFilePath))
        {
          var webConfig = this.BuildWebConfigResult(webConfigFilePath);
          webConfig.Save(xmlTextWriter);
        }

        this.OptimizeNamespaces(outputFilePath);

        if (normalizeOutput)
        {
          Normalizer.Normalize(outputFilePath, outputFilePath + ".normalized.xml");
        }

        return;
      }

      using (var xmlTextWriter = this.OpenWriter(outputFilePath))
      {
        var showConfig = this.BuildShowConfig(webConfigFilePath);
        showConfig.Save(xmlTextWriter);
      }

      this.OptimizeNamespaces(outputFilePath);

      if (normalizeOutput)
      {
        Normalizer.Normalize(outputFilePath, outputFilePath + ".normalized.xml");
      }
    }

    /// <summary>
    /// Takes Sitecore configuration and (using embedded Sitecore 7.1 configuration engine) depending on parameters makes and returns one of the following files (normalized or not): showconfig.xml or web.config.result.xml.
    /// </summary>
    /// <param name="webConfigFilePath">The path to the web.config file that is located side-by-side with App_Config folder and other files it may refer to.</param>
    /// <param name="buildWebConfigResult">Indicates if showconfig.xml file should be merged into web.config file.</param>
    /// <param name="normalizeOutput">Indicates if the output file should be normalized the same way it is done in runtime.</param>
    /// <returns>The result xml document.</returns>
    public virtual XmlDocument Build(string webConfigFilePath, bool buildWebConfigResult, bool normalizeOutput)
    {
      Assert.ArgumentNotNullOrEmpty(webConfigFilePath, "webConfigFilePath");

      if (!File.Exists(webConfigFilePath))
      {
        throw new InvalidOperationException("The webConfigFilePath parameter points to missing file (" + webConfigFilePath + ")");
      }

      if (buildWebConfigResult)
      {
        var webConfig = this.BuildWebConfigResult(webConfigFilePath);

        var xmlDocument = normalizeOutput ? Normalizer.Normalize(webConfig) : webConfig;

        return this.OptimizeNamespaces(xmlDocument);
      }

      var showConfig = this.BuildShowConfig(webConfigFilePath);

      return this.OptimizeNamespaces(normalizeOutput ? Normalizer.Normalize(showConfig) : showConfig);
    }

    #endregion

    /// <summary>
    /// Opens XmlTextWriter with intended formatting enabled.
    /// </summary>
    /// <param name="outputFilePath">The output file path.</param>
    /// <returns>The XmlTextWriter with intended formatting enabled.</returns>
    [NotNull]
    protected virtual XmlTextWriter OpenWriter([NotNull] string outputFilePath)
    {
      Assert.ArgumentNotNullOrEmpty(outputFilePath, "outputFilePath");

      return new XmlTextWriter(new StreamWriter(outputFilePath)) { Formatting = Formatting.Indented };
    }

    /// <summary>
    /// Optimizes namespaces in a xml file.
    /// </summary>
    /// <param name="filePath">Path to a xml file.</param>
    protected virtual void OptimizeNamespaces([NotNull] string filePath)
    {
      Assert.ArgumentNotNullOrEmpty(filePath, "filePath");

      var xml = new XmlDocument();
      xml.Load(filePath);
      this.OptimizeNamespaces(xml);
      xml.Save(filePath);
    }

    /// <summary>
    /// Returns an xml document with optimized namespaces.
    /// </summary>
    /// <param name="configuration">Xml document.</param>
    /// <returns>Xml document with optimized namespaces.</returns>
    [NotNull]
    protected virtual XmlDocument OptimizeNamespaces([NotNull] XmlDocument configuration)
    {
      Assert.ArgumentNotNull(configuration, "configuration");

      var sitecoreElement = this.FindSitecoreNode(configuration);
      sitecoreElement.InnerXml = sitecoreElement.InnerXml.Replace(" xmlns:patch=\"http://www.sitecore.net/xmlconfig/\"", string.Empty);

      return configuration;
    }

    [NotNull]
    protected virtual XmlDocument BuildWebConfigResult([NotNull] string webConfigFilePath)
    {
      Assert.ArgumentNotNullOrEmpty(webConfigFilePath, "webConfigFilePath");
      Assert.IsTrue(File.Exists(webConfigFilePath), string.Format("The {0} file doesn't exist", webConfigFilePath));

      var webConfig = this.ReadConfigFile(webConfigFilePath);
      Assert.IsNotNull(webConfig, "webConfig");

      // fix for issue #4 - configSource attribute is not supported
      // merge files one into one, nested are not supported
      var websiteFolderPath = Path.GetDirectoryName(webConfigFilePath);
      foreach (var element in webConfig.DocumentElement.ChildNodes.OfType<XmlElement>())
      {
        Assert.IsNotNull(element, "element");

        var configSource = element.GetAttribute("configSource");
        if (!string.IsNullOrEmpty(configSource))
        {
          var filePath = configSource;
          if (filePath.Length > 2 && filePath[1] != ':')
          {
            filePath = filePath.StartsWith("~/") ? filePath.Substring(2) : filePath;
            filePath = filePath.TrimStart('/', '\\');
            filePath = Path.Combine(websiteFolderPath, filePath);
          }

          if (File.Exists(filePath))
          {
            var document = new XmlDocument();
            document.Load(filePath);
            XmlHelper.ReplaceElement(element, document.DocumentElement);
          }
        }
      }

      // <sitecore> node of web.config
      var sitecoreNode = this.FindSitecoreNode(webConfig);

      // <sitecore> node with merged <sc.include file="*.config" />, configSource="*.config" and App_Config\Include\**.config files
      var sitecoreConfiguration = this.ComputeSitecoreConfiguration(Path.GetDirectoryName(webConfigFilePath), webConfig);
      Assert.IsNotNull(sitecoreConfiguration, "sitecoreConfiguration");

      // replace /configuration/sitecore node with the real /sitecore one from showconfig.aspx 
      XmlHelper.ReplaceElement(sitecoreNode, sitecoreConfiguration.DocumentElement);

      // fix for issue #4 - configSource attribute is not supported
      // remove "configSource" attributes
      foreach (var element in webConfig.DocumentElement.ChildNodes.OfType<XmlElement>())
      {
        var configSource = element.GetAttribute("configSource");
        if (!string.IsNullOrEmpty(configSource))
        {
          element.RemoveAttribute("configSource");
        }
      }

      return webConfig;
    }

    [NotNull]
    protected virtual XmlDocument BuildShowConfig([NotNull] string webConfigFilePath)
    {
      Assert.ArgumentNotNullOrEmpty(webConfigFilePath, "webConfigFilePath");
      Assert.IsTrue(File.Exists(webConfigFilePath), "The web.config file doesn't exist");

      var webConfig = this.BuildWebConfigResult(webConfigFilePath);

      // /configuration/sitecore node of web.config
      var sitecoreNode = this.FindSitecoreNode(webConfig);

      var document = new XmlDocument();
      document.LoadXml(sitecoreNode.OuterXml);
      return document;
    }

    [NotNull]
    protected XmlDocument ReadConfigFile([NotNull] string configFilePath)
    {
      Assert.ArgumentNotNullOrEmpty(configFilePath, "configFilePath");

      var raw = File.ReadAllText(configFilePath);
      var xml = this.RepairXml(raw);
      var xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(xml);

      return xmlDocument;
    }

    [NotNull]
    protected virtual XmlDocument ComputeSitecoreConfiguration([NotNull] string webRootPath, [NotNull] XmlDocument configuration)
    {
      Assert.ArgumentNotNullOrEmpty(webRootPath, "webRootPath");
      Assert.ArgumentNotNull(configuration, "configuration");

      var sitecoreNode = this.FindSitecoreNode(configuration);
      
      foreach (XmlNode attribute in sitecoreNode.Attributes)
      {
        if (attribute != null && attribute.NamespaceURI == ConfigPatcher.RoleNamespace)
        {
          XmlPatchUtils.ProcessRolesNamespace(attribute.LocalName, attribute.Value);
        }
      }

      var sitecoreConfiguration = new XmlDocument();

      var pathMapper = new PathMapper(webRootPath);

      // Add <sitecore /> node from web.config
      sitecoreConfiguration.AppendChild(sitecoreConfiguration.ImportNode(sitecoreNode, true));

      // Merge <sc.include path="*.config" />
      IncludeFileExpander.ExpandIncludeFiles(sitecoreConfiguration.DocumentElement, new Hashtable(), pathMapper);

      // Merge App_Config/Include/**.config files
      var includeFoldersPaths = this.GetIncludeFoldersPaths();
      AutoIncludeFilesLoader.LoadAutoIncludeFiles(sitecoreConfiguration.DocumentElement, pathMapper, includeFoldersPaths);

      // Replace variables like $(variableName)
      GlobalVariablesReplacer.ReplaceGlobalVariables(sitecoreConfiguration.DocumentElement);

      return sitecoreConfiguration;
    }

    [NotNull]
    protected virtual string RepairXml([NotNull] string text)
    {
      Assert.ArgumentNotNull(text, "text");

      return text
        .Replace("\"<>", "&quot;&lt;&gt;")
        .Replace("find=\"&\"", "find=\"&amp;\"")
        .Replace("value=\"http://sitecore1.maxmind.com/app/sc?i={0}&l={1}\"", "value=\"http://sitecore1.maxmind.com/app/sc?i={0}&amp;l={1}\"")
        .Trim();
    }

    [NotNull]
    protected virtual IEnumerable<string> GetIncludeFoldersPaths()
    {
      return new[]
      {
        "/App_Config/Sitecore/Components", 
        "/App_Config/Include"
      };
    }

    [NotNull]
    protected virtual XmlElement FindSitecoreNode([NotNull] XmlDocument configuration)
    {
      Assert.ArgumentNotNull(configuration, "configuration");

      var sitecoreNode = configuration.SelectSingleNode("configuration/sitecore") as XmlElement;
      sitecoreNode = sitecoreNode ?? configuration.SelectSingleNode("configuration/location/sitecore") as XmlElement;
      sitecoreNode = sitecoreNode ?? configuration.SelectSingleNode("sitecore") as XmlElement;
      Assert.IsNotNull(sitecoreNode, "<sitecore> node is missing");

      return sitecoreNode;
    }
  }
}