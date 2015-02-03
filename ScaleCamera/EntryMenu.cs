
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
	[Activity (Label = "ScaleCamera", MainLauncher = true)]
	public class EntryMenu : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Menu);
			//

			Button startBtn = FindViewById<Button>(Resource.Id.startBtn);	
			Button instrBtn = FindViewById<Button>(Resource.Id.instrBtn);	
			Button creditsBtn = FindViewById<Button>(Resource.Id.creditsBtn);

			startBtn.Click += delegate(object sender, EventArgs e){
				StartActivity(typeof(ScaleCamera));
			};
			instrBtn.Click += delegate(object sender, EventArgs e){
				StartActivity(typeof(InstrScreen));
			};
			creditsBtn.Click += delegate(object sender, EventArgs e){
				StartActivity (typeof(CreditsScreen));
			};
		}
	}
}

