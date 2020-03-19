using System;
using System.Threading;

namespace SnapTest
{
	class Program
	{
		static void Main(string[] args)
		{
			App.Write = true;

			var testRun = new TestRun();

			App app = App.Start(testRun.Program);
			Thread.Sleep(1000);

			foreach (object action in testRun.Schedule)
			{
				if (action is MouseClickAction mouseClick)
				{
					app.Click(mouseClick.X, mouseClick.Y);
					Console.WriteLine("X: " + mouseClick.X + ", Y: " + mouseClick.Y);
				}
				else if (action is Snapshot snapshot)
				{
					app.WaitSnapshot(snapshot.Name, snapshot.SkipTop, snapshot.TimeoutMilli);
				}
				else
				{
					throw new ArgumentException();
				}
			}
			
			app.Kill();
		}
	}
}
