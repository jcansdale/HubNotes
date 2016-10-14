namespace HubNotes.Tests
{
    using System;
    using System.Threading.Tasks;
    using Octokit;
    using NUnit.Framework;

    [Explicit("IntegrationTests")]
    public class ReleaseNotesIntegrationTests
    {
        [Test]
        public async Task GetReleaseNotes()
        {
            var owner = "jcansdale";
            var name = "TestDriven.Net-Issues";
            var issueNumber = 61;
            var client = getClient();
            var releaseNotes = new ReleaseNotes(client);

            var md = await releaseNotes.GetReleaseNotes(owner, name, issueNumber);

            Console.WriteLine(md);
        }

        [Test]
        public async Task RefreshReleaseNotes()
        {
            var owner = "jcansdale";
            var name = "TestDriven.Net-Issues";
            var issueNumber = 61;
            var client = getClient();
            var releaseNotes = new ReleaseNotes(client);

            var issue = await releaseNotes.RefreshReleaseNotes(owner, name, issueNumber);

            Console.WriteLine(issue.Body);
        }

        static GitHubClient getClient()
        {
            var appName = "HubNotes";
            var login = Environment.GetEnvironmentVariable("GitHub_Login");
            var password = Environment.GetEnvironmentVariable("GitHub_Password");
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
