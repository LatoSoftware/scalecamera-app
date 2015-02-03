using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Runtime;
using Android.Webkit; //for mimeTypes
//using Android.Provider; //for MediaStore

//TODO Catch end of Video ... make button go back to 'Play'.
//http://docs.xamarin.com/android/recipes/Media/Video/Record_Video
//I've hidden the resultTextView in layout-land, because it is not necessary and not working. :/
//http://docs.xamarin.com/android/recipes/Other_UX/Camera_Intent/Take_a_Picture_and_Save_Using_Camera_App
//http://docs.xamarin.com/android/recipes/Data/Files/Selecting_a_Gallery_Image
//http://docs.xamarin.com/android/recipes/Media/Video/Record_Video
//USE saveInstanceState to save text from resultTextView and put it back AFTER XML HAS LOADED AGAIN - done.
/// <summary>
/// Scale camera.
///  http://www.41post.com/3024/programming/android-loading-and-playing-videos-from-different-sources
/// http://developer.android.com/resources/faq/commontasks.html#opennewscreen
/// http://stackoverflow.com/questions/920306/sending-data-back-to-the-main-activity-in-android
/// http://stackoverflow.com/questions/4111863/cant-set-checkbox-state-in-onpreparedialog-please-help
/// http://stackoverflow.com/questions/2698817/linear-layout-and-weight-in-android //for layout troubles
/// http://mobile.tutsplus.com/tutorials/android/android-sdk-implement-a-share-intent/
/// http://mobile.tutsplus.com/tutorials/android/android-email-intent/ // // VERY USEFUL!!!!!!!!!//
/// http://stackoverflow.com/questions/3515198/share-text-on-facebook-from-android-app-via-action-send
/// </summary>
/*public void seek() {
mediaplayer.start();
mediaplayer.seekTo(mediaplayer.getCurrentPosition()+1);
}

public void onSeekComplete(MediaPlayer mp) {
mediaplayer.pause();
}*/

//CAMERA
//http://developer.android.com/guide/topics/media/camera.html#intents <---------------------------------------------------------DO!
//http://stackoverflow.com/questions/4916159/android-get-thumbnail-of-image-on-sd-card-given-uri-of-original-image
//http://stackoverflow.com/questions/2577221/android-how-to-create-runtime-thumbnail
namespace ScaleCamera
{
	[Activity (Label = "ScaleCamera")]
	public class ScaleCamera : Activity
	{
		private const int SET_SCALE_DIALOG = 123;
		private const int INPUT_TAG_DIALOG = 234;
		private const int OPEN_FILE_ACTIVITY = 456;
		private const int OPEN_IMAGE_ACTIVITY = 135;
		//private const int CAMERA_ACTIVITY = 567;
		//private const int NAME_YOUR_PICTURE_DIALOG = 678;
		//private const int SHOW_TABLE_ACTIVITY = 789;
		//private Java.IO.File _file; //for camera stuff

		#region Views
		private Button playButton;
		private Button loadButton_img;
		private Button loadButton_vid;
		private Button computeButton;
		private Button scaleButton;
		private Button tableButton;
		//private Button cameraButton;
		private TextView resultTextView;
		private CrossHairView aCrossHairView;
		private VideoView vView;
		private ImageView iView;
		private TextView distTextView;
		private EditText distEditText;
		#endregion

		//double currentScreenDist; //ugly code
		private Distance currDist;
		//private string currTag;
		private Android.Net.Uri mediaUri;
		private List<KeyValuePair<string, string>> dataKVPairList = new List<KeyValuePair<string, string>> ();
		
		#region LIFECYCLE HANDLERS
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ScaleCamera6); //new layout

