using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester
{
	public class Model
	{
		public string Program { get; set; }

		public int LaunchTimeout { get; set; } = 1000;

		public ObservableCollection<IAction> Actions { get; set; } = new ObservableCollection<IAction>();
	}
}
