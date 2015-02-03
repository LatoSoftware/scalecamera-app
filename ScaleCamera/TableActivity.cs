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

/// <summary>
/// Table activity.
/// </summary>
namespace ScaleCamera
{
	[Activity (Label = "TableActivity")]			
	public class TableActivity : Activity
	{
		private ListView tableListView;		
		private Button emailButton;
		//private Button backButton;
		private string[] tags;
		private string[] data;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Table);
			
			tableListView = FindViewById<ListView>(Resource.Id.tableListView);
			
			data = this.Intent.GetStringArrayExtra("MEASUREMENTS");
			tags = this.Intent.GetStringArrayExtra("TAGS");
			tableListView.Adapter = fill(tags, data);

			
			emailButton = FindViewById<Button> (Resource.Id.emailButton);
			emailButton.Click += OnClick_emailButton;

			//backButton = FindViewById<Button>(Resource.Id.backBtn);
			//backButton.Click += delegate(object sender, EventArgs e) {
			//	this.Finish();
			//};
		}
		private void OnClick_emailButton (object sender, EventArgs e)
		{////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			Intent emailIntent = new Intent(Android.Content.Intent.ActionSend);   
			//string [] [] ans = CreateStrArrsFromData();
			//string[] tagArr = ans[0];
			//string[] dataArr = ans[1];
			//emailIntent.PutStringArrayListExtra ("TAGS",ans[0]);
			//emailIntent.PutStringArrayListExtra ("MEASUREMENTS",ans[1]);
			string resultsStr = CreateStrFromData();
			emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Measurements");  

			emailIntent.SetType ("message/rfc822");
			emailIntent.PutExtra(Android.Content.Intent.ExtraText, resultsStr);  
  
			StartActivity(emailIntent);//Intent.CreateChooser(emailIntent, "Send your results with:"));
		}
		string CreateStrFromData ()
		{
			string resultsStr = "";
			string[] tagArr = data;
			string[] dataArr = tags; //these had better be the same length!
			for (int i = 0; i < tagArr.Length; i++) {
				resultsStr += tagArr[i] + " = " + dataArr[i] + "\n";
			}
			return resultsStr;
		}

		public TableArrayAdapter fill(string[] tagStrArr, string[] dataStrArr)
		{
			List<KeyValuePair<string, string>> obj = new List<KeyValuePair<string, string>>();
			for (int i = 0; i<dataStrArr.Length; i++)
			{
				obj.Add(new KeyValuePair<string, string>(tagStrArr[i], dataStrArr[i]));
				//Console.WriteLine(dataStrArr[i]);
			}			
			return new TableArrayAdapter(this, Resource.Layout.ListItem, obj);
		}
	}
	
	public class TableArrayAdapter : Android.Widget.ArrayAdapter<KeyValuePair<string, string>>
	{
		private Context context;
		private int ListViewId;
		private List<KeyValuePair<string, string>> items;

		public TableArrayAdapter (Context contextIn, int textViewResourceId, List<KeyValuePair<string, string>> objects) : base(contextIn, textViewResourceId, objects)
		{
			context = contextIn;
			ListViewId = textViewResourceId;
			items = objects; // a list of KeyValuePair<string, string>
		}

		public KeyValuePair<string, string> GetItemAtPosition (int position)
		{
			return items [position];
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			//Each row contains data from a KeyValuePair<string, string> in items, which has been set in the constructor.
			View view = convertView;
			if (view == null) { //if view doesn't exist yet
				LayoutInflater inflater = (LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
				view = inflater.Inflate (ListViewId, null);
			}
			KeyValuePair<string, string> kvPair = items [position]; //Get a folder or file
			TextView t1 = (TextView)view.FindViewById (Resource.Id.LTextView);
			TextView t2 = (TextView)view.FindViewById (Resource.Id.RTextView);

			if (t1 != null) {
				t1.Text = kvPair.Key;
			}
			if (t2 != null) { 
				t2.Text = kvPair.Value;
			}
			return view;
		}
	}
}

