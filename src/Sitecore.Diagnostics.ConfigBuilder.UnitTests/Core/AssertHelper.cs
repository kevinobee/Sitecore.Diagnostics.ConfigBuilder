namespace Sitecore.Diagnostics.ConfigBuilder.UnitTests.Core
{
  using System;
  using System.IO;
  using System.Xml;

  public static class AssertHelper
  {
    public static void AreEqual(string expected, string actual)
    {
      Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, actual);
    }

    public static void FilesAreEqual(string expectedFilePath, string actualFilePath)
    {
      var expectedText = File.ReadAllText(expectedFilePath);
      var actualText = File.ReadAllText(actualFilePath);
      if (!string.Equals(expectedText, actualText))
      {
        var folderPath = GetFolderPath(expectedFilePath, actualFilePath);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(string.Format(
          @"Files {{0}} and {{1}} are different, open folder to compare them: 
{{2}}"), Path.GetFileName(expectedFilePath), Path.GetFileName(actualFilePath), folderPath);
      }
    }

    public static void XmlFilesAreEqual(string expectedFilePath, string actualFilePath)
    {
      var expectedText = CleanXml(File.ReadAllText(expectedFilePath));
      File.WriteAllText(expectedFilePath, expectedText);
      var actualText = CleanXml(File.ReadAllText(actualFilePath));
      File.WriteAllText(actualFilePath, actualText);
      if (!string.Equals(expectedText, actualText))
      {
        var folderPath = GetFolderPath(expectedFilePath, actualFilePath);

        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(string.Format(
          @"Files {{0}} and {{1}} are different, open folder to compare them: 
{{2}}"), Path.GetFileName(expectedFilePath), Path.GetFileName(actualFilePath), folderPath);
      }
    }

    private static string CleanXml(string xml)
    {
      var doc = new XmlDocument();
      doc.LoadXml(xml);
      using (var strWriter = new StringWriter())
      {
        using (var xmlWriter = new XmlTextWriter(strWriter)
          {
            Formatting = Formatting.Indented,
            Indentation = 2,
            IndentChar = ' '
          })
        {
          doc.Save(xmlWriter);
        }

        xml = strWriter.ToString();
        while (xml.Contains("  "))
        {
          xml = xml.Replace("  ", " ");
        }

        return xml;
      }
    }

    private static string GetFolderPath(string expectedFilePath, string actualFilePath)
    {
      var expectedFolderPath = Path.GetDirectoryName(expectedFilePath);
      var actualFolderPath = Path.GetDirectoryName(actualFilePath);
      if (string.Equals(expectedFolderPath, actualFolderPath, StringComparison.OrdinalIgnoreCase))
      {
        return expectedFolderPath;
      }

      var tmp = "dir." + Path.GetTempFileName();
      Directory.CreateDirectory(tmp);
      File.Copy(expectedFilePath, Path.Combine(tmp, Path.GetFileName(expectedFilePath)));
      File.Copy(actualFilePath, Path.Combine(tmp, Path.GetFileName(actualFilePath)));
      return tmp;
    }
  }
}