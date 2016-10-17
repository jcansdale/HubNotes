namespace HubNotes.Tests
{
    using System;
    using System.Threading.Tasks;
    using Octokit;
    using NUnit.Framework;

    [Explicit("IntegrationTests")]
    public class ReleaseNotesIntegrationTests
    {
        // NOTE: GetReleaseNotes() still works if these aren't set.
        string login = Environment.GetEnvironmentVariable("GitHub_Login");
        string password = Environment.GetEnvironmentVariable("GitHub_Password");

        string owner = "jcansdale";
        string name = "TestDriven.Net-Issues";
        int issueNumber = 61;

        [Test]
        public async Task GetReleaseNotes()
        {
            var client = getClient();
            var releaseNotes = new ReleaseNotes(client);

            var md = await releaseNotes.GetReleaseNotes(owner, name, issueNumber);

            Console.WriteLine(md);
        }

        [Test]
        public async Task RefreshReleaseNotes()
        {
            var client = getClient();
            var releaseNotes = new ReleaseNotes(client);

            var issue = await releaseNotes.RefreshReleaseNotes(owner, name, issueNumber);

            Console.WriteLine(issue.Body);
        }

        GitHubClient getClient()
        {
            var appName = "HubNotes";
            if (login != null && password != null)
            {
                return new GitHubClient(new ProductHeaderValue(appName))
                {
                    Credentials = new Credentials(login, password)
                };
            }

            return new GitHubClient(new ProductHeaderValue(appName));
        }
    }
}
