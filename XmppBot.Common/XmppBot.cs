﻿using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.x.muc;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XmppBot.GetParsedText;
using static XmppBot.GetParsedText.Questions;

namespace XmppBot.Common
{
    public class Bot
    {
        #region log4net
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private const int MaxRosterSize = 100;

        private static AggregateCatalog _catalog = null;
        private static DirectoryCatalog _directoryCatalog = null;
        private static XmppClientConnection _client = null;


        private XmppBotConfig _config;

        public Bot(XmppBotConfig config)
        {
            _config = config;
        }

        public void Stop()
        {
        }

        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, args) =>
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                return loadedAssemblies.FirstOrDefault(asm => asm.FullName == args.Name);
            };

            // use our running app and a directory for our MEF catalog
            _catalog = new AggregateCatalog();
            _catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            
            // create a MEF directory catalog for the plugins directory
            string pluginsDirectory = Environment.CurrentDirectory + "\\plugins\\";
            if (!Directory.Exists(pluginsDirectory))
            {
                Directory.CreateDirectory(pluginsDirectory);
            }
            _directoryCatalog = new DirectoryCatalog(Environment.CurrentDirectory + "\\plugins\\");
            _catalog.Catalogs.Add(_directoryCatalog);


            _catalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(_catalog_Changed);
            var pluginList = LoadPlugins();

            log.Info(pluginList);

            _client = new XmppClientConnection(_config.Server);

            //_client.ConnectServer = "talk.google.com"; //necessary if connecting to Google Talk
            _client.AutoResolveConnectServer = false;

            _client.OnLogin += new ObjectHandler(xmpp_OnLogin);
            _client.OnMessage += new MessageHandler(xmpp_OnMessage);
            _client.OnError += new ErrorHandler(_client_OnError);

            log.Info("Connecting...");
            _client.Resource = _config.Resource;
            _client.Open(_config.User, _config.Password);
            log.Info("Connected.");

            _client.OnRosterStart += new ObjectHandler(_client_OnRosterStart);
            _client.OnRosterItem += new XmppClientConnection.RosterHandler(_client_OnRosterItem);
        }

        #region Xmpp Events

        void xmpp_OnLogin(object sender)
        {
            MucManager mucManager = new MucManager(_client);

            string[] rooms = _config.Rooms.Split(',');

            foreach (string room in rooms)
            {
                Jid jid = new Jid(room + "@" + _config.ConferenceServer);
                mucManager.JoinRoom(jid, _config.RoomNick);
            }
        }

        private void xmpp_OnMessage(object sender, Message msg)
        {
            if (!string.IsNullOrEmpty(msg.Body))
            {
                log.InfoFormat("Message : {0} - from {1}", msg.Body, msg.From);

                IChatUser user = null;

                if (msg.Type == MessageType.groupchat)
                {
                    //msg.From.Resource = User Room Name
                    //msg.From.Bare = Room 'email'
                    //msg.From.User = Room id

                    user = _roster.Values.FirstOrDefault(u => u.Name == msg.From.Resource);
                }
                else
                {
                    _roster.TryGetValue(msg.From.Bare, out user);
                }

                // we can't find a user or this is the bot talking
                if (null == user || _config.RoomNick == user.Name)
                    return;

                ParsedLine line = new ParsedLine(msg.From.Bare, msg.Body.Trim(), msg.From.User, user, (BotMessageType) msg.Type);
                Dictionary<QuestionType,string> lineParsed = null;
                //Regex.IsMatch(text, "\\bthe\\b", RegexOptions.IgnoreCase)
                if (Regex.IsMatch(line.Command, "\\bhi\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bmorning\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bhello\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bhey\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bheyy\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bGoodMorning\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bgoedendag\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bmorgen\\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line.Command, "\\bgoedemorgen\\b", RegexOptions.IgnoreCase)
                    )
                {
                    var greeting = DateTime.Now.Hour<12? "Good Morning": DateTime.Now.Hour < 19?"Good afternoon":"Good evening";
                    SendMessage(msg.From, $"{greeting} {user?.Name}.I am ur lovely bot.I am not human but definitely a depster and Tammo.:)", msg.Type);
                    SendMessage(msg.From, $"I am here to help you. As of nowI have learnt to answer following questions?", msg.Type);
                    SendMessage(msg.From, $"1) What is the weather in CityName or type: 1-CityName", msg.Type);
                    SendMessage(msg.From, $"2) How much can I take loan or type: 2-mortgage", msg.Type);
                    SendMessage(msg.From, $"3) What is available room in Amsterdam or type: 3-Amsterdam", msg.Type);
                    SendMessage(msg.From, $"4) Where is X busy? or what is X doing or type 4-Rupesh", msg.Type);
                    SendMessage(msg.From, $"5) Compare price Bier? or Prijs Heineken or typr 5-tomat", msg.Type);
                }
                else if(!line.Command.Contains("<p>"))
                {
                    try
                    {
                        var parser = new LineParser();
                        lineParsed = parser.ParseLine(line.Command, line.Raw);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                        SendMessage(msg.From, $"Sorry i couldn't understand your question.", msg.Type);
                    }
                }
                var answer = new Answer();
                
                //        var plist = Plugins.ToList();
                //        plist.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));

                //        foreach (var p in plist)
                //        {
                //            var helpLine = p.Help(line);
                //            if (!String.IsNullOrWhiteSpace(helpLine))
                //            {
                //                helpText.AppendLine(p.Help(line));
                //            }
                //        }

                //        helpText.AppendLine("-----------------------");
                //        helpText.AppendLine("En/Dis-able a plugin: !disable|!enable <pluginname>");
                if (lineParsed !=null && !line.Command.Contains("<p>"))
                {
                    switch(lineParsed.First().Key)
                    {
                        case QuestionType.Weather:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var answerWeather= answer.GetAnswer(QuestionType.Weather, lineParsed.First().Value);
                            SendMessage(msg.From, $"{answerWeather}", msg.Type);
                            break;
                        case QuestionType.MortgageInfo:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            SendMessage(msg.From, $"Please type my salary is ur salary euro/year and my age is ur age.For ex: My salary is 50000 euro/year and my age is 35.", msg.Type);
                            break;
                        case QuestionType.MortgageCalculation:
                            SendMessage(msg.From, $"You asked: for mortgage caluclation. ur salary is {lineParsed.First().Value.Split(' ').First()}. And your age is {lineParsed.First().Value.Split(' ').Last()}  ", msg.Type);
                            var mortgageInfo = answer.GetAnswer(QuestionType.MortgageCalculation, lineParsed.First().Value);
                            SendMessage(msg.From, $"{mortgageInfo}", msg.Type);
                            break;
                        case QuestionType.RoomQuery:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var roomQueryAnster = answer.GetAnswer(QuestionType.RoomQuery, lineParsed.First().Value);
                            SendMessage(msg.From, $"{roomQueryAnster}", msg.Type);
                            SendMessage(msg.From, $"Above availabe rooms are only Test Data. I am waiting for sufficient right on Google Calendar API from Stekker support", msg.Type);
                            break;
                        case QuestionType.FindIndividualQuery:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var individualQuery = answer.GetAnswer(QuestionType.FindIndividualQuery, lineParsed.First().Value);
                            var individualInfo = JsonConvert.DeserializeObject<Individual>(individualQuery);
                            var helpTextt = new StringBuilder();
                            helpTextt.AppendLine($"/code {Environment.NewLine}.{lineParsed.First().Value} is busy");
                            SendMessage(msg.From, $"{helpTextt}", msg.Type);
                            break;
                        case QuestionType.JiraIssue:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var jiraQuery = answer.GetAnswer(QuestionType.JiraIssue, user.Name);
                            SendMessage(msg.From, $"{jiraQuery}", msg.Type);
                            SendMessage(msg.From, $"Above information about {lineParsed.First().Value} is only Test Data. I am waiting for sufficient right on Google Calendar API from Stekker support", msg.Type);
                            break;
                        case QuestionType.CommodityPrice:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var priceQuery = answer.GetAnswer(QuestionType.CommodityPrice, lineParsed.First().Value);
                            var products = JsonConvert.DeserializeObject<List<Product>>(priceQuery);
                            if (products.Count == 0) {
                                SendMessage(msg.From, $" Sorry currently i can't find price of {lineParsed.First().Value}!. Please try dutch names.!", msg.Type);
                                SendMessage(msg.From, $"1) What is the weather in CityName or type: 1-CityName", msg.Type);
                                SendMessage(msg.From, $"2) How much can I take loan or type 2-mortgage", msg.Type);
                                SendMessage(msg.From, $"3) What is available room in Amsterdam or type 3-Amsterdam", msg.Type);
                                SendMessage(msg.From, $"4) Where is X busy? or what is X doing or type 4-Rupesh", msg.Type);
                                SendMessage(msg.From, $"5) Compare price Bier? or Prijs Heineken or type 5-tomat", msg.Type);
                            }
                            foreach(var product in products.OrderByDescending(p=>p.Discount).Take(5))
                            {
                                var helpText = new StringBuilder();
                                helpText.AppendLine($"At:{product.SuperMarket.ToUpper()}. Product: {product.Title}.");
                                helpText.AppendLine($"Normalprice: {product.NormlPrice.Replace("euro", "€")} .Offerprice: {product.Offerprice.Replace("euro", "€")}. Discount is {product.Discount}%");
                                helpText.AppendLine($"Link is: {product.OfferLink.Replace("http://","")}  Image: {product.ImageLink} ");
                                SendMessage(msg.From, $"{helpText}", msg.Type);
                            }
                            break;
                        case QuestionType.UnknownQuestionType:
                            SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            SendMessage(msg.From, $" Sorry currenlty i can't help you with that!!!", msg.Type);
                            break;
                        default:
                            SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            SendMessage(msg.From, $" Sorry currently i can't help you with that!!!", msg.Type);
                            SendMessage(msg.From, $"you can also ask me following questions", msg.Type);
                            SendMessage(msg.From, $"1) What is the weather in CityName or type: 1-CityName", msg.Type);
                            SendMessage(msg.From, $"2) How much can I take loan or type 2-mortgage", msg.Type);
                            SendMessage(msg.From, $"3) What is available room in Amsterdam or type 3-Amsterdam", msg.Type);
                            SendMessage(msg.From, $"4) Where is X busy? or what is X doing or type 4-Rupesh", msg.Type);
                            SendMessage(msg.From, $"5) Compare price Bier? or Prijs Heineken or type 5-tomat", msg.Type);
                            break;

                    }
                }
                //switch (line.Command)
                //{
                //    case "help":
                //        var helpText = new StringBuilder();
                //        var plist = Plugins.ToList();
                //        plist.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));

                //        foreach (var p in plist)
                //        {
                //            var helpLine = p.Help(line);
                //            if (!String.IsNullOrWhiteSpace(helpLine))
                //            {
                //                helpText.AppendLine(p.Help(line));
                //            }
                //        }

                //        helpText.AppendLine("-----------------------");
                //        helpText.AppendLine("En/Dis-able a plugin: !disable|!enable <pluginname>");
                //        helpText.AppendLine("List plugin names: !list");
                //        SendMessage(msg.From, helpText.ToString(), msg.Type);

                //        break;
                //    case "close":
                //        SendMessage(msg.From, "I'm a quitter...", msg.Type);
                //        Environment.Exit(1);
                //        return;

                //    case "reload":
                //        SendMessage(msg.From, LoadPlugins(), msg.Type);
                //        break;

                //    default:
                //        Task.Factory.StartNew(() =>
                //                              Parallel.ForEach(Plugins,
                //                                  plugin => SendMessage(msg.From, plugin.Evaluate(line), msg.Type)
                //                                  ));
                //        break;
                //}
            }
        }


        static void _client_OnError(object sender, Exception ex)
        {
            log.Error("Exception: " + ex);
        }

        static void _catalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            if (null != _directoryCatalog)
                _directoryCatalog.Refresh();
        }

        #endregion

        #region Roster Management

        private static Dictionary<string, IChatUser> _roster = new Dictionary<string, IChatUser>(MaxRosterSize);

        static void _client_OnRosterStart(object sender)
        {
            _roster = new Dictionary<string, IChatUser>(MaxRosterSize);
        }

        static void _client_OnRosterItem(object sender, RosterItem item)
        {
            if (!_roster.ContainsKey(item.Jid.User))
            {
                _addRoster(new ChatUser(item));
            }
        }

        private static void _addRoster(IChatUser user)
        {
            if (!_roster.ContainsKey(user.Bare))
                _roster.Add(user.Bare, user);
        }

        #endregion

        #region Message Senders

        private void SendMessage(string text, string jid, BotMessageType messageType)
        {
            //msg.from or jid
            if (!jid.Contains("@"))
                jid = jid + "@" + _config.Server;

            SendMessage(new Jid(jid), text, (MessageType)messageType);
        }

        private void SendMessage(Jid to, string text, MessageType type)
        {
            if (text == null) return;

            _client.Send(new Message(to, type, text));
        }

        #endregion

        #region Plugin Management

        [ImportMany(AllowRecomposition = true)]
        public static IEnumerable<IXmppBotPlugin> Plugins { get; set; }

        private string LoadPlugins()
        {
            var container = new CompositionContainer(_catalog);
            Plugins = container.GetExportedValues<IXmppBotPlugin>();

            foreach (IXmppBotPlugin plugin in Plugins)
            {
                // wire up message send event
                plugin.SentMessage += new PluginMessageHandler(SendMessage);

                // wire up plugin init
                plugin.Initialize();
            }


            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Loaded the following plugins");
            foreach (var part in _catalog.Parts)
            {
                builder.AppendFormat("\t{0}\n", part.ToString());
            }

            return builder.ToString();
        }

        #endregion

    }
}