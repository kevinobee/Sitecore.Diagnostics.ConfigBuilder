namespace Sitecore.Diagnostics.ConfigBuilder.Engine.Common
{
  using System;
  using Sitecore.Diagnostics.Annotations;

  public static class ConfigBuilderLog
  {
    [PublicAPI]
    public static event Action<string> OnError;

    internal static void Error(string message)
    {
      if (OnError != null)
      {
        OnError(message);
      }
    }
  }
}
