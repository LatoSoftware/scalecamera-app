//Originally from:
//http://www.dreamincode.net/forums/topic/190013-creating-simple-file-chooser/
using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace ScaleCamera
{
	[Activity (Label = "Open File...", MainLauncher = false, Theme = "@android:style/Theme.Dialog")]
	public class OpenFileDialog : Activity
	{
		private DirectoryInfo currentDir;
		private Stack<DirectoryInfo> dirStack = new Stack<DirectoryInfo> ();
		private FileArrayAdapter currAdapter;
		private ListView aListView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.OpenFileDialog_l);

			aListView = FindViewById<ListView> (Resource.Id.list);
			aListView.ItemClick += onItemClicked;

			string state = Android.OS.Environment.ExternalStorageState;
			if (Android.OS.Environment.MediaMountedReadOnly.Equals (state) || Android.OS.Environment.MediaMounted.Equals (state)) {//////////////////////////////////http://developer.android.com/reference/android/os/Environment.html#getExternalStorageDirectory%28%29
				//currentDir = new DirectoryInfo ("/sdcard/DCIM/Camera/");
				currentDir = new DirectoryInfo (Android.OS.Environment.ExternalStorageDirectory + @"/DCIM/Camera/");
				//SetTitle(//CHAR SEQUENCE!!!!! currentDir.Name)
				dirStack.Push (new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath.ToString()));
				dirStack.Push (currentDir.Parent.Parent);
               	dirStack.Push (currentDir.Parent);
				try {
					currAdapter = fill (currentDir);
				} catch (UnauthorizedAccessException ex) {
					Console.WriteLine ("ex " + ex.Message);
					Console.WriteLine ("Try turning on USB Mass Storage only, disconnecting and reconnecting usb, and then turning on USB Debugging mode.");
				}
			} else {
				Console.WriteLine ("Media is not mounted.");
			}
			aListView.Adapter = currAdapter;
		}

		private void onItemClicked (object sender, AdapterView.ItemClickEventArgs e)
		{
			Option o = currAdapter.GetItemAtPosition (e.Position); //may require casting

			if (string.Compare ("Folder", o.getData ()) == 0) {
				dirStack.Push (currentDir);
				currentDir = new DirectoryInfo (o.getPath ());
				currAdapter = fill (currentDir);
				aListView.Adapter = currAdapter;// // //  // Instance of FILEARRAYADAPTER/////////////////////////////////////////////////////////////
				//Console.WriteLine("DIRECTORY");
			} else if (string.Compare ("Parent Directory", o.getData ()) == 0) {
				//Console.WriteLine (dirStack.Count);
				if (dirStack.Count > 0) {
					currentDir = dirStack.Pop ();
					currAdapter = fill (currentDir);
					aListView.Adapter = currAdapter;
				} else {
					throw new NotImplementedException("Move up from highest directory");
				}
			} else {
				onFileClick (o);
			}
		}

		private FileArrayAdapter fill (DirectoryInfo dirInfo)
		{
			List<Option> dir = new List<Option> ();
			List<Option> fls = new List<Option> ();

			foreach (DirectoryInfo subDir in dirInfo.GetDirectories()) {
				dir.Add (new Option (subDir.Name, "Folder", subDir.FullName));
			}
			foreach (FileInfo file in dirInfo.GetFiles()) {
				fls.Add (new Option (
					file.Name,
					"File Size: " + file.Length,
					file.FullName
				));
			}
//			dir.Sort();
//			fls.Sort();
			dir.AddRange (fls);
			if (string.Compare (dirInfo.Name, "sdcard", false) != 0) { //NOT the same
				dir.Insert (
					0,
					new Option ("..", "Parent Directory", dirInfo.Parent.Name)
				);
			}
			FileArrayAdapter adapter = new FileArrayAdapter (
					this,
					Resource.Layout.ListItem,
					dir
			);
			return adapter; // // //  // Instance of FILEARRAYADAPTER
		}

		private void onFileClick (Option o)
		{
			Toast.MakeText (this, "File Clicked: " + o.getName (), ToastLength.Short).Show ();
			string result = o.getPath ();
			// Prepare data intent
			Intent databackIntent = new Intent ();
			databackIntent.PutExtra ("FILE_PATH", result);
			SetResult (Result.Ok, databackIntent);
			Finish ();
		}
	}

	public class FileArrayAdapter : Android.Widget.ArrayAdapter<Option>
	{
		private Context c;
		private int id;
		private List<Option> items;

		public FileArrayAdapter (Context context, int textViewResourceId, List<Option> objects) : base(context, textViewResourceId, objects)
		{
			c = context;
			id = textViewResourceId;
			items = objects; //List<Option>
		}

		public Option GetItemAtPosition (int position)
		{
			return items [position];
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			//Each row contains data from an option in items, which has been set in the constructor.
			View v = convertView;
			if (v == null) { //if v doesn't exist yet
				LayoutInflater vi = (LayoutInflater)c.GetSystemService (Context.LayoutInflaterService);
				v = vi.Inflate (id, null);
			}
			Option o = items [position]; //Get a folder or file
			if (o != null) {
				TextView t1 = (TextView)v.FindViewById (Resource.Id.LTextView);
				//TextView t2 = (TextView)v.FindViewById (Resource.Id.TextView02);

				if (t1 != null) {
					t1.Text = o.getName ();
				}
//				if (t2 != null) { //Commented because we don't need info on the file size
//					t2.Text = o.getData ();
//				} 
			}
			return v;
		}
	}

	public class Option
	{
		private string name;
		private string data;
		private string path;

		public Option (string n, string d, string p)
		{
			name = n;
			data = d;
			path = p;
		}

		public string getName ()
		{
			return name;
		}

		public String getData ()
		{
			return data;
		}

		public String getPath ()
		{
			return path;
		}
	}
}

