using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester
{
	enum TestError
	{
		LaunchTimeout,
		ForegroundFailed,
		ScreenshotMismatch
	};

	class TestException : Exception
	{
		public TestException(TestError testError) : base(testError.ToString()) { }
	}
}