			vView = FindViewById<VideoView> (Resource.Id.vView);
			iView = FindViewById<ImageView> (Resource.Id.iView);
			aCrossHairView = FindViewById<CrossHairView> (Resource.Id.aCrossHairView);
			playButton = FindViewById<Button> (Resource.Id.playButton);
			loadButton_img = FindViewById<Button> (Resource.Id.loadButton_img);
			loadButton_vid = FindViewById<Button> (Resource.Id.loadButton_vid);
			computeButton = FindViewById<Button> (Resource.Id.computeButton);
			scaleButton = FindViewById<Button> (Resource.Id.scaleButton);
			tableButton = FindViewById<Button> (Resource.Id.tableButton);
			//cameraButton = FindViewById<Button> (Resource.Id.cameraButton);
			resultTextView = FindViewById<TextView> (Resource.Id.resultTextView);

			playButton.Click += OnClick_playButton;
			loadButton_img.Click += OnClick_loadButton_img;			
			loadButton_vid.Click += OnClick_loadButton_vid;
			computeButton.Click += OnClick_computeButton;
			scaleButton.Click += OnClick_scaleButton;
			tableButton.Click += OnClick_tableButton;
			//cameraButton.Click += OnClick_cameraButton;
			resultTextView.MovementMethod = new Android.Text.Method.ScrollingMovementMethod ();  //necessary?
			
