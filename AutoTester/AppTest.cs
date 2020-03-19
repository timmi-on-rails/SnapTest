using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTester
{
	public static class AppTest
	{
		public static async Task Run(Model model, bool interactive)
		{
			Process process = await StartAsync(model.Program, model.LaunchTimeout);

			foreach(IAction action in model.Actions)
			{
				await action.Execute(process.MainWindowHandle);
			}

			process.CloseMainWindow();
		}

		public static async Task<Process> StartAsync(string fileName, int timeout)
		{
			Process process = Process.Start(fileName);

			CancellationToken cancellationToken = new CancellationToken();
			Task task = Task.Run(() =>
			{
				while (process.MainWindowHandle == IntPtr.Zero)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Thread.Sleep(10);
				}

				process.WaitForInputIdle();
			});

			Task readyTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationToken));

			if (readyTask == task)
			{
				// We re-await the task so that any exceptions/cancellation is rethrown.
				await task;

				return process;
			}
			else
			{
				throw new TestException(TestError.LaunchTimeout);
			}
		}
	}
}
