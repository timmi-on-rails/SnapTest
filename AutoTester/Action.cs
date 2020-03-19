using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester
{
	public interface IAction
	{
		Task Execute(IntPtr windowHandle);
	}
}