			vView.Completion += OnCompletion_vView;			
		}

		protected override void OnSaveInstanceState (Bundle bundle)
		{
			//scaleButton.Enabled = false;
			//computeButton.Enabled = false;
			string[][] tdA = CreateStrArrsFromData();
			bundle.PutStringArray("TAGS_ARR", tdA[0]);
			bundle.PutStringArray ("DATA_ARR", tdA[1]);
			bundle.PutParcelable ("MEDIA_URI", mediaUri);
			bundle.PutString ("V_VIEW_HEIGHT", vView.Height.ToString ());
			bundle.PutString ("RESULT_TEXT", resultTextView.Text);
			base.OnSaveInstanceState (bundle);
		}
		
		protected override void OnRestoreInstanceState (Bundle bundle)
		{
			RestoreDataFromStrArr(bundle.GetStringArray ("TAGS_ARR"), bundle.GetStringArray ("DATA_ARR"));
			resultTextView.Text = bundle.GetString ("RESULT_TEXT");
			//after an orientation change, force user to enter scale again
			if (bundle.GetString ("V_VIEW_HEIGHT") != vView.Height.ToString ()) {//iView.Visibility == true || vView.Visibility == true)
				scaleButton.Enabled = true;
				computeButton.Enabled = false;
			}
			/// I really need to make sure this isn't a dumb way to do things. :)
			Android.Net.Uri uri;
			try {
				uri = (Android.Net.Uri) bundle.GetParcelable ("MEDIA_URI");
			} catch (Exception) {
				uri = null;
			}
			if (uri != null) {
				LoadMedia (uri);
			}
			base.OnRestoreInstanceState (bundle);
		}

		protected override void OnDestroy ()
		{
			iView.SetImageDrawable (null);
			vView.SetVideoURI (null);
			base.OnDestroy ();
		}
		#endregion

		protected override Dialog OnCreateDialog (int id)
		{
			Dialog dialog;
			switch (id) {
			case SET_SCALE_DIALOG:	
//			case NAME_YOUR_PICTURE_DIALOG:
			case INPUT_TAG_DIALOG:
				dialog = new Dialog (this);
				dialog.SetContentView (Resource.Layout.EditTextDialog);
				distTextView = (TextView)dialog.FindViewById (Resource.Id.distTextView);
				distEditText = (EditText)dialog.FindViewById (Resource.Id.distEditText);
				Button distOKButton = (Button)dialog.FindViewById (Resource.Id.distOKButton);
				if (id == SET_SCALE_DIALOG) {
					distTextView.Text = "";
					distTextView.Visibility = ViewStates.Gone;
					distOKButton.SetText (Resource.String.distOKButtonText_scale);
					distOKButton.Click += OnClick_distOKButton_SetScale;			
				} else if (id == INPUT_TAG_DIALOG) {
					distTextView.Visibility = ViewStates.Visible;
					distTextView.SetText (Resource.String.distTextViewText_tag);
					distOKButton.SetText (Resource.String.distOKButtonText_tag);
					distOKButton.Click += OnClick_distOKButton_InputTag;
//				} else if (id == NAME_YOUR_PICTURE_DIALOG) {					
//					distTextView.Visibility = ViewStates.Visible;
//					distTextView.SetText (Resource.String.distTextViewText_camera);
//					distOKButton.SetText (Resource.String.distOKButtonText_camera);
//					distOKButton.Click += OnClick_distOKButton_NamePicture;
				} else {
					Console.WriteLine ("If you see this, something is wrong.");
				}
				break;
			default:
				dialog = base.OnCreateDialog (id);
				break;
			}
			return dialog;
		}

		protected override void OnPrepareDialog (int id, Dialog dialog)
		{
			switch (id) {
			case (SET_SCALE_DIALOG):
				distEditText.Text = "";
				//distEditText.InputType = Android.Text.InputTypes.ClassNumber; //see http://mono-for-android.1047100.n5.nabble.com/Android-InputType-constants-namespace-td5054578.html
				//distEditText.InputType = Android.Text.InputTypes.NumberFlagDecimal;	
				distEditText.KeyListener = new Android.Text.Method.DigitsKeyListener(false,true);		
				//distEditText.InputType = Android.Text.InputTypes.NumberFlagDecimal;
				break;				
			case (INPUT_TAG_DIALOG):
//			case (NAME_YOUR_PICTURE_DIALOG):
				distEditText.KeyListener = new Android.Text.Method.TextKeyListener(Android.Text.Method.TextKeyListener.Capitalize.None, false);		
				//distEditText.InputType = Android.Text.InputTypes.ClassText; //see http://mono-for-android.1047100.n5.nabble.com/Android-InputType-constants-namespace-td5054578.html
				distEditText.Text = "";
				//distEditText.PerformClick ();
				break;
			default:
				base.OnPrepareDialog (id, dialog);
				break;
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			//base.OnActivityResult (requestCode, resultCode, data);
			// See which child activity (Intent) is calling us back.
			switch (requestCode) {
			case OPEN_FILE_ACTIVITY:
				if (resultCode == Result.Canceled) {
					// This is the standard resultCode that is sent back if the
					// activity crashed or didn't doesn't supply an explicit result.
					Console.WriteLine ("Cancelled");
				} else {
					string path = data.GetStringExtra ("FILE_PATH");
					LoadMedia (path);
				}
				break;
			case OPEN_IMAGE_ACTIVITY:
				if (resultCode == Result.Ok) {
			        LoadMedia ((Android.Net.Uri)data.Data);
			    }
				break;
//			case CAMERA_ACTIVITY:
//				if (_file != null)
//				{
//				    var imageView = FindViewById<ImageView> (Resource.Id.iView);
//
//				    // make it available in the gallery
//				    var mediaScanIntent = new Intent (Intent.ActionMediaScannerScanFile);
//			        var contentUri = Android.Net.Uri.FromFile (_file);
//			        mediaScanIntent.SetData (contentUri);
//			        this.SendBroadcast (mediaScanIntent);
//
//				    // display in ImageView
//				    //var bitmap = MediaStore.Images.Media.GetBitmap (ContentResolver, contentUri);
//				    //iView.SetImageBitmap (bitmap);   
//					LoadMedia (_file.Path);
//				    //bitmap.Dispose ();
//				} else {
//					Console.WriteLine (_file.Exists().ToString());
//				}
//				break;
//			case SHOW_TABLE_ACTIVITY:
//				if (resultCode == Result.Canceled) {
//					// This is the standard resultCode that is sent back if the
//					// activity crashed or didn't doesn't supply an explicit result.
//					Console.WriteLine ("Cancelled");
//				} else {
//					// string value = data.GetStringExtra ("KEY");
//				}
//				break;
			default:
				break;
			}
		}

		private void LoadMedia (Android.Net.Uri uri)
		{
			//string[] nameParts = path.Split (".".ToCharArray ());
			//string filetype = nameParts [nameParts.Length - 1];
			//string filetype = nameParts[nameParts.Length - 1]; //might be whole filename...

			//http://stackoverflow.com/questions/12473851/how-i-can-get-the-mime-type-of-a-file-having-its-uri
			ContentResolver cR = this.ContentResolver;
			//MimeTypeMap mime = MimeTypeMap.Singleton;
			string filetype = cR.GetType (uri);

			aCrossHairView.locationArr.Clear ();
			aCrossHairView.Invalidate ();
			//resultTextView.SetText (Resource.String.resultTextView_title);
			scaleButton.Enabled = true;
			computeButton.Enabled = false;
			switch (filetype.Split ('/')[0]) {
			case "video":
				vView.SetVideoURI (uri);
				mediaUri = uri;
				iView.Visibility = ViewStates.Invisible;
				vView.Visibility = ViewStates.Visible;
				playButton.Enabled = true;
				break;
			case "image":
				iView.SetImageURI (uri);
				mediaUri = uri;

				iView.Visibility = ViewStates.Visible;
				vView.Visibility = ViewStates.Invisible;
				playButton.Enabled = false;
				break;
			default:
				Console.WriteLine (filetype);
				break;
			}
		}
		/// <summary>
		/// Loads the media to either iView or vView, depending on filetype (that is, extension).
		/// </summary>
		/// <param name='filetype'>
		/// Filetype. A string that is made up of those characters that follow the dot (eg. frog.JPG)
		/// </param>
		/// <param name='path'>
		/// Path. A string. The absolute path to the file.
		/// </param>
		private void LoadMedia (string path)
		{
			Android.Net.Uri.Builder builder = new Android.Net.Uri.Builder ();
			builder.Path (path);
			LoadMedia (builder.Build ()); //use the uri
		
//			string[] nameParts = path.Split (".".ToCharArray ());
//			string filetype = nameParts [nameParts.Length - 1];
//			aCrossHairView.locationArr.Clear ();
//			aCrossHairView.Invalidate ();
//			//resultTextView.SetText (Resource.String.resultTextView_title);
//			scaleButton.Enabled = true;
//			computeButton.Enabled = false;
//			switch (filetype) {
//			case "3gp":
//				vView.SetVideoPath (path);
//				mediaPath = path;
//
//				iView.Visibility = ViewStates.Invisible;
//				vView.Visibility = ViewStates.Visible;
//				playButton.Enabled = true;
//				break;
//			case "jpg":
//				Console.WriteLine (path);
//				Android.Net.Uri.Builder builder = new Android.Net.Uri.Builder ();
//				builder.Path (path);
//				iView.SetImageURI (builder.Build ());
//				mediaPath = path;
//
//				iView.Visibility = ViewStates.Visible;
//				vView.Visibility = ViewStates.Invisible;
//				playButton.Enabled = false;
//				break;
//			}
		}

		string[][] CreateStrArrsFromData ()
		{
			string[] tagArr = new string[dataKVPairList.Count];
			string[] dataArr = new string[dataKVPairList.Count];
			for (int i = 0; i < dataKVPairList.Count; i++) {
				KeyValuePair<string, string> kvPair = dataKVPairList [i];
				tagArr [i] = kvPair.Key;
				dataArr [i] = kvPair.Value;
			}
			return new string[][]{tagArr, dataArr};
		}
		void RestoreDataFromStrArr (string[] tagArr, string[] dataArr)
		{
			dataKVPairList = new List<KeyValuePair<string, string>>();
//			string[] tagArr = tagDataArrs[0];
//			string[] dataArr = tagDataArrs[1];
			for (int i = 0; i < tagArr.Length; i++) {
				dataKVPairList.Add (new KeyValuePair<string, string>(tagArr[i],dataArr[i]));
			}
		}
		

		#region EVENT LISTENERS
		private void OnCompletion_vView (object sender, EventArgs e)
		{
			vView.SeekTo (0);
			playButton.SetText (Resource.String.playButtonText_play);
		}

		private void OnClick_playButton (object sender, EventArgs e)
		{
			if (vView.IsPlaying) {
				vView.Pause ();
				playButton.SetText (Resource.String.playButtonText_play);
			} else if (vView.CurrentPosition >= 0) {
				vView.Start ();
				playButton.SetText (Resource.String.playButtonText_pause);
			}
		}

		private void OnClick_loadButton_img (object sender, EventArgs e)
		{
			Intent intent = new Intent(Intent.ActionPick, null);
			intent.SetType("image/*");
			StartActivityForResult(intent, OPEN_IMAGE_ACTIVITY);
		}

		private void OnClick_loadButton_vid (object sender, EventArgs e)
		{
			//http://stackoverflow.com/questions/6995901/android-unable-to-invoke-gallery-with-video?rq=1
			//WORKS PERFECTLY FOR VIDEO
			Intent intent = new Intent(Intent.ActionPick, null);
			intent.SetType("video/*");
			StartActivityForResult(intent, OPEN_IMAGE_ACTIVITY);
		}

		private void OnClick_computeButton (object sender, EventArgs e)
		{			
			if (aCrossHairView.locationArr.Count == 2) {
				currDist = aCrossHairView.getDist ();
				ShowDialog (INPUT_TAG_DIALOG); //get currTag
				
				//UpdateResult();//currTag, currDist.realValue.ToString ("0.000") + "\n");
				//resultTextView.Text += currentDist.realValue.ToString ("0.000") + "\n";
			} else {
				Toast.MakeText (
					this.ApplicationContext,
					"Two points are required before computing the distance between them.",
					ToastLength.Short
				).Show ();
			}
		}

		private void OnClick_scaleButton (object sender, EventArgs e)
		{
			//currentScreenDist = aCrossHairView.ComputeDist ().screenValue;
			//Console.WriteLine (currentScreenDist);///////////////////////////////////////////////NOT CALLED!!!
			if (aCrossHairView.locationArr.Count == 2) {
				ShowDialog (SET_SCALE_DIALOG);
				currDist = aCrossHairView.getDist ();
			} else {
				Toast.MakeText (
					this.ApplicationContext,
					"Two points are required before setting the scale.",
					ToastLength.Short
				).Show ();
			}
		}
		/// <summary>
		/// Raises the click_table button event.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		private void OnClick_tableButton (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof(TableActivity));
			string[][] ans = CreateStrArrsFromData();
			string[] tagArr = ans[0];
			string[] dataArr = ans[1];
			intent.PutExtra ("TAGS", tagArr);
			intent.PutExtra ("MEASUREMENTS", dataArr);
//			StartActivity(SHOW_TABLE_ACTIVITY);
			StartActivity (intent);
		}

		/// <summary>
		/// Directly copied from 
		/// http://docs.xamarin.com/android/recipes/Other_UX/Camera_Intent/Take_a_Picture_and_Save_Using_Camera_App
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		//private void OnClick_cameraButton (object sender, EventArgs e)
//		{
//			//choose a filename for your photo
//			ShowDialog (NAME_YOUR_PICTURE_DIALOG);
//		}

