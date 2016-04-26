namespace Sitecore.Diagnostics.ConfigBuilder.Engine.Common
{
  using System;
  using Sitecore.Diagnostics.Base.Annotations;

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
