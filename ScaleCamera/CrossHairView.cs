using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;//math module
//http://stackoverflow.com/questions/4324362/detect-touch-press-vs-long-press-vs-movement
namespace ScaleCamera
{
	public class CrossHairView : View
	{
		private const float DIST_THRESHOLD = 100.0F;

		//private static System.TimeSpan TIME_DELAY = new System.TimeSpan((long)0.1);
		private static Path crosshairPath;
		private static Paint crosshairPaint;
		private static int ARM_LENGTH = 10;

		public List<Point> locationArr;
		private List<Distance> distArr;
		private Point location;
//		private System.DateTime upTime; //time of most recent MotionEventsActions.Up event, for deciding on double-click

		public CrossHairView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		public CrossHairView (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}

		private void Initialize ()
		{
			locationArr = new List<Point> ();
			distArr = new List<Distance> ();

			//Path
			//an arrow with tip at (0,0)
			crosshairPath = new Path ();
			crosshairPath.MoveTo(0,0);
			crosshairPath.RLineTo (ARM_LENGTH, 0);
			crosshairPath.MoveTo (0,0);			
			crosshairPath.RLineTo (0,ARM_LENGTH);
			crosshairPath.MoveTo (0,0);	
			crosshairPath.RLineTo (ARM_LENGTH*2, ARM_LENGTH*2);

			//Path
			// (5,5) is in the centre of the crosshair. 
//			// Each of the four arms is ARM_LENGTH units long.
//			crosshairPath = new Path ();
//			crosshairPath.MoveTo (0, ARM_LENGTH); //leftmost point
//			crosshairPath.RLineTo (2 * ARM_LENGTH, 0); 
//			crosshairPath.MoveTo (ARM_LENGTH, 0); //topmost
//			crosshairPath.RLineTo (0, 2 * ARM_LENGTH);

			//Paint
			crosshairPaint = new Paint ();
			crosshairPaint.Color = Color.White;
			crosshairPaint.StrokeWidth = 2.0F;
			crosshairPaint.SetStyle (Paint.Style.Stroke);
		}

		protected override void OnDraw (Canvas canvas)
		{
			Path crosshairToDraw = new Path ();
			foreach (Point location in locationArr) {
//				crosshairPath.Offset (location.X-ARM_LENGTH, location.Y-ARM_LENGTH, crosshairToDraw);//assign result to 
				crosshairPath.Offset (location.X,location.Y,crosshairToDraw);
				canvas.DrawPath (crosshairToDraw, crosshairPaint);
			}
		}

		public override bool OnTouchEvent (MotionEvent e)//////////////////////////////////////
		{
			switch (e.Action)
			{
				case(MotionEventActions.Down):
//					if (System.DateTime.Now - upTime < TIME_DELAY)
//					{
						location = new Point (e.GetX (), e.GetY ());
						locationArr.Add (location);
						
//					} 
//					else
//					{
						
//						if (locationArr.Count > 2) {
//							locationArr.Clear ();
//						}						
//					}
					this.Invalidate ();
					break;
				case(MotionEventActions.Move):
//					if ((Math.Abs (e.GetX () - location.X) > DIST_THRESHOLD) || (Math.Abs(e.GetY () -location.Y) > DIST_THRESHOLD)) //reasonable movement
//					{
						locationArr.Remove(location);	
						location = new Point (e.GetX (), e.GetY ());
						locationArr.Add (location);
						if (locationArr.Count > 2) {
							locationArr.Clear ();
						}						
						this.Invalidate ();
//					}
					break;
				case(MotionEventActions.Up):
//					upTime = stopwatch.
					locationArr.Remove(location);	
					location = new Point (e.GetX (), e.GetY ());
					locationArr.Add (location);
					if (locationArr.Count > 2) {
						locationArr.Clear ();
					}
					this.Invalidate ();
					break;
			}
			return true;
		}
		
		public Distance getDist()
		{
			Distance d = locationArr [0].dist (locationArr [1]);
			distArr.Add (d);
			return d;
		}

		public List<Distance> getDistances ()
		{
			return distArr;
		}
	}
}

