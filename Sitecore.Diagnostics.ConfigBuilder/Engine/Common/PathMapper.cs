namespace Sitecore.Diagnostics.ConfigBuilder.Engine.Common
{
  using System;
  using System.IO;

  internal class PathMapper
  {
    protected string BasePath { get; set; }

    internal PathMapper(string basePath)
    {
      this.BasePath = basePath;
    }

    internal string MapPath(string path)
    {
      if ((path.Length != 0) && (path.IndexOf("://", StringComparison.InvariantCulture) < 0))
      {
        var trimmedPath = path.TrimStart(new []{'~'}).TrimStart(new[] { '.' });
        int num = trimmedPath.IndexOfAny(new[] { '\\', '/' });
        if ((num >= 0) && (trimmedPath[num] == '\\'))
        {
          return trimmedPath.Replace('/', '\\');
        }
        if (trimmedPath[0] == '/')
        {
          return Path.Combine(this.BasePath, trimmedPath.Substring(1).Replace('/', '\\'));  
        }

      }
      return path;
    }
  }
}
