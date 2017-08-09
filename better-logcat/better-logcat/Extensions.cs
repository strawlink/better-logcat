namespace better_logcat
{
	public static class Extensions
	{
		public static string ToContentString(this LogEntry entry)
		{
			return $"Time: {entry.Timestamp:MM-dd HH:mm:ss.fff} | {LogLevelToChar(entry.Level)} | PID: {entry.Pid} | Tag: {entry.Tag} | Text: {entry.Text}"; // | RawText {entry.RawText}";
		}

		public static LogLevel CharToLogLevel(this char c)
		{
			switch (c)
			{
				case 'V':
					return LogLevel.V;
				case 'D':
					return LogLevel.D;
				case 'I':
					return LogLevel.I;
				case 'W':
					return LogLevel.W;
				case 'E':
					return LogLevel.E;
				default:
					return LogLevel.U;
			}
		}

		public static char LogLevelToChar(this LogLevel level)
		{
			switch (level)
			{
				case LogLevel.V:
					return 'V';
				case LogLevel.D:
					return 'D';
				case LogLevel.I:
					return 'I';
				case LogLevel.W:
					return 'W';
				case LogLevel.E:
					return 'E';
				default:
					return 'U';
			}
		}
	}
}