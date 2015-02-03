
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ScaleCamera
{
	[Activity (Label = "Credits")]			
	public class CreditsScreen : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Credits);
			//Button backFromCreditsBtn = FindViewById<Button>(Resource.Id.backFromCreditsBtn);
			//backFromCreditsBtn.Click += delegate(object sender, EventArgs e)
			//{
			//	this.Finish ();
			//};
		}
	}
}

