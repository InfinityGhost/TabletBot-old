using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Octokit;
using TabletBot.Discord.Commands;

namespace TabletBot.Discord.Watchers.GitHub
{
    public class CodeMessageWatcher : IMessageWatcher
    {
        private readonly GitHubClient _client;

        public CodeMessageWatcher(GitHubClient client)
        {
            _client = client;
        }

        private static readonly Regex CodeRefRegex = new Regex(
            @"https:\/\/github.com\/(?<Owner>.+?)\/(?<Repo>.+?)\/blob\/(?<GitRef>.+?)\/(?<Path>.+?)#L(?<StartLine>.+?)(?:-L(?<EndLine>.+?))?($| |>)",
            RegexOptions.Compiled
        );

        public async Task Receive(IMessage message)
        {
            var match = CodeRefRegex.Match(message.Content);
            if (message is IUserMessage userMessage && match.Success)
            {
                using (message.Channel.EnterTypingState())
                {
                    var owner = match.Groups["Owner"].Value;
                    var repo = match.Groups["Repo"].Value;
                    var gitRef = match.Groups["GitRef"].Value;
                    var path = match.Groups["Path"].Value;
                    var startLine = int.Parse(match.Groups["StartLine"].Value) - 1;
                    var endLine = match.Groups["EndLine"].Success ? int.Parse(match.Groups["EndLine"].Value) : startLine + 1;

                    var url = $"https://raw.githubusercontent.com/{owner}/{repo}/{gitRef}/{path}";
                    var rawContent = await _client.Connection.GetHtml(new Uri(url));

                    var fileContent = rawContent.Body.Split(Environment.NewLine);
                    var lines = fileContent[startLine..endLine];

                    var extension = Path.GetExtension(path).Replace(".", string.Empty);

                    var sb = new StringBuilder();
                    sb.AppendLine(Formatting.CodeString(path));
                    sb.AppendCodeBlock(lines, extension);

                    var text = sb.ToString();
                    if (text.Length <= 2000)
                        await message.Channel.SendMessageAsync(text, messageReference: userMessage.ToReference());
                }
            }
        }

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) =>
            Task.CompletedTask;
    }
}