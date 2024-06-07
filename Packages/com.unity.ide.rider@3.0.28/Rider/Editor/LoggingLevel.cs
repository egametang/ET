namespace Packages.Rider.Editor
{
  internal enum LoggingLevel
  {
    /// <summary>
    /// Do not use it in logging. Only in config to disable logging.
    /// </summary>
    OFF,
    /// <summary>For errors that lead to application failure</summary>
    FATAL,
    /// <summary>For errors that must be shown in Exception Browser</summary>
    ERROR,
    /// <summary>Suspicious situations but not errors</summary>
    WARN,
    /// <summary>Regular level for important events</summary>
    INFO,
    /// <summary>Additional info for debbuging</summary>
    VERBOSE,
    /// <summary>Methods &amp; callstacks tracing, more than verbose</summary>
    TRACE,
  }
}