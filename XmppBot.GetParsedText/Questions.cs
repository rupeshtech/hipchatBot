using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
	public class Questions
	{
		public static Dictionary<string, QuestionType> QuestionList;
        public static Dictionary<string, QuestionType> HelpList;
        public static Dictionary<string, string> MatchingWords;

        public Questions()
		{
			Questions.QuestionList = Questions.QuestionList ?? this.GetQuestionsList();
            Questions.HelpList = Questions.HelpList ?? this.GetHelpList();
            Questions.MatchingWords = Questions.MatchingWords ?? this.GetMatchingWords();
        }

        private Dictionary<string, string> GetMatchingWords()
        {
            Dictionary<string, string> matchingWords = new Dictionary<string, string>()
            {
                { "today", "today" },
                { "2day", "today" },
                { "tody", "today" },
                { "tday", "today" },
                { "vandaag", "today" },
                { "vandag", "today" },
                { "toda", "today" },
                { "tomorrow", "tomorrow" },
                { "tomorow", "tomorrow" },
                { "2morrow", "tomorrow" },
                { "tmorrow", "tomorrow" },
                { "tomorrw", "tomorrow" },
                { "tmorow", "tomorrow" },
                { "morgen", "tomorrow" },
                { "monday", "monday" },
                { "mnday", "monday" },
                { "maandag", "monday" },
                { "mandag", "monday" },
                { "tuesday", "tuesday" },
                { "dinsdag", "tuesday" },
                { "", "no match" }

            };
            return matchingWords;
        }

        private Dictionary<string, QuestionType> GetHelpList()
        {
            Dictionary<string, QuestionType> questionsList = new Dictionary<string, QuestionType>()
            {
                { "room", QuestionType.RoomHelp },
                { "Rooterdam", QuestionType.RoomHelp },
                { "Amsterdam", QuestionType.RoomHelp },
                { "Manchester", QuestionType.RoomHelp },
                { "kamer", QuestionType.RoomHelp },
                { "free", QuestionType.RoomHelp },
                { "individual", QuestionType.IndividualHelp },
                { "person", QuestionType.IndividualHelp },
                { "find", QuestionType.IndividualHelp },
                { "calendar", QuestionType.IndividualHelp },
                { "reminder", QuestionType.ReminderHelp },
                { "remindr", QuestionType.ReminderHelp },
                { "reminde", QuestionType.ReminderHelp },
                { "reminer", QuestionType.ReminderHelp },
                { "rminder", QuestionType.ReminderHelp },
                { "", QuestionType.Help }

            };
            return questionsList;
        }

        private Dictionary<string, QuestionType> GetQuestionsList()
		{
			Dictionary<string, QuestionType> questionsList = new Dictionary<string, QuestionType>()
			{
                { "help", QuestionType.Help },
                { "weather", QuestionType.Weather },
				{ "wather", QuestionType.Weather },
				{ "wether", QuestionType.Weather },
				{ "weathe", QuestionType.Weather },
				{ "weathr", QuestionType.Weather },
				{ "weer", QuestionType.Weather },
				{ "mortgage", QuestionType.MortgageInfo },
				{ "Mortgag", QuestionType.MortgageInfo },
				{ "lenen", QuestionType.MortgageInfo },
				{ "loan", QuestionType.MortgageInfo },
				{ "free", QuestionType.RoomQuery },
				{ "available", QuestionType.RoomQuery },
				{ "vacant", QuestionType.RoomQuery },
				{ "salary", QuestionType.MortgageCalculation },
				{ "salry", QuestionType.MortgageCalculation },
				{ "salay", QuestionType.MortgageCalculation },
				{ "salar", QuestionType.MortgageCalculation },
				{ "age", QuestionType.MortgageCalculation },
				{ "salaris", QuestionType.MortgageCalculation },
				{ "salris", QuestionType.MortgageCalculation },
				{ "salars", QuestionType.MortgageCalculation },
				{ "busy", QuestionType.FindIndividualQuery },
				{ "doing", QuestionType.FindIndividualQuery },
                { "where", QuestionType.FindIndividualQuery },
                { "waar", QuestionType.FindIndividualQuery },
                { "busy now", QuestionType.FindIndividualQuery },
				{ "doing now", QuestionType.FindIndividualQuery },
				{ "jira issue", QuestionType.JiraIssue },
				{ "jira issues", QuestionType.JiraIssue },
				{ "issue in jira", QuestionType.JiraIssue },
				{ "issues in jira", QuestionType.JiraIssue },
				{ "jiraissue", QuestionType.JiraIssue },
				{ "jiraissues", QuestionType.JiraIssue },
				{ "jira assigned", QuestionType.JiraIssue },
				{ "jira task", QuestionType.JiraIssue },
				{ "price compare", QuestionType.CommodityPrice },
				{ "price vergelijk", QuestionType.CommodityPrice },
				{ "prijs compare", QuestionType.CommodityPrice },
				{ "rpijs vergelijk", QuestionType.CommodityPrice },
				{ "compare price", QuestionType.CommodityPrice },
				{ "vergelijk price ", QuestionType.CommodityPrice },
				{ "compare prijs", QuestionType.CommodityPrice },
				{ "vergelijk prijs", QuestionType.CommodityPrice },
				{ "price ", QuestionType.CommodityPrice },
				{ "prijs", QuestionType.CommodityPrice },
				{ "cpmpare ", QuestionType.CommodityPrice },
                { "help ", QuestionType.Help }


            };
			return questionsList;
		}
		public enum QuestionType
		{
			UnknownQuestionType,
			Weather,
			MortgageInfo,
			MortgageCalculation,
			RoomQuery,
			FindIndividualQuery,
			JiraIssue,
			CommodityPrice,
            ReminderHour,
            SetReminder,
            Help,
            RoomHelp,
            IndividualHelp,
            ReminderHelp,
            IndividualProfile
		}
	}
}
