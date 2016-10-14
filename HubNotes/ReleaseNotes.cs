namespace HubNotes
{
    using Octokit;
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Nito.AsyncEx;

    public class ReleaseNotes
    {
        GitHubClient client;

        public ReleaseNotes(GitHubClient client)
        {
            this.client = client;
        }

        public async Task<Issue> RefreshReleaseNotes(string owner, string name, int issueNumber)
        {
            var notesIssue = await client.Issue.Get(owner, name, issueNumber);
            var md = await GetReleaseNotes(owner, name, issueNumber);

            var issueUpdate = new IssueUpdate
            {
                Body = md,
                Milestone = notesIssue.Milestone.Number
            };

            return await client.Issue.Update(owner, name, issueNumber, issueUpdate);
        }

        public async Task<string> GetHtmlReleaseNotes(string owner, string name, int issueNumber)
        {
            var md = await GetReleaseNotes(owner, name, issueNumber);
            md = md.Replace("###", "##"); // Html notes use <h2> not <h3>.
            var arb = new NewArbitraryMarkdown(md, "gfm");
            var html = await client.Miscellaneous.RenderArbitraryMarkdown(arb);
            return html;
        }

        public async Task<string> GetReleaseNotes(string owner, string name, int issueNumber)
        {
            var repository = await client.Repository.Get(owner, name);
            var notesIssue = await client.Issue.Get(owner, name, issueNumber);
            var issues = await GetIssuesByMilestone(repository, notesIssue.Milestone);
            return getReleaseNotes(repository, notesIssue, issues);
        }

        static string getReleaseNotes(Repository repository, Issue notesIssue, IReadOnlyList<Issue> issues)
        {
            if (string.IsNullOrEmpty(notesIssue.Body))
            {
                return createReleaseNotes(repository, notesIssue, issues);
            }

            return updateReleaseNotes(repository, notesIssue, issues);
        }

        static string updateReleaseNotes(Repository repository, Issue notesIssue, IReadOnlyList<Issue> issues)
        {
            var md = "";
            var reader = new StringReader(notesIssue.Body);
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                var issueUrl = findIssueUrl(repository.HtmlUrl, line);
                if (issueUrl == null)
                {
                    continue;
                }

                var issueUri = new Uri(issueUrl);
                var issue = issues.Where(i => i.HtmlUrl == issueUri).Last();
                var previousNote = reader.ReadLine();

                var heading = formatIssue(issue);
                var note = findNote(issue) ?? previousNote;
                md += heading + "\n";
                md += note + "\n";
                md += "\n";
            }

            return md;
        }

        static string findIssueUrl(string repositoryUrl, string line)
        {
            var pattern = "[(](" + repositoryUrl + "/issues/[0-9]+" + ")[)]";

            var match = Regex.Match(line, pattern);
            if (!match.Success)
            {
                return null;
            }

            return match.Groups[1].Value;
        }

        static string createReleaseNotes(Repository repository, Issue notesIssue, IReadOnlyList<Issue> issues)
        {
            var md = "";

            foreach (var issue in issues)
            {
                var heading = formatIssue(issue);
                var note = findNote(issue) ?? "TODO: Add description.";
                md += heading + "\n";
                md += note + "\n";
                md += "\n";
            }

            return md;
        }

        static string formatIssue(Issue issue)
        {
            return string.Format("### [TDI{0}]({1}): {2}", issue.Number, issue.HtmlUrl, issue.Title);
        }

        static string findNote(Issue issue)
        {
            var prefix = "NOTE: ";
            var reader = new StringReader(issue.Body);
            var line = reader.ReadLine();
            if (line == null || !line.StartsWith(prefix))
            {
                return null;
            }

            return line.Substring(prefix.Length);
        }

        async Task<IReadOnlyList<Issue>> GetIssuesByMilestone(Repository repository, string title)
        {
            var milestone = await GetMilestoneByTitle(repository, title);
            return await GetIssuesByMilestone(repository, milestone);
        }

        async Task<IReadOnlyList<Issue>> GetIssuesByMilestone(Repository repository, Milestone milestone)
        {
            var milestoneRequest = new RepositoryIssueRequest
            {
                Milestone = milestone.Number.ToString(),
                State = ItemStateFilter.All
            };

            return await client.Issue.GetAllForRepository(repository.Id, milestoneRequest);
        }

        async Task<Milestone> GetMilestoneByTitle(Repository repository, string title)
        {
            var id = repository.Id;
            var milestones = await client.Issue.Milestone.GetAllForRepository(id);
            return milestones.Where(m => m.Title == title).First();
        }
    }
}
