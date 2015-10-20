namespace Sitecore.Diagnostics.ConfigBuilder.UnitTests.Core
{
  using System;
  using System.IO;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class BuilderTests
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

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\4")]
    public void BuildTestCD()
    {
      BuildShowConfig();
      BuildWebConfigResult();
    }

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\1")]
    public void NormalizeTest1()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var nonNormalized = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, true, false).OuterXml);
      var normalized = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, true, true).OuterXml);
      Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(normalized.OuterXml, nonNormalized.OuterXml);
    }

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\1")]
    public void NormalizeTest2()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var nonNormalized = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, false, false).OuterXml);
      var normalized = XmlDocumentEx.LoadXml(ConfigBuilder.Build(webConfigFilePath, false, true).OuterXml);
      Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(normalized.OuterXml, nonNormalized.OuterXml);
    }

    [TestMethod]
    [DeploymentItem("Core\\BuilderTestsData\\3")]
    public void BuildTestSpaces()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var actualFilePath = Environment.CurrentDirectory + "\\in\\showconfig.xml";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\showconfig.xml";
      var expected = XmlDocumentEx.LoadFile(expectedFilePath);
      var webConfigText = "             " + File.ReadAllText(webConfigFilePath);
      var webConfigModifiedFilePath = webConfigFilePath + "_modified.config";
      File.WriteAllText(webConfigModifiedFilePath, webConfigText);
      ConfigBuilder.Build(webConfigFilePath, actualFilePath, false, false);
      var output = XmlDocumentEx.LoadFile(actualFilePath);
      TestHelper.AreEqual(output, expected);
    }

    private static void BuildWebConfigResult()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var actualFilePath = Environment.CurrentDirectory + "\\in\\web.config.result.xml";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\web.config.result.xml";
      var expected = XmlDocumentEx.LoadFile(expectedFilePath);
      ConfigBuilder.Build(webConfigFilePath, actualFilePath, true, false);
      var output = XmlDocumentEx.LoadFile(actualFilePath);
      TestHelper.AreEqual(output, expected);
    }

    private static void BuildShowConfig()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var actualFilePath = Environment.CurrentDirectory + "\\in\\showconfig.xml";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\showconfig.xml";
      var expected = XmlDocumentEx.LoadFile(expectedFilePath);
      ConfigBuilder.Build(webConfigFilePath, actualFilePath, false, false);
      var output = XmlDocumentEx.LoadFile(actualFilePath);
      TestHelper.AreEqual(output, expected);
    }
  }
}
