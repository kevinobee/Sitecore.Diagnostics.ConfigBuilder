namespace Sitecore.Diagnostics.ConfigBuilder
{
  using System.Xml;
  using Sitecore.Diagnostics.Annotations;

  public interface IConfigBuilderEngine
  {
    /// <summary>
    /// Takes Sitecore configuration and (using embedded Sitecore 7.1 configuration engine) depending on parameters makes and returns one of the following files (normalized or not): showconfig.xml or web.config.result.xml.
    /// </summary>
    /// <param name="webConfigFilePath">The path to the web.config file that is located side-by-side with App_Config folder and other files it may refer to.</param>
    /// <param name="outputFilePath">The path to the file where the result will be written to.</param>
    /// <param name="buildWebConfigResult">Indicates if showconfig.xml file should be merged into web.config file.</param>
    /// <param name="normalizeOutput">Indicates if the output file should be normalized the same way it is done in runtime.</param>
    void Build([NotNull] string webConfigFilePath, [NotNull] string outputFilePath, bool buildWebConfigResult, bool normalizeOutput);

    /// <summary>
    /// Takes Sitecore configuration and (using embedded Sitecore 7.1 configuration engine) depending on parameters makes and returns one of the following files (normalized or not): showconfig.xml or web.config.result.xml.
    /// </summary>
    /// <param name="webConfigFilePath">The path to the web.config file that is located side-by-side with App_Config folder and other files it may refer to.</param>
    /// <param name="buildWebConfigResult">Indicates if showconfig.xml file should be merged into web.config file.</param>
    /// <param name="normalizeOutput">Indicates if the output file should be normalized the same way it is done in runtime.</param>
    /// <returns>The result xml document.</returns>
    [NotNull]
    XmlDocument Build([NotNull] string webConfigFilePath, bool buildWebConfigResult, bool normalizeOutput);
  }
}