using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Weather
    {
        public List<Request> request;

        public List<Current_condition> current_condition;

        public List<ErrorMessage> error;

        public Weather()
        {
        }
    }
    public class Request
    {
        public string type;

        public string query;

        public Request()
        {
        }
    }
    public class ErrorMessage
    {
        public string msg;

        public ErrorMessage()
        {
        }
    }
    public class Current_condition
    {
        public int temp_C;

        public int temp_F;

        public int windspeedMiles;

        public int windspeedKmph;

        public int winddirDegree;

        public string winddir16Point;

        public int humidity;

        public int visibility;

        public int pressure;

        public int cloudcover;

        public int FeelsLikeC;

        public int FeelsLikeF;

        public Current_condition()
        {
        }
    }
}
