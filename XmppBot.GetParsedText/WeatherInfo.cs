using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class WeatherInfo
    {
        public WeatherInfo()
        {
        }

        public WeatherResponse GetWeatherInfo(string city)
        {
            WebClient client = new WebClient();
            string response = client.DownloadString(string.Format("http://api.worldweatheronline.com/premium/v1/weather.ashx?q={0}&key=cce343f9e6bd4d159bc133122171803&format=json", city));
            return JsonConvert.DeserializeObject<WeatherResponse>(response);
        }
    }
}
