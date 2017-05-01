﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
	public class Questions
	{
		public static Dictionary<string, QuestionType> QuestionList;
        public static Dictionary<string, QuestionType> HelpList;

        public Questions()
		{
			Questions.QuestionList = Questions.QuestionList ?? this.GetQuestionsList();
            Questions.HelpList = Questions.HelpList ?? this.GetHelpList();
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
                { "help", QuestionType.CommodityPrice },
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
				{ "cpmpare ", QuestionType.CommodityPrice }
				
                
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
            ReminderHelp
		}
	}
}
