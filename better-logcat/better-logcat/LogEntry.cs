using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace better_logcat
{
	public enum LogLevel
	{
		// TODO: Rename this to full names
		U,
		V,
		D,
		I,
		W,
		E,
	}

	public enum ParsingError
	{
		None,
		EmptyRawText,
		FailedParsing
	}

	public static class Global
	{

		// TODO: UserPrefs
		public static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Colors.LightGreen);
		public static readonly SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
		public static readonly SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);
		public static readonly SolidColorBrush GrayBrush = new SolidColorBrush(Colors.Gray);
		public static readonly SolidColorBrush DarkGrayBrush = new SolidColorBrush(Colors.DarkGray);
		public static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
		public static readonly SolidColorBrush BlueBrush = new SolidColorBrush(Colors.Blue);
	}

	public class LogEntry
	{
		public SolidColorBrush Color
		{
			get
			{
				switch (Level)
				{
					case LogLevel.V:
						return Global.GrayBrush;
					case LogLevel.D:
						return Global.DarkGrayBrush;
					case LogLevel.I:
						return Global.GreenBrush;
					case LogLevel.W:
						return Global.OrangeBrush;
					case LogLevel.E:
						return Global.RedBrush;
					default:
						return Global.WhiteBrush;
				}
			}
		}
		public DateTime Timestamp { get; set; }
		public LogLevel Level { get; set; }
		public int Pid { get; set; }
		//public int Tid;
		public string Tag { get; set; }
		public string Text { get; set; }
		public string RawText { get; set; }

		public LogEntry()
		{

		}
	}
}
