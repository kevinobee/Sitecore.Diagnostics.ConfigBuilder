namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Xml;
  using Sitecore.Diagnostics;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes;

  internal class AutoIncludeFilesLoader
  {
    internal static void LoadAutoIncludeFiles(XmlNode element, PathMapper mapper, IEnumerable<string> includeFoldersPaths)
    {
      Assert.ArgumentNotNull(element, "element");
      Assert.ArgumentNotNull(mapper, "mapper");
      Assert.ArgumentNotNull(includeFoldersPaths, "includeFoldersPaths");

      var patcher = new ConfigPatcher(element);
      foreach (var includeFolderPath in includeFoldersPaths)
      {
        LoadAutoIncludeFiles(patcher, mapper.MapPath(includeFolderPath));
      }
    }

    private static void LoadAutoIncludeFiles(ConfigPatcher patcher, string folder)
    {
      try
      {
        if (Directory.Exists(folder))
        {
          foreach (string str in Directory.GetFiles(folder, "*.config"))
          {
            try
            {
              if ((File.GetAttributes(str) & FileAttributes.Hidden) == 0)
              {
                patcher.ApplyPatch(str);
              }
            }
            catch (Exception exception)
            {
              ConfigBuilderLog.Error(string.Concat(new object[] { "Could not load configuration file: ", str, ": ", exception }));
            }
          }
          foreach (string str2 in Directory.GetDirectories(folder))
          {
            try
            {
              if ((File.GetAttributes(str2) & FileAttributes.Hidden) == 0)
              {
                LoadAutoIncludeFiles(patcher, str2);
              }
            }
            catch (Exception exception2)
            {
              ConfigBuilderLog.Error(string.Concat(new object[] { "Could not scan configuration folder ", str2, " for files: ", exception2 }));
            }
          }
        }
      }
      catch (Exception exception3)
      {
        ConfigBuilderLog.Error(string.Concat(new object[] { "Could not scan configuration folder ", folder, " for files: ", exception3 }));
      }
    }



  }
}
