using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XmppBot.GetParsedText
{
    public class Answer
    {
        public Answer()
        {
        }

        private string CutString(string input, int from, int lastindex)
        {
            return input.Substring(from, lastindex - from);
        }

        public string GetAnswer(QuestionType questionType, string questionVariables)
        {
            string weather;
            switch (questionType)
            {
                case QuestionType.Weather:
                    {
                        weather = this.GetWeather(questionVariables);
                        break;
                    }
                case QuestionType.MortgageInfo:
                    {
                        weather = this.GetWeather("");
                        break;
                    }
                case QuestionType.MortgageCalculation:
                    {
                        weather = this.GetMortgage(questionVariables.Split(new char[] { ' ' }).First<string>(), questionVariables.Split(new char[] { ' ' }).Last<string>());
                        break;
                    }
                case QuestionType.RoomQuery:
                    {
                        weather = this.GetRoomQuery(questionVariables);
                        break;
                    }
                case QuestionType.FindIndividualQuery:
                    {
                        weather = this.GetFindIndividualQuery(questionVariables);
                        break;
                    }
                case QuestionType.JiraIssue:
                    {
                        weather = this.GetJiraIssue(questionVariables);
                        break;
                    }
                case QuestionType.CommodityPrice:
                    {
                        weather = this.GetCommodityPrice(questionVariables);
                        break;
                    }
                default:
                    {
                        goto case QuestionType.MortgageInfo;
                    }
            }
            return weather;
        }

        private string GetCommodityPrice(string questionVariables)
        {
            WebClient client = new WebClient();
            string response = client.DownloadString(string.Format("http://www.supermarktaanbiedingen.com/zoeken/{0}", questionVariables));
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            List<Product> products = new List<Product>();
            if (doc.DocumentNode != null)
            {
                foreach (HtmlNode li in (IEnumerable<HtmlNode>)doc.DocumentNode.SelectNodes("//li"))
                {
                    try
                    {
                        string supermarket = li.GetAttributeValue("class", null);
                        Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                        li.InnerHtml = regex.Replace(li.InnerHtml, "").Replace("\r\n", "").Replace("\t", "");
                        if (li.InnerHtml.Contains("card_prijs-oud"))
                        {
                            string[] elements = li.InnerHtml.Replace("><", "@").Split(new char[] { '@' });
                            string offerLink = null;
                            string imageLink = null;
                            string offerprice = null;
                            string normlPrice = null;
                            string title = null;
                            string text = null;
                            string[] strArrays = elements;
                            for (int i = 0; i < (int)strArrays.Length; i++)
                            {
                                string element = strArrays[i];
                                offerLink = (element.Contains("a href") ? this.CutString(element, element.IndexOf("href=") + 5, element.IndexOf("title") - 1).Replace("\"", "") : offerLink);
                                imageLink = (element.Contains("card_productimage") ? this.CutString(element, element.IndexOf("src=") + 4, element.IndexOf("alt") - 1).Replace("\"", "") : imageLink);
                                offerprice = (element.Contains("card_prijs\"") ? this.CutString(element, element.IndexOf(">") + 1, element.IndexOf("<")).Replace("\"", "").Replace("&nbsp;", "").Replace("&", "").Replace(";", "") : offerprice);
                                normlPrice = (element.Contains("card_prijs-oud") ? this.CutString(element, element.IndexOf(">") + 1, element.IndexOf("<")).Replace("\"", "").Replace("&nbsp;", "").Replace("&", "").Replace(";", "") : normlPrice);
                                title = (element.Contains("card_title") ? this.CutString(element, element.IndexOf(">") + 1, element.IndexOf("<")).Replace("\"", "") : title);
                                text = (element.Contains("card_text") ? this.CutString(element, element.IndexOf(">") + 1, element.IndexOf("<")).Replace("\"", "") : text);
                            }
                            products.Add(new Product()
                            {
                                SuperMarket = supermarket,
                                OfferLink = string.Format("http://www.supermarktaanbiedingen.com/{0}", offerLink),
                                ImageLink = string.Format("http://www.supermarktaanbiedingen.com/{0}", imageLink),
                                Offerprice = offerprice,
                                NormlPrice = normlPrice,
                                Title = title,
                                Text = text
                            });
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception exception)
                    {
                    }
                }
            }
            return JsonConvert.SerializeObject(products);
        }

        private string GetFindIndividualQuery(string name)
        {
            return JsonConvert.SerializeObject((new IndividualInfo()).GetIndividualInfo(name));
        }

        private string GetJiraIssue(string questionVariables)
        {
            return "hi";
        }

        private string GetMortgage(string annualIncome, string age)
        {
            HypothekerInfo weatherInfo = new HypothekerInfo();
            return JsonConvert.SerializeObject(weatherInfo.GetMortgageInfo(annualIncome, age));
        }

        private string GetRoomQuery(string city)
        {
            string str;
            if (city != null)
            {
                str = ((Regex.IsMatch(city, "\\brotterdam\\b", RegexOptions.IgnoreCase) || Regex.IsMatch(city, "\\bamsterdam\\b", RegexOptions.IgnoreCase) ? false : !Regex.IsMatch(city, "\\bmanchester\\b", RegexOptions.IgnoreCase)) ? "Please specify office location. For ex: Available rooms in Amsterdam." : JsonConvert.SerializeObject((new RoomInfo()).GetAvailableRooms(city)));
            }
            else
            {
                str = "Please specify office location. For ex: Available rooms in Amsterdam.";
            }
            return str;
        }

        private string GetWeather(string city)
        {
            Current_condition currentCondition;
            WeatherResponse weatherInfo = (new WeatherInfo()).GetWeatherInfo(city);
            if (weatherInfo != null)
            {
                Weather weather = weatherInfo.data;
                if (weather != null)
                {
                    List<Current_condition> currentConditions = weather.current_condition;
                    if (currentConditions != null)
                    {
                        currentCondition = currentConditions.First<Current_condition>();
                    }
                    else
                    {
                        currentCondition = null;
                    }
                }
                else
                {
                    currentCondition = null;
                }
            }
            else
            {
                currentCondition = null;
            }
            Current_condition weatherInformation = currentCondition;
            return (weatherInformation == null ? string.Format("Couldn't retrive weather info for {0}", city) : string.Format("Temperatur is {0} degree celcius. Feels like {1} degree celcius. Humidity is {2}. Wind Speed is {3} kmph", new object[] { weatherInformation.temp_C, weatherInformation.FeelsLikeC, weatherInformation.humidity, weatherInformation.windspeedKmph }));
        }
    }

    
}