//		private void OnClick_distOKButton_NamePicture (object sender, EventArgs e)
//		{ 
//			//make dir 
//			Intent intent = new Intent (MediaStore.ActionImageCapture);
//
//			var availableActivities = this.PackageManager.QueryIntentActivities (intent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
//
//			if (availableActivities != null && availableActivities.Count > 0) {              
//				Java.IO.File dir = new Java.IO.File (
//			        Android.OS.Environment.GetExternalStoragePublicDirectory (
//			        Android.OS.Environment.DirectoryPictures), "ScaleCamera");     
//              
//				if (!dir.Exists ()) {
//					dir.Mkdirs ();  
//				}   
//				//Name your photo with the filename you've already chosen
//				_file = new Java.IO.File (dir, distEditText.Text);
//				DismissDialog(NAME_YOUR_PICTURE_DIALOG);
//				RemoveDialog (NAME_YOUR_PICTURE_DIALOG);
//				//open camera
//				intent.PutExtra (MediaStore.ExtraOutput,
//    				Android.Net.Uri.FromFile (_file));                    
//				StartActivityForResult (intent, CAMERA_ACTIVITY);
//			}
//		}
		private void OnClick_distOKButton_SetScale (object sender, EventArgs e)
		{
			double inputDouble;
			string inputText = distEditText.Text;///
			if (double.TryParse (inputText, out inputDouble) == true) {
				Distance.SetScale (currDist.screenValue, inputDouble);
				DismissDialog (SET_SCALE_DIALOG);
				RemoveDialog(SET_SCALE_DIALOG);
				computeButton.Enabled = true;
			} else {
				Toast.MakeText (this, string.Format("Please enter a valid number. You entered '{0}'",inputText), ToastLength.Short).Show ();
			}
		}
		
		/// <summary>
		/// Raises the click_dist OK button_ input tag event.
		/// Sets currTag if inputText is good, else makes Toast.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='e'>
		/// E.
		/// </param>
		private void OnClick_distOKButton_InputTag (object sender, EventArgs e)
		{
			string inputText = distEditText.Text;
			bool tagIsUnique = true;
			foreach (KeyValuePair<string, string> kvPair in dataKVPairList) {
				if (inputText == kvPair.Key) {
					tagIsUnique = false;
					Toast.MakeText (
						this,
						"The tag must be a UNIQUE identifier.",
						ToastLength.Short
					)
						.Show ();
					break;
				}
			}
			if (tagIsUnique == true) {
				string distStr = currDist.realValue.ToString ("0.000") + "\n";
				dataKVPairList.Add (new KeyValuePair<string, string> (inputText, distStr));
				resultTextView.Text += distStr;			
				Android.Text.Layout layout = resultTextView.Layout;
				if (layout != null && resultTextView.Text != distStr) {
					// scrollAmount = bottomOfText - scrolledAlready - height
					int scrollDelta = layout.GetLineBottom (resultTextView.LineCount - 1)
						- resultTextView.ScrollY - resultTextView.Height;
					if (scrollDelta > 0) {
						resultTextView.ScrollBy (0, scrollDelta);
					}
				}
				DismissDialog (INPUT_TAG_DIALOG);				
				RemoveDialog(INPUT_TAG_DIALOG);
			}
		}
