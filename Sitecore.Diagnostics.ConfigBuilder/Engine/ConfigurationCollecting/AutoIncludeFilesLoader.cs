namespace Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.IO.Abstractions;

  using System.Xml;
  using Sitecore.Diagnostics.Base;
  using Sitecore.Diagnostics.Base.Annotations;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.Common;
  using Sitecore.Diagnostics.ConfigBuilder.Engine.ConfigurationCollecting.AutoIncludes;

  internal class AutoIncludeFilesLoader
  {
    private IFileSystem FileSystem;

    internal AutoIncludeFilesLoader(IFileSystem fileSystem)
    {
      Assert.ArgumentNotNull(fileSystem, "fileSystem");

      this.FileSystem = fileSystem;
    }

    internal void LoadAutoIncludeFiles([NotNull] XmlNode element, [NotNull] PathMapper mapper, [NotNull] IEnumerable<string> includeFoldersPaths)
    {
      Assert.ArgumentNotNull(element, "element");
      Assert.ArgumentNotNull(mapper, "mapper");
      Assert.ArgumentNotNull(includeFoldersPaths, "includeFoldersPaths");

      var patcher = new ConfigPatcher(this.FileSystem, element);
      foreach (var includeFolderPath in includeFoldersPaths)
      {
        this.LoadAutoIncludeFiles(patcher, mapper.MapPath(includeFolderPath));
      }
    }

    private void LoadAutoIncludeFiles([NotNull] ConfigPatcher patcher, [NotNull] string folder)
    {
      Assert.ArgumentNotNull(patcher, "patcher");
      Assert.ArgumentNotNull(folder, "folder");

      try
      {
        if (!this.FileSystem.Directory.Exists(folder))
        {
          return;
        }

        foreach (var str in this.FileSystem.Directory.GetFiles(folder, "*.config"))
        {
          try
          {
            if ((this.FileSystem.File.GetAttributes(str) & FileAttributes.Hidden) == 0)
            {
              patcher.ApplyPatch(str);
            }
          }
          catch (Exception exception)
          {
            ConfigBuilderLog.Error("Could not load configuration file: " + str + ": " + exception);
          }
        }

        foreach (string str2 in this.FileSystem.Directory.GetDirectories(folder))
        {
          try
          {
            if ((this.FileSystem.File.GetAttributes(str2) & FileAttributes.Hidden) == 0)
            {
              this.LoadAutoIncludeFiles(patcher, str2);
            }
          }
          catch (Exception exception2)
          {
            ConfigBuilderLog.Error("Could not scan configuration folder " + str2 + " for files: " + exception2);
          }
        }
      }
      catch (Exception exception3)
      {
        ConfigBuilderLog.Error("Could not scan configuration folder " + folder + " for files: " + exception3);
      }
    }
  }
}
