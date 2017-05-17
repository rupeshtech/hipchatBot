using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XmppBot.Log;
using static XmppBot.GetParsedText.Questions;

namespace XmppBot.GetParsedText
{
    public class LineParser
    {
        public KeyValuePair<string, QuestionType> QuestionsList
        {
            get;
            set;
        }

        public LineParser()
        {
        }

        public Dictionary<QuestionType, string> ParseLine(string command, string line, string userId, string userName=null)
        {
            line = line.ToLower();
            Dictionary<QuestionType, string> questionTypes;
            QuestionType questionType;
            string lower = line;
            Dictionary<QuestionType, string> questionParsed = new Dictionary<QuestionType, string>();
            if (line.Contains("set reminder") && !line.Contains("help"))
            {
                var setReminderStatus = UpsertReminder(line, userId,userName);
                questionParsed.Add(QuestionType.SetReminder,$"{setReminderStatus.Value}");
                return questionParsed;
            }
            if (line.ToLower().Contains("delete reminder"))
            {
                var deleteReminderStatus = DeleteReminder(line, userId);
                questionParsed.Add(QuestionType.SetReminder, $"{deleteReminderStatus.Value}");
                return questionParsed;
            }
            RegexOptions options = RegexOptions.None;
            if ((new Regex(string.Concat("(\\d+)\\s*", "(-)\\s*", "((?:[a-z][a-z]+))\\s*"), RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace)).IsMatch(lower))
            {
                string questionNumber = lower.Substring(0, lower.IndexOf("-"));
                string variable = lower.Substring(lower.IndexOf("-") + 1);
                variable = variable.Replace("?", "").Replace(".", "").Trim();
                int questionNumberInt = 0;
                if (int.TryParse(questionNumber, out questionNumberInt))
                {
                    if ((questionNumberInt <= 0 ? false : questionNumberInt < 8))
                    {
                        if (questionNumberInt == 1)
                        {
                            questionType = QuestionType.RoomQuery;
                        }
                        else if (questionNumberInt == 2)
                        {
                            questionType = QuestionType.FindIndividualQuery;
                        }
                        else if (questionNumberInt == 5)
                        {
                            questionType = QuestionType.MortgageInfo;
                        }
                        else if (questionNumberInt == 4)
                        {
                            questionType = QuestionType.CommodityPrice;
                        }
                        else if (questionNumberInt == 6)
                        {
                            questionType = QuestionType.IndividualProfile;
                        }
                        else
                        {
                            questionType = (questionNumberInt == 5 ? QuestionType.MortgageInfo : QuestionType.UnknownQuestionType);
                        }
                        questionParsed.Add(questionType, variable);
                        //questionTypes = questionParsed;
                        return questionParsed;
                    }
                }
            }
            Regex regex = new Regex("[ ]{2,}", options);
            lower = regex.Replace(lower, " ");
            lower = Regex.Replace(lower, "\\bis\\b", "", RegexOptions.IgnoreCase).Replace("get all", "").Replace("get ", "").Replace(" in", "").Replace(" the", "").Replace(" are", "").Replace("?", "").Replace(" de", "").Replace(" on", "").Replace(" for", "").Replace(".", "").Replace("  ", " ").ToLower();
            Questions questions = new Questions();
            KeyValuePair<string, QuestionType> matchingQuestion = Questions.QuestionList.FirstOrDefault<KeyValuePair<string, QuestionType>>((KeyValuePair<string, QuestionType> x) => lower.Contains(x.Key.ToLower()));
            string questionVariable = lower.Replace(matchingQuestion.Key, "").Trim();
            if (matchingQuestion.Value != QuestionType.MortgageCalculation && matchingQuestion.Value != QuestionType.Help)
            {
                if (matchingQuestion.Value == QuestionType.FindIndividualQuery || lower.Contains("what") || lower.Contains("wat") || lower.Contains("where")|| lower.Contains("waar"))
                {
                    lower = lower.ToLower().Replace("what","").Replace("wat", "").Replace("where", "").Replace("waar", "");
                    questionVariable = lower.Replace(matchingQuestion.Key, "").Trim();
                }
                else 
                    questionVariable = lower.Replace(matchingQuestion.Key, "").Trim().Split(new char[] { ' ' }).Last<string>();
            }
            else if(matchingQuestion.Value == QuestionType.Help)
            {
                KeyValuePair<string, QuestionType> matchingHelpQuestion = Questions.HelpList.FirstOrDefault<KeyValuePair<string, QuestionType>>((KeyValuePair<string, QuestionType> x) => lower.Contains(x.Key.ToLower()));
               questionParsed.Add(matchingHelpQuestion.Value, questionVariable);
               questionTypes = questionParsed;
               return questionTypes;
            }
            else 
            {
                IEnumerable<string> calculationVariables =
                    from x in questionVariable.Split(new char[] { ' ' })
                    where Regex.IsMatch(x, "\\d+")
                    select x;
                questionVariable = string.Format("{0} {1}", calculationVariables.First<string>(), calculationVariables.Last<string>());
            }
            questionParsed.Add(matchingQuestion.Value, questionVariable);
            questionTypes = questionParsed;
            return questionTypes;
        }

