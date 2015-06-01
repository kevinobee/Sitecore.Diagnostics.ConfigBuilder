namespace Sitecore.Diagnostics.ConfigBuilder.Tests
{
  using System.IO;

  public static class Extensions
  {
    public static string FormatWith(this string str, params object[] p)
    {
      return string.Format(str, p);
    }

    public static string ReadText(this FileInfo file)
    {
      return File.ReadAllText(file.FullName);
    }
  }
}