using System;
using System.IO;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LibGit2Sharp;
using System.Diagnostics;
using System.Management.Automation;

public class Settings
{
    public string Token { get; set; }
    public string RepositoryPath { get; set; }
    //public string GitEmail { get; set; }
    //public string GitUserName { get; set; }
    //public string GitPassword { get; set; }

    public static Settings Load(string jsonFile)
    {
        Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(jsonFile));
        return settings;

    }

    public static void Generate(string jsonFilePath)
    {
        Settings settings = new Settings()
        {
            Token = "YOUR BOT TOKEN HERE.",
            RepositoryPath = "FULL PATH TO COMMUNITY REPOSITORY HERE",
            //GitEmail = "YOUR GITHUB EMAIL HERE THAT YOU WANT TO USE FOR COMMITS.",
            //GitUserName = "YOUR GITHUB USERNAME HERE.",
            //GitPassword = "YOUR GITHUB PASSWORD HERE."
        };

        string json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(jsonFilePath, json);
    }
}


public class MainClass
{
    private DiscordSocketClient Client;
    private CommunityRepository Repository;
    private static string SETTINGS_JSON = "settings.json";
    private static Settings settings;

    public static void Main(string[] args)
    {
        Console.WriteLine(String.Format("Loading settings file {0}...", SETTINGS_JSON));

        if (!File.Exists(SETTINGS_JSON))
        {
            Settings.Generate(SETTINGS_JSON);
            string formattedString = String.Format("Generated settings file {0} in directory {1}. Please edit this file by hand outside of this program and relaunch this program for it to work properly", SETTINGS_JSON, Directory.GetCurrentDirectory());
            Console.WriteLine(formattedString);
            return;
        }

        settings = Settings.Load(SETTINGS_JSON);

        Console.WriteLine(String.Format("Moving into directory {0} from settings file...", settings.RepositoryPath));

        if (!Directory.Exists(settings.RepositoryPath))
        {
            Console.WriteLine(String.Format("Failed to move into directory {0} from the settings file. Please make sure that you typed in the community repository directory correctly.", settings.RepositoryPath));
            return;
        }

        Console.WriteLine("Successfully moved into the community repository directory. Starting the Discord bot...");
        string token = settings.Token;
        Directory.SetCurrentDirectory(settings.RepositoryPath);
        new MainClass().StartBot(token).GetAwaiter().GetResult();

    }

/*
    public static void UpdateGitHub(string commitMessage)
    {
        using (var repo = new Repository(Directory.GetCurrentDirectory()))
        {
            LibGit2Sharp.Commands.Stage(repo, "*");
            Signature signature = new Signature(new Identity("Stage-Checker BOT", settings.GitEmail), DateTime.Now);
            Commit commit = repo.Commit(commitMessage, signature, signature);

            Remote remote = repo.Network.Remotes["origin"];
            var options = new PushOptions();

            options.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((url, usernameFromUrl, types) =>
            new UsernamePasswordCredentials()
            {
                Username = settings.GitUserName,
                Password = settings.GitPassword
            });

            repo.Network.Push(remote, @"refs/heads/master", options);

        }
    }*/

    /*public static void UpdateGitHubHack(string commitMessage)
    {
        using (PowerShell shell = PowerShell.Create())
        {
            shell.AddCommand(@"git init");
            shell.AddCommand(@"git add .");
            shell.AddCommand(String.Format(@"git commit -m ""{0}""", commitMessage));
            shell.AddCommand(@"git push origin master");
            var results = shell.Invoke();
            string test = "eouthoae";
        }
    }
    */
    

    public async Task StartBot(string token)
    {
        Client = new DiscordSocketClient();
        Client.MessageReceived += MessageReceived;
        const string COMMUNITY_REPOSITORY_FILE = "CommunityStages.json";
        Repository = CommunityRepository.Load(COMMUNITY_REPOSITORY_FILE);
        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    private async Task MessageReceived(SocketMessage message)
    {
        string discordMessage = message.Content.ToLower();
        string discordMessageChannel = message.Channel.Name;

        if (message.Author.IsBot)
        {
            return;
        }

        if (discordMessageChannel != "bot-testing" && discordMessageChannel != "uploaded-maps")
        {
            return;
        }
        
        if (discordMessage == "!help")
        {
            await Commands.HelpCommand(message);
            return;
        }

        if (discordMessage == "!upload")
        {
            await Commands.HandleUploadCommand(message, Repository);
            return;
        }

        if (discordMessage == "!remove")
        {
            await message.Channel.SendMessageAsync(@"You did not specify a stage name. Type in ""!help remove"" if you need help");
            return;
        }

        if (discordMessage == "!ping")
        {
            await message.Channel.SendMessageAsync("Pong.");
            return;
        }

        if (discordMessage == "!search")
        {
            await message.Channel.SendMessageAsync(@"You did not specify a stage name. Type in ""!help search"" if you need help.");
            return;
        }

        if (discordMessage == "!info")
        {
            await message.Channel.SendMessageAsync(@"You did not specify a stage name. Type in ""!help info"" if you need help.");
            return;
        }

        if (discordMessage == "!get")
        {
            await message.Channel.SendMessageAsync(@"You did not specify a stage name. Type in ""!help get"" if you need help.");
            return;
        }

        if (discordMessage.StartsWith("!help "))
        {
            await Commands.HandleSubHelpCommand(message);
            return;
        }

        if (discordMessage.StartsWith("!remove "))
        {
            await Commands.HandleRemoveCommand(message, Repository);
            return;
        }

        if (discordMessage.StartsWith("!search "))
        {
            await Commands.HandleSearchCommand(message, Repository);
            return;
        }

        if (discordMessage.StartsWith("!info "))
        {
            await Commands.HandleInformationCommand(message, Repository);
            return;
        }

        if (discordMessage.StartsWith("!get "))
        {
            await Commands.HandleGetCommand(message, Repository);
            return;
        }
    }
}

        


