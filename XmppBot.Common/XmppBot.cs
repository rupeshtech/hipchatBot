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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
//using System.Timers;
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
        private Timer _schedular;
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
            ScheduleService();
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
                //mucManager.GrantVoice(jid,, "testvoice");
            }
        }
        //public System.Timers.Timer myTimer;
        public void ScheduleService()
        {
            try
            {
                //myTimer = new System.Timers.Timer(1000); //one hour in milliseconds
                //myTimer.Elapsed += new ElapsedEventHandler(SchedularCallback);
                TimeSpan timeSpan;
                int currentMinute = DateTime.Now.Minute;
                currentMinute = currentMinute == 60 ? 0 : currentMinute;
                log.Info($"going to run after {60-currentMinute} minutes.");
                var isTestRunTimeInterval = Convert.ToBoolean(ConfigurationManager.AppSettings["RunTimeInterval"]);
                _schedular = new Timer(new TimerCallback(SchedularCallback));
                if(isTestRunTimeInterval)
                timeSpan = DateTime.Now.AddSeconds(10).Subtract(DateTime.Now);
                else
                    timeSpan = DateTime.Now.AddMinutes(60-currentMinute).Subtract(DateTime.Now);
                var dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                _schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
            }
        }

        private void SchedularCallback(object e)
        {
            GetRemindersAndSendMessage();
            //Jid botUser = new Jid("94767_4717350@chat.hipchat.com");
            //xmpp_OnMessage(null, new Message { Body="Remember to fill hours",From=botUser,Type= MessageType.chat});
            ScheduleService();
        }

        private void GetRemindersAndSendMessage()
        {
            var reminders = GetReminder();
            reminders.Add(new Reminder());
            foreach(var reminder in reminders)
            {
                SendMessageToUser(reminder);
            }
        }

        private void SendMessageToUser(Reminder reminder)
        {
            reminder.UserId = "94767_dha";
            reminder.ReminderText = "(standup)";
            Jid botUser = new Jid($"{reminder.UserId}@chat.hipchat.com");
            xmpp_OnMessage(null, new Message { Body = $"Your Reminder: { reminder.ReminderText}", From = botUser, Type = MessageType.groupchat });
        }

        private List<Reminder> GetReminder()
        {
            var parser = new LineParser();
            return parser.GetReminders();
        }

        private void xmpp_OnMessage(object sender, Message msg)
         {
            if (!string.IsNullOrEmpty(msg.Body))
            {
                log.InfoFormat("Message : {0} - from {1}", msg.Body, msg.From);

                IChatUser user = null;

                if (msg.Type == MessageType.groupchat)
                {
                    msg.From.Resource = "test";
                    msg.From.User = "757630";
                    var t = _roster.Values.OrderByDescending(x => x.Name);
                    user = _roster.Values.FirstOrDefault(u => u.Name == msg.From.Resource);
                }
                else
                {
                    if (msg.Body.Contains("Your Reminder:"))
                        //user = new ChatUser { Id="94767_4717350",Bare= "94767_4717350@chat.hiptchat.com" };
                        user = new ChatUser { Id = msg.From.User, Bare = msg.From.Bare };
                    else
                        _roster.TryGetValue(msg.From.Bare, out user);
                }

                // we can't find a user or this is the bot talking
                if (null == user || _config.RoomNick == user.Name)
                    return;
                ParsedLine line;
                if (msg.Body.Contains("Your Reminder:"))
                    line = new ParsedLine(msg.From.Bare, msg.Body.Trim(), msg.From.User, user, (BotMessageType)msg.Type);
                else
                    line = new ParsedLine(msg.From.Bare, msg.Body.Trim(), msg.From.User, user, (BotMessageType)msg.Type);
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
                    SendMessage(msg.From, $"I am here to help you. As of now I have learnt to answer following questions?", msg.Type);
                    var initalText = new StringBuilder();
                    initalText.AppendLine($"/code {Environment.NewLine}1) Room avalibility.Please type: '1-Amsterdam' or 'Availabe room in Amsterdam?'");
                    initalText.AppendLine( $"2) Depster info or Room info.Please type: '2-Rupesh' or '2- big room 1' or 'Where is Rupesh' or 'What is Rupesh doing?'");
                    //initalText.AppendLine( $"3) Weather info.Please type: '3-London' or 'Weather in Washinton?'");
                    initalText.AppendLine($"3) Set\\Delete reminder.Please type: set reminder-10-daily-Standup meeting");
                    initalText.AppendLine( $"4) Grocery info.Please type: '4-Heineken' or 'Compare price Bier?' or 'Prijs Heineken?'");
                    initalText.AppendLine($"5) Mortgage info.Please type: '5-MortageInfo' or 'How much can I take loan?'");
                    SendMessage(msg.From, $"{initalText}", msg.Type);
                }
                else if(!line.Command.Contains("<p>"))
                {
                    try
                    {
                       // _schedular = new Timer(new TimerCallback(SchedularCallback));
                        var parser = new LineParser();
                        if (msg.Body.Contains("Your Reminder:")) {
                            Dictionary<QuestionType, string> questionTypes= new Dictionary<QuestionType, string>();
                            questionTypes.Add(QuestionType.ReminderHour, msg.Body);
                            lineParsed = questionTypes;
                    }
                        else
                            lineParsed = parser.ParseLine(line.Command, line.Raw,user.Id,user.Name);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                        SendMessage(msg.From, $"Sorry i couldn't understand your question.You can ask me following questions", msg.Type);
                        var initalTexttt = new StringBuilder();
                        initalTexttt.AppendLine($"/code {Environment.NewLine}1) Room avalibility.Please type: '1-Amsterdam' or 'Availabe room in Amsterdam?'");
                        initalTexttt.AppendLine($"2) Depster info or Room info.Please type: '2-Rupesh' or '2- big room 1' or 'Where is Rupesh' or 'What is Rupesh doing?'");
                        //initalTexttt.AppendLine($"3) Weather info.Please type: '3-London' or 'Weather in Washinton?'");
                        initalTexttt.AppendLine($"3) Set\\Delete reminder.Please type: set reminder-10-daily-Standup meeting");
                        initalTexttt.AppendLine($"4) Grocery info.Please type: '4-Heineken' or 'Compare price Bier?' or 'Prijs Heineken?'");
                        initalTexttt.AppendLine($"5) Mortgage info.Please type: '5-MortageInfo' or 'How much can I take loan?'");
                        SendMessage(msg.From, $"{initalTexttt}", msg.Type);
                    }
                }
                var answer = new Answer();
                if (lineParsed !=null && !line.Command.Contains("<p>"))
                {
                    switch(lineParsed.First().Key)
                    {
                        case QuestionType.Weather:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var answerWeather= answer.GetAnswer(QuestionType.Weather, lineParsed.First().Value);
                            SendMessage(msg.From, $"{answerWeather}", msg.Type);
                            break;
                        case QuestionType.Help:
                            var help = new StringBuilder();
                            SendMessage(msg.From, $"Please type one of the following to see examples", msg.Type);
                            help.AppendLine($"/code {Environment.NewLine} help room {Environment.NewLine} help person {Environment.NewLine} help reminder");
                            SendMessage(msg.From, $"{help}", msg.Type);
                            //SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/bot_individual.png", msg.Type);
                            //SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/bot_room.png", msg.Type);
                            //SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/reminder.png", msg.Type);
                            break;
                        case QuestionType.IndividualHelp:
                            SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/bot_individual.png", msg.Type);
                            break;
                        case QuestionType.IndividualProfile:
                            var depstersProfile = answer.GetAnswer(QuestionType.IndividualProfile, lineParsed.First().Value);
                            var depsters = JsonConvert.DeserializeObject<List<Depster>>(depstersProfile);
                            if(depsters==null || depsters.Count==0)
                                SendMessage(msg.From, $"No {lineParsed.First().Value} found", msg.Type);
                            if (depsters.Count > 2)
                            {
                                var individualProfileText = new StringBuilder();
                                individualProfileText.AppendLine($"/code {Environment.NewLine}. More than one {lineParsed.First().Value}. {Environment.NewLine} {String.Join(";", depsters.Select(x => $"{x.Given_Name} {x.Family_Name}").ToList())}");
                                SendMessage(msg.From, $"{individualProfileText}", msg.Type);
                            }
                            if (depsters.Count > 0 && depsters.Count < 3)
                            {
                                foreach (var depster in depsters)
                                {
                                    var individualProfile = new StringBuilder();
                                    individualProfile.AppendLine($"/code {Environment.NewLine} {depster.Given_Name} {depster.Family_Name} {Environment.NewLine} {depster.Organization_Name} {Environment.NewLine}{depster.Phone_Value} {Environment.NewLine}{depster.E_mail_Value}");
                                    SendMessage(msg.From, $"{individualProfile}", msg.Type);
                                }
                            }
                            break;
                        case QuestionType.RoomHelp:
                            SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/bot_room.png", msg.Type);
                            break;
                        case QuestionType.ReminderHelp:
                            SendMessage(msg.From, $"http://helpdeptbot.azurewebsites.net/reminder.png", msg.Type);
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
                            var roomQueryAnswer = answer.GetAnswer(QuestionType.RoomQuery, lineParsed.First().Value);
                            if(roomQueryAnswer.Contains("Please specify office location"))
                            {
                                SendMessage(msg.From, $"{roomQueryAnswer}", msg.Type);
                                break;
                            }
                            var helpTexttt = new StringBuilder();
                            var roomStatus = JsonConvert.DeserializeObject<RoomsStatus>(roomQueryAnswer);
                            if (roomStatus.Rooms == null || roomStatus.Rooms.Count == 0 || roomStatus.Rooms.All(x => string.IsNullOrEmpty(x.Name)))
                                helpTexttt.AppendLine($"/code {Environment.NewLine}No room available now at {lineParsed.First().Value}");
                            else
                            {
                                SendMessage(msg.From, $"Available rooms in {lineParsed.First().Value} for next {roomStatus.Minutes} minutes are", msg.Type);
                                helpTexttt.AppendLine($"/code");
                                foreach (var room in roomStatus.Rooms)
                                    helpTexttt.AppendLine($"{room.Name}");
                            }
                            SendMessage(msg.From, $"{helpTexttt}", msg.Type);
                            break;
                        case QuestionType.FindIndividualQuery:
                            //byte[] image = 
                            
                            var individualQuery = answer.GetAnswer(QuestionType.FindIndividualQuery, lineParsed.First().Value);
                            var helpTextt = new StringBuilder();
                            if (individualQuery.Contains("More than one"))
                            {
                                var list = individualQuery.Substring(individualQuery.IndexOf("found.")+6);
                                var foundList = list.Split(',');
                                helpTextt.AppendLine($"/code {Environment.NewLine}More than one {new CultureInfo("en-US").TextInfo.ToTitleCase(lineParsed.First().Value)} found. Who are you l0oking for {list}. Please type full name. for ex: 2- sandor voordes");
                                SendMessage(msg.From, $"{helpTextt}", msg.Type);
                                break;
                            }
                            if (individualQuery.Contains("Couldn't find any"))
                            {
                                helpTextt.AppendLine($"/code {Environment.NewLine} {new CultureInfo("en-US").TextInfo.ToTitleCase(individualQuery)} ");
                                SendMessage(msg.From, $"{helpTextt}", msg.Type);
                                break;
                            }
                            var individualInfo = JsonConvert.DeserializeObject<Individual>(individualQuery);

                            if (individualInfo.IsBusyNow)
                            {
                                helpTextt.AppendLine($"/code {Environment.NewLine}{new CultureInfo("en-US").TextInfo.ToTitleCase(lineParsed.First().Value)} is busy now. Till {individualInfo.Busy_Till}");
                                if(individualInfo.RoomName !=null)
                                    helpTextt.Append($" In {individualInfo.RoomName}");
                                if (individualInfo.BusyWith !=null && !individualInfo.BusyWith.Contains("more than"))
                                    helpTextt.Append($" With {individualInfo.BusyWith}");
                            }
                            else
                            {
                                if (!individualInfo.IsRoom)
                                    helpTextt.AppendLine($"/code {Environment.NewLine}{new CultureInfo("en-US").TextInfo.ToTitleCase(lineParsed.First().Value)} is free now. His\\Her next meeting is {individualInfo.Events.FirstOrDefault(x=>x.Busy_From.Value.TimeOfDay.ToString() != "12:00:00 AM").Busy_From}  at {individualInfo.Events.FirstOrDefault().RoomName}");
                                else
                                    helpTextt.AppendLine($"/code {Environment.NewLine}{new CultureInfo("en-US").TextInfo.ToTitleCase(lineParsed.First().Value)} is free now. Next meeting from {individualInfo.Events.FirstOrDefault().Busy_From}");
                            }
                            SendMessage(msg.From, $"{helpTextt}", msg.Type);
                            break;
                        case QuestionType.JiraIssue:
                            var jiraQuery = answer.GetAnswer(QuestionType.JiraIssue, $"{user.Name}-{lineParsed.First().Value}");
                            var issues = JsonConvert.DeserializeObject<List<Issue_>>(jiraQuery);
                            var jiraText = new StringBuilder();
                            //jiraText.AppendLine($"/code");
                            foreach(var issue in issues)
                            jiraText.AppendLine($"{issue.key}. url is {issue.IssueUrl}");
                            SendMessage(msg.From, $"{jiraText}", msg.Type);
                            break;
                        case QuestionType.ReminderHour:
                            SendMessage(msg.From, $"{msg.Body}", msg.Type);
                            break;
                        case QuestionType.SetReminder:
                            SendMessage(msg.From, $"{lineParsed.First().Value}", msg.Type);
                            break;
                        case QuestionType.CommodityPrice:
                            //SendMessage(msg.From, $"You asked: {msg.Body.Trim()}.", msg.Type);
                            var priceQuery = answer.GetAnswer(QuestionType.CommodityPrice, lineParsed.First().Value);
                            var products = JsonConvert.DeserializeObject<List<Product>>(priceQuery);
                            if (products.Count == 0) {
                                SendMessage(msg.From, $" Sorry currently i can't find price of {lineParsed.First().Value}!. Please try dutch names.!", msg.Type);
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
                            var initalTextt = new StringBuilder();
                            initalTextt.AppendLine($"/code {Environment.NewLine}1) Room avalibility.Please type: '1-Amsterdam' or 'Availabe room in Amsterdam?'");
                            initalTextt.AppendLine($"2) Depster info or Room info.Please type: '2-Rupesh' or '2- big room 1' or 'Where is Rupesh' or 'What is Rupesh doing?'");
                            //initalTextt.AppendLine($"3) Weather info.Please type: '3-London' or 'Weather in Washinton?'");
                            initalTextt.AppendLine($"3) Set\\Delete reminder.Please type: set reminder-10-daily-Standup meeting");
                            initalTextt.AppendLine($"4) Grocery info.Please type: '4-Heineken' or 'Compare price Bier?' or 'Prijs Heineken?'");
                            initalTextt.AppendLine($"5) Mortgage info.Please type: '5-MortageInfo' or 'How much can I take loan?'");

                            SendMessage(msg.From, $"{initalTextt}", msg.Type);
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