using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace better_logcat
{
	// TODO: Fix namespace, clean up stuff that doesn't belong in this class

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableRangeCollection<LogEntry> logEntries { get; set; }
		private IParser _logParser = new LogcatParser();

		public bool AutoScrollEnabled
		{
			get { return (bool)GetValue(AutoScrollEnabledProperty); }
			set { SetValue(AutoScrollEnabledProperty, value); }
		}


		// TODO: UserPrefs
		public static readonly DependencyProperty AutoScrollEnabledProperty = DependencyProperty.Register("AutoScrollEnabled", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(true));
		private int BUFFER_COUNT = 10000;


		public MainWindow()
		{
			InitializeComponent();

			// TODO: Don't have this running all the time, enable/disable using the event provided by checkbox
			_autoScrollThread = new Thread(() => Timer(500, () =>
			{
				Dispatcher.InvokeAsync(ScrollToBottom);
			}));
			_autoScrollThread.IsBackground = true;
			_autoScrollThread.Start();
			
			logListView.ItemsSource = logEntries = new ObservableRangeCollection<LogEntry>();

			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(logListView.ItemsSource);
			view.Filter = Filter;
			
			BindingOperations.EnableCollectionSynchronization(logEntries, logEntries);

			ADBProcessWrapper.OnOutputReceived += OnOutputReceived;
			ADBProcessWrapper.Instance.StartMainProcess();


			this.Closed += MainWindow_Closed;
		}

		// TODO: Expand filter logic
		private bool Filter(object item)
		{
			string text = logSearchTextBox.Text;

			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			else
			{
				LogEntry log = item as LogEntry;

				return log.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
					log.Pid.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0 ||
					log.Tag.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			ADBProcessWrapper.Instance.Stop();
		}

		private void OnOutputReceived(string s)
		{
			LogEntry entry;
			if (_logParser.TryParse(s, out entry))
			{
				AddEntry(entry);

				// TODO: Is this even necessary?
				int updateRate = Clamp(_dynamicUpdateRate * _dynamicUpdateRateMultiplier, UPDATE_RATE_MIN, UPDATE_RATE_MAX);

				long ticks = DateTime.UtcNow.Ticks;
				if (ticksRequiredForNextInvoke < ticks || _queuedEntries.Count > 5) // TODO: UserPrefs
				{
					ticksRequiredForNextInvoke = ticks + updateRate;
					UpdateEntries();
				}
			}
		}

		private int Clamp(int val, int lower, int upper)
		{
			val = Math.Max(val, lower);
			val = Math.Min(val, upper);
			return val;
		}

		private Queue<LogEntry> _queuedEntries = new Queue<LogEntry>();

		private void AddEntry(LogEntry entry)
		{
			lock (_queuedEntries)
			{
				_queuedEntries.Enqueue(entry);
				_dynamicUpdateRate++;
			}
		}

		private void UpdateEntries()
		{
			_dynamicUpdateRate = 0;

			int tooManyEntriesCount = 0;
			lock (_queuedEntries)
			{
				lock (logEntries)
				{
					var tmp = logEntries;

					// Set to null so we don't invoke any OnChanged events while messing with the collection
					logEntries = null;

					tmp.AddRange(_queuedEntries);
					tooManyEntriesCount = tmp.Count - BUFFER_COUNT;
					if(tooManyEntriesCount > 0)
					{
						tmp.RemoveAtRange(0, tooManyEntriesCount);
					}
					logEntries = tmp;
				}
				_queuedEntries.Clear();
			}



			if (!_queuedScroll)
			{
				_queuedScroll = true;
				Dispatcher.InvokeAsync(ScrollToBottom);
			}
		}

		private bool _queuedScroll = false;

		private void ScrollToBottom()
		{
			if (AutoScrollEnabled && logListView.Items.CurrentPosition != logListView.Items.Count - 1)
			{
				logListView.Items.MoveCurrentToLast();
				logListView.ScrollIntoView(logListView.Items.CurrentItem);
			}
			_queuedScroll = false;
		}

		private Thread _autoScrollThread;

		private void Timer(int millisecondsTimeout, Action action)
		{
			while (true)
			{
				Thread.Sleep(millisecondsTimeout);
				action();
			}
		}
	

		private long ticksRequiredForNextInvoke;

		// TODO: UserPrefs
		private const int UPDATE_RATE_MIN = 10; //100000;			// 0.01s
		private const int UPDATE_RATE_MAX = 10; //2000000;		// 0.2s
		private int _dynamicUpdateRate = 0;
		private int _dynamicUpdateRateMultiplier = 10000;	// 0.001s
		
		private object _lastNotNullSelectedItem = null;

		private void logSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			CollectionViewSource.GetDefaultView(logListView.ItemsSource).Refresh();

			if(_lastNotNullSelectedItem != null)
			{
				logListView.SelectedItem = _lastNotNullSelectedItem;
				logListView.ScrollIntoView(_lastNotNullSelectedItem);
			}
		}

		private void logListView_MouseWheel(object sender, MouseWheelEventArgs e)
		{
		}

		private void logListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			// TODO: This stuff only works with the mouse wheel, not when dragging the scrollbar
			AutoScrollEnabled = false;

			if (!AutoScrollEnabled)
			{
				if (e.Delta < 0 && IsFullyOrPartiallyVisible((ListViewItem)logListView.ItemContainerGenerator.ContainerFromIndex(logListView.Items.Count - 1), logListView)) // == logListView.Items.Count - 1)
				{
					AutoScrollEnabled = true;
				}
			}

		}


		// TODO: This seems like a nicer way to handle scrolling to the bottom, but it doesn't seem like the event is available any longer
		//private void logListViewScrollBar_scroll(object sender, ScrollEventArgs e)
		//{
		//	if(AutoScrollEnabled)
		//	{
		//		return;
		//	}

		//	ScrollBar sb = e.OriginalSource as ScrollBar;

		//	if(sb.Orientation == Orientation.Horizontal)
		//	{
		//		return;
		//	}

		//	if(sb.Value == sb.Maximum)
		//	{
		//		AutoScrollEnabled = true;
		//	}
		//}

		private void logListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Console.WriteLine("selectionchanged");
			var newItem = logListView.SelectedItem;
			if(newItem != null)
			{
				_lastNotNullSelectedItem = newItem;
			}
		}


		// http://munnaondotnet.blogspot.dk/2011/09/is-item-is-visible-in-scroll-viewer.html
		private bool IsFullyOrPartiallyVisible(FrameworkElement child, FrameworkElement scrollViewer)
		{
			if(child == null)
			{
				return false;
			}

			var childTransform = child.TransformToAncestor(scrollViewer);
			var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
			var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
			return ownerRectangle.IntersectsWith(childRectangle);
		}
	}
}
