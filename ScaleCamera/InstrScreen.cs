
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
	[Activity (Label = "Instructions")]			
	public class InstrScreen : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Credits);
			//Button backFromCreditsBtn = FindViewById<Button>(Resource.Id.backFromCreditsBtn);
			TextView title = FindViewById<TextView>(Resource.Id.creditsTitle);
			TextView content = FindViewById<TextView>(Resource.Id.creditsContent);
			title.SetText(Resource.String.instr_title);
			content.SetText (Resource.String.instr_content);
			//backFromCreditsBtn.Click += delegate(object sender, EventArgs e)
			//{
			//	this.Finish ();
			//};
		}
	}
}

