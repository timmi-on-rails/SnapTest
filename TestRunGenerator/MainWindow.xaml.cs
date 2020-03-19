using SnapTest;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace TestRunGenerator
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public TestRun TestRun { get; set; }

		SnapTest.App app;

		public MainWindow()
		{
			InitializeComponent();

			DataContext = this;

			SnapTest.App.Write = true;

			TestRun = new TestRun();
			TestRun.Schedule = new System.Collections.ObjectModel.ObservableCollection<object>();

			app = SnapTest.App.Start(@"C:\Users\Tom\Desktop\dev-snapshot\InsulogixT3.exe");
			/*foreach (object action in testRun.Schedule)
			{
				if (action is MouseClick mouseClick)
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

			app.Kill();*/
		}

		private void Snapshot_Click(object sender, RoutedEventArgs e)
		{
			Bitmap bmp = app.WaitSnapshot(name.Text, 50, 1000);
			TestRun.Schedule.Add(new Snapshot
			{
				Name = name.Text,
				SkipTop = 50,
				TimeoutMilli = 1000
			});

			using (MemoryStream memory = new MemoryStream())
			{
				bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
				memory.Position = 0;
				BitmapImage bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();

				image.Source = bitmapimage;
			}
		}

		int x, y;

		private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			x = (int)e.GetPosition(image).X;
			y = (int)e.GetPosition(image).Y;
			ellipse.Margin = new Thickness(x - 0.5 * ellipse.Width, y - 0.5 * ellipse.Height, 0, 0);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TestRun));
			//xmlSerializer.Serialize()
			app.Kill();
		}

		private void GenMouseClick(object sender, RoutedEventArgs e)
		{
			TestRun.Schedule.Add(new MouseClickAction { X = x, Y = y + (int)(50 / 1.25) });
			app.Click(x, y + (int)(50 / 1.25));

		}
	}
}
