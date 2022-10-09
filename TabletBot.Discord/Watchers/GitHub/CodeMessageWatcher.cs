using System;
using System.Collections.Generic;
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
            @"https:\/\/github.com\/(?<Owner>.+?)\/(?<Repo>.+?)\/blob\/(?<GitRef>.+?)\/(?<Path>.+?)(?:\?.+?)?#L(?<StartLine>[0-9]+[0-9]?)(?:-L(?<EndLine>[0-9]+[0-9]?))",
            RegexOptions.Compiled
        );

        public async Task Receive(IMessage message)
        {
            if (message is not IUserMessage userMessage)
                return;

            var messageReference = userMessage.ToReference();
            var blocks = GetCodeBlocks(message.Content);

            using (message.Channel.EnterTypingState())
                await foreach (var block in blocks.Take(3))
                    await message.Channel.SendMessageAsync(block, messageReference: messageReference);
        }

        private async IAsyncEnumerable<string> GetCodeBlocks(string content)
        {
            foreach (Match match in CodeRefRegex.Matches(content))
                yield return await GetFileContents(match);
        }

        private async Task<string> GetFileContents(Match match)
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

            // Clip to Discord's character limit of 2000
            return sb.ToString(0, 2000);
        }
        
        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel) => Task.CompletedTask;
    }
}
