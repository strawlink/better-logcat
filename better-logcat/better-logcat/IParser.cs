namespace better_logcat
{
	public interface IParser
	{
		bool TryParse(string rawText, out LogEntry entry);
	}
}