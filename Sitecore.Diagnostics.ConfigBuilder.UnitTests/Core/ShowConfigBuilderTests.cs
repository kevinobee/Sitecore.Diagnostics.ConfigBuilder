namespace Sitecore.Diagnostics.ConfigBuilder.Tests.Core
{
  using System;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class ConfigurationCollectorBuilderTests
  {
    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\1")]
    public void BuildTest()
    {
      BuildShowConfig();
      BuildWebConfigResult();
    }

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\2")]
    public void BuildTestConfigSource()
    {
      BuildWebConfigResult();
    }

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\3")]
    public void BuildTestTilde()
    {
      BuildShowConfig();
      BuildWebConfigResult();
    }

    private static void BuildWebConfigResult()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\web.config.result.xml";
      var expected = XmlDocumentEx.LoadFile(expectedFilePath);
      var output = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, true, false).OuterXml);
      TestHelper.AreEqual(output, expected);
      //Assert.AreEqual(output, File.ReadAllText(expectedFilePath));
    }

    private static void BuildShowConfig()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\showconfig.xml";
      var expected = XmlDocumentEx.LoadFile(expectedFilePath);
      var output = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, false, false).OuterXml);
      TestHelper.AreEqual(output, expected);
    }
  }
}
