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
            if (IssueRefRegex.Match(message) is Match match)
            {
                refNums = from grp in match.Groups as IEnumerable<Group>
                    select int.Parse(grp.Value);
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