using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Diagnostics.ConfigBuilder.UnitTests
{
  using System.IO.Abstractions;
  using System.IO.Abstractions.TestingHelpers;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass]
  public class NoLeadingSlashTests
  {
    [TestMethod]
    public void Test()
    {
      var webConfigPath = "C:\\web.config";
      var webConfigText = "<configuration><sitecore><sc.include file=\"file.config\" /></sitecore></configuration>";

      var fileConfigPath = "C:\\file.config";
      var fileConfigText = "<configuration><settings></settings></configuration>";

      var expected = "<configuration><sitecore><settings/></sitecore></configuration>";

      var fs = new MockFileSystem();
      fs.AddFile(webConfigPath, new MockFileData(webConfigText));
      fs.AddFile(fileConfigPath, new MockFileData(fileConfigText));
      fs.AddDirectory("C:\\App_Config\\Include");
      var result = new ConfigBuilderEngine(fs).Build(webConfigPath, true, false);

      Assert.AreEqual(expected, result.OuterXml.Replace(" ", ""));
    }
  }
}
