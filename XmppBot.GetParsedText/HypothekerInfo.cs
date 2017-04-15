using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class HypothekerInfo
    {
        public HypothekerInfo()
        {
        }

        public Mortgage GetMortgageInfo(string annualIncome, string age)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string data = string.Concat(new string[] { "{ApplicantYearlyIncome:", annualIncome, ",ApplicantAge:", age, "}" });
            string hypothekerInfo = client.UploadString("https://api.hypotheker.nl/Calculations/CalculateMaximumMortgageByIncome", "POST", data);
            return JsonConvert.DeserializeObject<Mortgage>(hypothekerInfo);
        }
    }
}
