using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public Dictionary<QuestionType, string> ParseLine(string command, string line)
        {
            Dictionary<QuestionType, string> questionTypes;
            QuestionType questionType;
            string lower = line;
            Dictionary<QuestionType, string> questionParsed = new Dictionary<QuestionType, string>();
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
                        else if (questionNumberInt == 3)
                        {
                            questionType = QuestionType.Weather;
                        }
                        else if (questionNumberInt == 4)
                        {
                            questionType = QuestionType.MortgageInfo;
                        }
                        else
                        {
                            questionType = (questionNumberInt == 5 ? QuestionType.CommodityPrice : QuestionType.UnknownQuestionType);
                        }
                        questionParsed.Add(questionType, variable);
                        questionTypes = questionParsed;
                        return questionTypes;
                    }
                }
            }
            Regex regex = new Regex("[ ]{2,}", options);
            lower = regex.Replace(lower, " ");
            lower = Regex.Replace(lower, "\\bis\\b", "", RegexOptions.IgnoreCase).Replace("get all", "").Replace("get ", "").Replace(" in", "").Replace(" the", "").Replace(" are", "").Replace("?", "").Replace(" de", "").Replace(" on", "").Replace(" for", "").Replace(".", "").Replace("  ", " ").ToLower();
            Questions questions = new Questions();
            KeyValuePair<string, QuestionType> matchingQuestion = Questions.QuestionList.FirstOrDefault<KeyValuePair<string, QuestionType>>((KeyValuePair<string, QuestionType> x) => lower.Contains(x.Key.ToLower()));
            string questionVariable = lower.Replace(matchingQuestion.Key, "").Trim();
            if (matchingQuestion.Value != QuestionType.MortgageCalculation)
            {
                questionVariable = lower.Replace(matchingQuestion.Key, "").Trim().Split(new char[] { ' ' }).Last<string>();
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
    }
}
