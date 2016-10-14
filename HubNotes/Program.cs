namespace HubNotes
{
    using Octokit;
    using System;
    using System.Threading.Tasks;
    using Nito.AsyncEx;

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return AsyncContext.Run(() => MainAsync(args));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        static async Task<int> MainAsync(string[] args)
        {
            var owner = args[0];
            var name = args[1];
            var issueNumber = int.Parse(args[2]);

            var client = new GitHubClient(new ProductHeaderValue("HubNotes"));
            var releaseNotes = new ReleaseNotes(client);
            var md = await releaseNotes.GetReleaseNotes(owner, name, issueNumber);
            Console.WriteLine(md);
            return 0;
        }
    }
}
