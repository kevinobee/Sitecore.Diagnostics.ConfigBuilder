using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sitecore.ConfigBuilder.Tests.Core
{
  [TestClass]
  public class RunnerTests
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

    private static void BuildWebConfigResult()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\web.config.result.xml";
      var kernel = Runner.FindKernel();
      Assert.IsFalse(string.IsNullOrEmpty(kernel), "kernel is null");
      var output = Runner.Build(webConfigFilePath, kernel, true, false).OuterXml;
      Assert.AreEqual(File.ReadAllText(expectedFilePath), output);
    }

    private static void BuildShowConfig()
    {
      string webConfigFilePath = Environment.CurrentDirectory + "\\in\\web.config";
      var expectedFilePath = Environment.CurrentDirectory + "\\out\\showconfig.xml";
      var kernel = Runner.FindKernel();
      Assert.IsFalse(string.IsNullOrEmpty(kernel), "kernel is null");
      var output = Runner.Build(webConfigFilePath, kernel, false, false).OuterXml;
      Assert.AreEqual(File.ReadAllText(expectedFilePath), output);
    }
  }
}