        private KeyValuePair<bool, string> DeleteReminder(string line, string userId)
        {
            var columns = line.Split('-');
            bool allReminderToDelete = columns[1].Trim().ToLower().Contains("all") ? true : false;
            try
            {
                using (var entities = new DeptBotEntities())
                {
                   if(allReminderToDelete)
                    {
                        var reminders = entities.Reminders.Where(x => x.UserId == userId);
                        entities.Reminders.RemoveRange(reminders);
                        entities.SaveChanges();
                    }
                   else
                    {
                        string runSchedule = columns[2].Trim();
                        int intHour = Convert.ToInt32(columns[1].Trim());
                        var existingRemider = entities.Reminders.FirstOrDefault(x => x.HourInt == intHour && x.UserId == userId && x.RunDay.ToLower() == runSchedule.ToLower());
                        if (existingRemider == null)
                            return new KeyValuePair<bool, string>(true, $"Nothing to delete");
                        else
                            entities.Reminders.Remove(existingRemider);
                        entities.SaveChanges();
                    }
                }
                return new KeyValuePair<bool, string>(true, $"{line} -success");
            }
            catch (Exception)
            {
                return new KeyValuePair<bool, string>(false, $"couldnot delete reminder");
            }
        }

        private KeyValuePair<bool,string> UpsertReminder(string line, string userId,string userName)
        {
            var columns = line.Split('-');
            int intHour = Convert.ToInt32(columns[1].Trim());
            string runSchedule = columns[2].Trim();
            string dateToSet = "";
            if(columns.Count()>5)
            {
                dateToSet = $"{columns[2]}-{columns[3]}-{columns[4]}";
            }
            DateTime reminderdateToSet;
            bool isDate=false;
            if(DateTime.TryParse(runSchedule,out reminderdateToSet) || (DateTime.TryParse(dateToSet, out reminderdateToSet)))
            {
                isDate = true;
            }
            string reminderMessage = columns.Last().Trim();
            try
            {
                if (isDate || runSchedule.ToLower() == "today" || runSchedule.ToLower() == "tomorrow")
                {
                    runSchedule = isDate ? reminderdateToSet.ToString() : runSchedule.ToLower() == "today" ? DateTime.Now.Date.ToString() : DateTime.Now.AddDays(1).Date.ToString();
                    var setRminder = SetReminder(userId, intHour, runSchedule, userName, reminderMessage);
                    return setRminder ? new KeyValuePair<bool, string>(true, $"{line} -success") : new KeyValuePair<bool, string>(false, $"Please check ur reminder days. Valida values are Today,Daily,Weekdays,weekends,Monday,Tuesday....Sunday");
                }
                else
                {
                    switch (runSchedule.ToLower())
                    {
                        case "monday":
                        case "tuesday":
                        case "wednesday":
                        case "thursday":
                        case "friday":
                        case "saturday":
                        case "sunday":
                        case "weekdays":
                        case "weekends":
                        case "daily":
                            var setRminder = SetReminder(userId, intHour, runSchedule, userName, reminderMessage);
                            return setRminder ? new KeyValuePair<bool, string>(true, $"{line} -success") : new KeyValuePair<bool, string>(false, $"Please check ur reminder days. Valida values are Today,Daily,Weekdays,weekends,Monday,Tuesday....Sunday");
                        default:
                            return new KeyValuePair<bool, string>(false, $"Please check ur reminder days. Valida values are Today,Daily,Weekdays,weekends,Monday,Tuesday....Sunday");
                    }
                }
                }
            catch (Exception)
            {
                return new KeyValuePair<bool, string>(false, $"couldnot set reminder for {intHour} - {runSchedule} for {reminderMessage}");
            }
        }

        private bool SetReminder(string userId, int intHour,string runSchedule, string userName,string reminderMessage)
        {
            try
            {
                using (var entities = new DeptBotEntities())
                {
                    var existingRemider = entities.Reminders.FirstOrDefault(x => x.HourInt == intHour && x.UserId == userId && x.RunDay.ToLower() == runSchedule.ToLower());

                    if (existingRemider == null)
                        entities.Reminders.Add(new Reminder { UserId = userId, UserName = userName, RunDay = runSchedule, HourInt = intHour, ReminderText = reminderMessage });
                    else
                        existingRemider.ReminderText = reminderMessage;
                    entities.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(JsonConvert.SerializeObject(ex));
                return false;
            }
        }

        public List<Reminder> GetReminders()
        {
            string weekDayOrWeekend = ((DateTime.Today.DayOfWeek== DayOfWeek.Saturday) || (DateTime.Today.DayOfWeek == DayOfWeek.Sunday))?"Weekend":"Weekdays";
            List<string> days = new List<string>();
            days.Add(DateTime.Now.Date.ToString().ToLower());
            days.Add("Daily".ToLower());
            days.Add("Today".ToLower());
            days.Add(weekDayOrWeekend.ToLower());
            days.Add(DateTime.Today.DayOfWeek.ToString().ToLower());
            using (var entities = new DeptBotEntities())
            {
                var reminders = entities.Reminders.Where(x=>x.HourInt== DateTime.Now.Hour && days.Contains(x.RunDay.ToLower())) .ToList();
                return reminders;
            }
        }
    }
}
