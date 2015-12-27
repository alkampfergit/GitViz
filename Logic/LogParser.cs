using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitViz.Logic
{
    public class LogParser
    {
        public readonly string ExpectedOutputFormat = "%ct %H %P %d %s";

        public IEnumerable<Commit> ParseCommits(StreamReader gitLogOutput)
        {
            while (gitLogOutput.BaseStream != null && !gitLogOutput.EndOfStream)
            {
                var line = gitLogOutput.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                yield return ParseCommit(line);
            }
            gitLogOutput.Close();
        }

        static readonly Regex ParseCommitRegex = new Regex(@"^(?<commitDate>\d*) (?<hash>\w{7,40})(?<parentHashes>( \w{7,40})+)?([ ]+\((?<refs>.*?)\))?\s?((?<subject>.*))?");

        internal static Commit ParseCommit(string logOutputLine)
        {
            var match = ParseCommitRegex.Match(logOutputLine.Trim());

            var commitDate = long.Parse(match.Groups["commitDate"].Value);

            var parentHashes = match.Groups["parentHashes"].Success
                ? match.Groups["parentHashes"].Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                : null;

            var refs = match.Groups["refs"].Success
                ? match.Groups["refs"]
                    .Value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(r => r.Trim())
                    .ToArray()
                : null;

            var subject = match.Groups["subject"].Success
                ? match.Groups["subject"].Value
                : null;

            return new Commit
            {
                Hash = match.Groups["hash"].Value,
                CommitDate = ConvertUnixTimeToLocalTime(commitDate),
                ParentHashes = parentHashes,
                Refs = refs,
                Subject = subject,
            };
        }

        private static DateTime ConvertUnixTimeToLocalTime(long unixTime)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTime).ToLocalTime();
        }
    }
}