//		/// <summary>
//		///
//		/// http://stackoverflow.com/a/9561614
//		/// </summary>
//		/// <param name='sender'>
//		/// Sender.
//		/// </param>
//		/// <param name='e'>
//		/// E.
//		/// </param>
//		private void OnTextChanged_resultTextView (object sender, EventArgs e)
//		{
//			Console.WriteLine ("OnAfterTextChanged_resultTextView CALLED");
//			Android.Text.Layout layout = resultTextView.Layout;
//			if (layout != null) {
//				// scrollDelta = bottomOfText - scrolledAlready - height
//				int scrollDelta = layout.GetLineBottom (resultTextView.LineCount - 1)
//					- resultTextView.ScrollY - resultTextView.Height;
//				if (scrollDelta > 0)
//				{
//					resultTextView.ScrollBy (0, scrollDelta);
//				}
//			}
//		}
		#endregion	
	}

	public class Distance
	{
		private static double? scaleRatio = null;
		public double screenValue;
		public readonly double realValue;

		public Distance (float screenD)
		{
			screenValue = (double)screenD;
			if (scaleRatio != null) {
				realValue = (double)scaleRatio * screenValue;
			}
		}

		public Distance (double screenD)
		{
			screenValue = screenD;
			if (scaleRatio != null) {
				realValue = (double)scaleRatio * screenValue;
			}
		}

		/// <summary>
		/// Sets the scale.
		/// STATIC METHOD
		/// </summary>
		/// <param name='measuredDist'>
		/// Measured dist.
		/// </param>
		/// <param name='realDist'>
		/// Real dist.
		/// </param>
		public static void SetScale (double measuredDist, double realDist)
		{
			scaleRatio = realDist / measuredDist;
		}
	}

	public class Point
	{
		public float X;
		public float Y;

		public Point (float x, float y)
		{
			X = x;
			Y = y;
		}

		public Distance dist (Point p)
		{
			return new Distance (Math.Sqrt (Math.Pow (X - p.X, 2) + Math.Pow (
				Y - p.Y,
				2
			)
			)
			);
		}
	}

}
