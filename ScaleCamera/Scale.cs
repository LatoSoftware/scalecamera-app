using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Graphics;
using Android.Graphics.Drawables;

using Android.Util;//includes IAttributeSet

namespace ScaleCamera
{
	[Activity (Label = "HelloM4A", MainLauncher = false, Theme = "@android:style/Theme.Translucent")]
	public class Activity1 : Activity
	{
		private Button computeButton;
		private Button clearButton;
//		private TextView aLabel;
		private CrossHareView aCrossHareView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			
			//UI with Resources
			computeButton = FindViewById<Button> (Resource.Id.computeButton);
			Console.WriteLine ("Here?");
			clearButton = FindViewById<Button> (Resource.Id.clearButton);
			Console.Write ("Or Here?");
//			aLabel = FindViewById<TextView> (Resource.Id.aLabel);
			
			aCrossHareView = FindViewById<CrossHareView> (Resource.Id.aCrossHareView);
			computeButton.Click += OnClick_computeButton;
			clearButton.Click += OnClick_clearButton;
		}

		private void OnClick_computeButton (object sender, System.EventArgs e)
		{
			Console.WriteLine (aCrossHareView.ComputeDist ());
		}

		private void OnClick_clearButton (object sender, System.EventArgs e)
		{
			aCrossHareView.ClearAll ();
		}
	}
	
	public class Point
	{
		public float X;
		public float Y;
//		public Point(int x, int y)
//		{
//			X = x;
//			Y = y;
//		}
		public Point (float x, float y)
		{
			X = x;
			Y = y;
		}

		public float dist (Point p)
		{			
			return (float)Math.Sqrt (Math.Pow (X - p.X, 2) + Math.Pow (Y - p.Y, 2));
		}
	}

	public class CrossHareView : View
	{
		private Path crosshare_path;
		private Paint crosshare_paint;
		private List<Point> location_arr;
		private int ARM_LENGTH;
		private Path ch_to_draw;

		public CrossHareView (Context context, IAttributeSet attrs) : base(context, attrs)///////////////////////////////////////
		{		
			location_arr = new List<Point> ();
			ARM_LENGTH = 10;
			
			//Path
			// (5,5) is in the centre of the crosshare. Each of the four arms is 10 units long.
			crosshare_path = new Path ();
			crosshare_path.MoveTo (0, ARM_LENGTH); //leftmost point
			crosshare_path.RLineTo (2 * ARM_LENGTH, 0); //line to 10 units right of current position.
			crosshare_path.MoveTo (ARM_LENGTH, 0); //topmost
			crosshare_path.RLineTo (0, 2 * ARM_LENGTH);
			
			//Paint
			crosshare_paint = new Paint ();
			crosshare_paint.Color = Color.Black;
			crosshare_paint.StrokeWidth = 2.0F;			
			crosshare_paint.SetStyle (Paint.Style.Stroke);
			
		}

		protected override void OnDraw (Canvas canvas)
		{					
			if (location_arr.Count >= 1) {
				if (location_arr.Count > 2) {					
					Console.WriteLine ("Too many crosshares");
					ClearAll ();
				} else {
					ch_to_draw = new Path ();
					foreach (Point location in location_arr) {
						crosshare_path.Offset (location.X, location.Y, ch_to_draw);//assign result to ch_to_draw
						canvas.DrawPath (ch_to_draw, crosshare_paint);
					}
				}
			}
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			//Console.WriteLine(e.Action);
			if (e.Action == MotionEventActions.Down) {
				Point location = new Point (e.GetX (), e.GetY ());
				location_arr.Add (location);
				this.Invalidate ();
			}
			return true;
		}

		public float ComputeDist ()
		{
			if (location_arr.Count == 2) {
				return location_arr [0].dist (location_arr [1]);
			} else {
				return 0;
			}
		}

		public void ClearAll ()
		{
			location_arr.Clear ();
			this.Invalidate ();
		}
	}
	
	
	
}


