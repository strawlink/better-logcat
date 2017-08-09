using System;

namespace better_logcat
{
	public class LogcatParser : IParser
	{
		private const int TIMESTAMP_LENGTH = 18;
		private const int LOG_LEVEL_POSITION = 31;

		private const int PID_START_POSITION = 19;
		private const int TID_START_POSITION = 25;
		private const int PID_TID_LENGTH = 5;

		private const int SEVERITY_POSITION = 31;
		private const int TAG_START_POSITION = 33;

		private const char TAG_TEXT_SEPARATOR_CHAR = ':';
		
		// -v threadtime structure example:
		//  06-17 11:46:09.905  2103  4205 I chatty  : uid=1000(system) Binder_4 expire 5 lines
		//  |________________| |___| |___| | |______|  |______________________________________|
		//          |            |     |   |     |                      |
		//      Timestamp       PID   TID Sev.  Tag                    Text
		//        0-18        19-23  25-29 31   33-:                  : -end

		public bool TryParse(string rawText, out LogEntry entry)
		{
			if (string.IsNullOrEmpty(rawText))
			{
				entry = new LogEntry
				{
					//ParsingErrorType = ParsingError.EmptyRawText
				};
				return false;
			}

			try
			{
				var rawTimestamp = rawText.Substring(0, TIMESTAMP_LENGTH);
				var timestamp = DateTime.ParseExact(rawTimestamp, "MM-dd HH:mm:ss.fff", null);
				var logLevel = rawText[LOG_LEVEL_POSITION].CharToLogLevel();

				var parsedPid = int.Parse(rawText.Substring(PID_START_POSITION, PID_TID_LENGTH).Trim());
				var parsedTid = int.Parse(rawText.Substring(TID_START_POSITION, PID_TID_LENGTH).Trim());
				var tagEndPosition = rawText.IndexOf(TAG_TEXT_SEPARATOR_CHAR, SEVERITY_POSITION);
				if (tagEndPosition < TAG_START_POSITION)
				{
					throw new Exception("Unable to determine PID");
				}

				var tag = rawText.Substring(TAG_START_POSITION, tagEndPosition - TAG_START_POSITION);
				var text = rawText.Substring(tagEndPosition + 2);

//				var tag = actualLog.Substring(0, pidStartPosition - 1).Trim();
//				var text = actualLog.Substring(pidEndPosition + PID_SEPARATOR_END.Length);

				entry = new LogEntry
				{
					Level = logLevel,
					//RawText = rawText,
					Timestamp = timestamp,
					//Severity = logLevel,
					Pid = parsedPid,
					Tag = tag,
					Text = text,
					//ParsingErrorType = ParsingError.None
				};
				return true;
			}
			catch (Exception e)
			{
				entry = new LogEntry
				{
					//RawText = rawText,
					//ParsingErrorType = ParsingError.FailedParsing,
					//ParsingErrorMessage = e.Message
				};
				return false;
			}
		}
	}
}