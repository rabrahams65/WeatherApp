using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using WeatherApp.Fragments;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button getWeatherButton;
        TextView placeTextView, temperatureTextView, weatherDescriptionTextView;
        EditText cityNameEditText;
        ImageView weatherImageView;
        ProgressDialogFragment ProgressDialog;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            getWeatherButton = FindViewById<Button>(Resource.Id.getWeatherButton);
            placeTextView = FindViewById<TextView>(Resource.Id.placeText);
            temperatureTextView = FindViewById<TextView>(Resource.Id.temperatureText);
            weatherDescriptionTextView = FindViewById<TextView>(Resource.Id.weatherDescriptionText);
            cityNameEditText = FindViewById<EditText>(Resource.Id.cityNameText);
            weatherImageView = FindViewById<ImageView>(Resource.Id.weatherImage);

            getWeatherButton.Click += GetWeatherButton_Click;
        }

        private void GetWeatherButton_Click(object sender, System.EventArgs e)
        {
            string place = cityNameEditText.Text;
            GetWeather(place);
            cityNameEditText.Text = string.Empty;
        }

        async void GetWeather(string place)
        {
            //Nuget Package: System.Net.Http
            string apiKey = "9fe138535ab3ae410e746fa635ae59e6";
            string apiBase = "https://api.openweathermap.org/data/2.5/weather?q=";
            string unit = "metric";

            if (string.IsNullOrEmpty(place))
            {
                Toast.MakeText(this, "Please enter a valid place name!", ToastLength.Short).Show();
                return;
            }

            //Nuget Package: Xam.Plugin.Connectivity
            if (!CrossConnectivity.Current.IsConnected) 
            {
                Toast.MakeText(this, "No Internet Connection", ToastLength.Short).Show();
                return;
            }


            string url = apiBase + place + "&appid=" + apiKey + "&units=" + unit;

            ShowProgressDialog("Fetching weather...");


            //Nuget Package: System.Net.Http
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);

            Console.WriteLine(result);
            //Nuget Package: Newtonsoft.Json
            var resultObject = JObject.Parse(result);

            string weatherDescription = resultObject["weather"][0]["description"].ToString();
            string icon = resultObject["weather"][0]["icon"].ToString();
            string temperature = resultObject["main"]["temp"].ToString();
            string placeName = resultObject["name"].ToString();
            string country = resultObject["sys"]["country"].ToString();
            weatherDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(weatherDescription);


            weatherDescriptionTextView.Text = weatherDescription;
            placeTextView.Text = placeName + ", " + country;
            temperatureTextView.Text = temperature;

            //Download Image using WebRequest
            string imageUrl = "http://openweathermap.org/img/wn/" + icon + ".png";
            WebRequest request = default(WebRequest);
            request = WebRequest.Create(imageUrl);
            request.Timeout = int.MaxValue;
            request.Method = "GET";

            WebResponse response = default(WebResponse);
            response = await request.GetResponseAsync();

            MemoryStream ms = new MemoryStream();
            response.GetResponseStream().CopyTo(ms);

            byte[] imageData = ms.ToArray();
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            weatherImageView.SetImageBitmap(bitmap);

            CloseProgressDialog();

        }



        void ShowProgressDialog(string status)
        {
            ProgressDialog = new ProgressDialogFragment(status);
            var trans = SupportFragmentManager.BeginTransaction();
            ProgressDialog.Cancelable = false;
            ProgressDialog.Show(trans, "progress");
        }

        void CloseProgressDialog()
        {
            if (ProgressDialog != null)
            {
                ProgressDialog.Dismiss();
                ProgressDialog = null;
            }
        }
    }
}