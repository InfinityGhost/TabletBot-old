using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Octokit;

namespace TabletBot.GitHub
{
    public static class GitHubTools
    {
        public static bool TryGetIssueRefNumbers(string message, out IEnumerable<int> refNums)
        {
            var matches = IssueRefRegex.Matches(message);
            if (matches.Count > 0)
            {
                refNums = from match in matches as IEnumerable<Match>
                    select int.Parse(match.Groups[1].Value);
                return true;
            }
            else
            {
                refNums = new int[0];
                return false;
            }
        }

        private static readonly Regex IssueRefRegex = new Regex(@" ?#([0-9]+[0-9]) ?");
    }
}