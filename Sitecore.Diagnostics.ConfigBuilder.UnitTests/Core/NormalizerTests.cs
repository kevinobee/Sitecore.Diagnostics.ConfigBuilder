namespace Sitecore.Diagnostics.ConfigBuilder.UnitTests.Core
{
  using System;
  using System.Xml;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class NormalizerTests
  {
    [TestMethod]
    [DeploymentItem("Core\\NormalizerTestsData")]
    public void NormalizeTest()
    {
      {
        var filePath = Environment.CurrentDirectory + "\\1.in.showconfig.xml";
        var actual = Normalizer.Normalize(filePath);
        var expected = Environment.CurrentDirectory + "\\1.out.showconfig.xml.normalized.xml";
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected) as XmlDocument, actual, "1");
      }
    }

    [TestMethod]
    [DeploymentItem("Core\\NormalizerTestsData")]
    public void NormalizeTest2()
    {
      {
        var filePath = Environment.CurrentDirectory + "\\2.in.web.config.result.xml";
        var actual = Normalizer.Normalize(filePath);
        var expected = Environment.CurrentDirectory + "\\2.out.web.config.result.xml.normalized.xml";
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected), actual, "2");
      }
    }

    [TestMethod]
    [DeploymentItem("Core\\NormalizerTestsData")]
    public void NormalizeTestTwoSetting()
    {
      {
        var filePath = Environment.CurrentDirectory + "\\3.in.web.config.result.xml";
        var actual = Normalizer.Normalize(filePath);
        var expected = Environment.CurrentDirectory + "\\3.out.web.config.result.xml.normalized.xml";
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected), actual, "3");
      }
    }
  }
}
