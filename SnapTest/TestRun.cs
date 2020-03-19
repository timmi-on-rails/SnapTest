using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SnapTest
{
	public class MouseClickAction
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public class Snapshot
	{
		public string Name { get; set; }
		public int TimeoutMilli { get; set; }
		public int SkipTop { get; set; }
	}

	public class TestRun
	{
		public string Program { get; set; } = @"..\InsulogixT3.exe";
		public string WorkDir { get; set; } = "..";

		public ObservableCollection<object> Schedule { get; set; } = new ObservableCollection<object>
		{
			new Snapshot { Name = @"overview", TimeoutMilli = 1000, SkipTop = 50 },
			new MouseClickAction { X = 10, Y = 10 },
			new Snapshot { Name = @"main", TimeoutMilli = 1000, SkipTop = 50 },
			new MouseClickAction { X = 600, Y = 300 },
			new Snapshot { Name = @"about", TimeoutMilli = 1000, SkipTop = 50 }
		};
	}
}
