using System.IO.Abstractions;
using System.Security.AccessControl;

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
        var normalizer = this.GetNormalizer();
        var filePath = Environment.CurrentDirectory + "\\1.in.showconfig.xml";
        var actualPath = Environment.CurrentDirectory + "\\1.in.showconfig.xml.normalized.xml";
        normalizer.Normalize(filePath, actualPath);
        var expected = Environment.CurrentDirectory + "\\1.out.showconfig.xml.normalized.xml";
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected), XmlDocumentEx.LoadFile(actualPath), "1");
      }
    }

    [TestMethod]
    [DeploymentItem("Core\\NormalizerTestsData")]
    public void NormalizeTest2()
    {
      {
        var normalizer = this.GetNormalizer();
        var filePath = Environment.CurrentDirectory + "\\2.in.web.config.result.xml";
        var actualPath = Environment.CurrentDirectory + "\\1.in.showconfig.xml.normalized.xml";
        var expected = Environment.CurrentDirectory + "\\2.out.web.config.result.xml.normalized.xml";
        normalizer.Normalize(filePath, actualPath);
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected), XmlDocumentEx.LoadFile(actualPath), "2");
      }
    }

    [TestMethod]
    [DeploymentItem("Core\\NormalizerTestsData")]
    public void NormalizeTestTwoSetting()
    {
      {
        var normalizer = this.GetNormalizer();
        var filePath = Environment.CurrentDirectory + "\\3.in.web.config.result.xml";
        var actualPath = Environment.CurrentDirectory + "\\1.in.showconfig.xml.normalized.xml";
        var expected = Environment.CurrentDirectory + "\\3.out.web.config.result.xml.normalized.xml";
        normalizer.Normalize(filePath, actualPath);
        TestHelper.AreEqual(XmlDocumentEx.LoadFile(expected), XmlDocumentEx.LoadFile(actualPath), "3");
      }
    }

    private Normalizer GetNormalizer()
    {
      return new Normalizer(new FileSystem());
    }
  }
}
