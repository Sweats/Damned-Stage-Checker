using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Net;
using System.Linq;

public static class Commands
{
    public async static Task HelpCommand(SocketMessage message)
    {
        string response = @"List of commands:

1. !upload - Checks your uploaded file to see if I can add it into the community repository. Type ""!help upload"" for more information.

2. !get - Checks the stage community repository for the requested file for me to upload to this channel. Type in ""!help get"" for more information.

3. !ping - Checks to see if this bot is working properly.

4. !help - Prints this help message.

5. !remove - Removes a stage from the community repository that you are the author of. Type ""!help remove"" for more information.

6. !search - Searches for one or more stages in the community repository and returns a list of stages that contains your search query. Type ""!help search"" for more information.

7. !info - Searches for a stage in the community repository and returns information on that stage. Type in ""!help info"" for more information.

8. !author - Updates the author of the stage. Type in ""!help author"" for more information.

9. !update - Updates an existing stage in the community repository that you are the author of with your new version of the stage. Type in ""!help update"" for more information.";


        await message.Channel.SendMessageAsync(response);
    }


    public async static Task HelpUploadCommand(SocketMessage message)
    {

        string response = @"!upload is used to upload a previously packaged stage into the community repository. Before I do this, I check for the following:

1. stage file having the same inner name as the .stage file itself.

2. .scene file having at least 1 light spawn point.

3. .scene file having at least 7 spawn points.

4. .scene file scene and stage section having the same name as the .scene file itself.

5. .scene file and .stage file having the same name.

6. Loading image being 1920x1080

7. Stage selection image being 900x100 and being a .png file. This is the one that shows when you are selecting a stage.

8. Stage selected image being 300x100 and being a .png file. This is the one that shows in the lobby after you have selected a stage.

To use !upload, simply attach a zip archive that contains all of the files that a stage needs and I will check everything for you.

If the check is successful, I will upload your stage to the community repository and make you the author of that stage.";

        await message.Channel.SendMessageAsync(response);

    }


    public async static Task HelpRemoveCommand(SocketMessage message)
    {
        string response = @"!remove is used to remove a stage from the community repository that you are the author of.

Usage: !remove <stageName> where stageName is the name of your stage that is in the community repository

Example: !remove Rose Crimnson Hotel";

        await message.Channel.SendMessageAsync(response);

    }

    public async static Task HelpSearchCommand(SocketMessage message)
    {
        string response = @"!search is used to search the community repository for the specified stage. 

Usage: !search <stageName> where stageName is the name of the stage that you want to search for in the community repository. Searches are case insensitive.

Example: !search Bodom Hotel

I will send a list of possible matches if I find any stage that contains your search query.";


        await message.Channel.SendMessageAsync(response);

    }


    public async static Task HelpInfoCommand(SocketMessage message)
    {
        string response = @"!info is used to search the community repository for the specified stage.

Usage: !info <stageName> where stageName is the name of the stage that you want to get information about in the community repository. Searches are case insensitive.

Example: !info Hund Hills Community Center

If the search is successful, I will return you information about that stage.

Use !search instead if you want to browse the list of stages by name.";


        await message.Channel.SendMessageAsync(response);

    }


    public async static Task HelpPingCommand(SocketMessage message)
    {
        string response = @"!ping is used to simply check if I am online and working properly

Usage: !ping";

        await message.Channel.SendMessageAsync(response);
    }


    public async static Task HelpGetCommand(SocketMessage message)
    {
        string response = @"!get is used to tell me to upload a requested stage for you.

Usage: !get <stageName> where stageName is the name of the stage that you want me to upload for you.

Example: !get Pog Champ Hotel

If the stage exists in the repository, I will upload it for you.

Use !search instead if you want to browse the list of stages by name";


        await message.Channel.SendMessageAsync(response);
    }


    public static async Task HelpAuthorCommand(SocketMessage message)
    {
        string response = @"!author is used to update the author of a stage. This command is only used if you are uploading the stage on someone elses behalf and you know that they will never join the Damned Discord.

Usage: !author <newAuthor> <stageName> where newAuthor is the author name and where stageName is the name of the stage that you want the author to be the same as newAuthor.

Example: !author sweats Black Lake Hotel

Parameter newAuthor must be one word and no spaces.

If the stage exists in the repository, I will update the author for you.";

        await message.Channel.SendMessageAsync(response);

    }


    public static async Task HelpUpdateCommand(SocketMessage message)
    {
        string response = @"!update is used to update the stage after you have made changes to your stage and wish to update the stage itself in the repository.

Usage: Type in !update and then upload your new zip archive with the message.

If the stage exists in the repository, I will update the stage for you.";

        await message.Channel.SendMessageAsync(response);
    }

    public async static Task HandleSubHelpCommand(SocketMessage message)
    {
        string discordMessage = message.Content.ToLower();
        string[] splitStrings = discordMessage.Split(' ');
        string subCommand = splitStrings[1];

        if (subCommand == "upload")
        {
            await HelpUploadCommand(message);
            return;
        }

        if (subCommand == "remove")
        {
            await HelpRemoveCommand(message);
            return;
        }

        if (subCommand == "search")
        {
            await HelpSearchCommand(message);
            return;

        }

        if (subCommand == "info")
        {
            await HelpInfoCommand(message);
            return;
        }


        if (subCommand == "get")
        {
            await HelpGetCommand(message);
            return;
        }

        if (subCommand == "update")
        {
            await HelpUpdateCommand(message);
            return;

        }

        if (subCommand == "author")
        {
            await HelpAuthorCommand(message);
            return;
        }
    }


    public async static Task HandleRemoveCommand(SocketMessage message, CommunityRepository repository)
    {
        string originalMessage = message.Content;
        string[] splitMessage = originalMessage.Split(new char[] { ' ' }, 2);
        string stageName = splitMessage[1];

        Stage stage = new Stage()
        {
            Name = stageName.ToLower(),
            Author = String.Format("{0}#{1}", message.Author.Username.ToLower(), message.Author.Discriminator.ToLower())
        };

        if (!repository.StageExists(stage))
        {
            string reply = String.Format(@"Failed to remove the stage ""{0}"" from the community repository because it does not exist", stageName);
            await message.Channel.SendMessageAsync(reply);
            return;
        }

        Stage repositoryStage = repository.GetStage(stage.Name);

        if (repositoryStage.Author.ToLower() != stage.Author.ToLower())
        {
            string reply = String.Format(@"Failed to remove the stage ""{0}"" from the community repository because you are not the author for that stage.", stageName);
            await message.Channel.SendMessageAsync(reply);
            return;
        }

        await message.Channel.SendMessageAsync("Removing your stage from the community repository now...");
        repository.RemoveStage(stage);
        string commitMessage = String.Format("Removed the stage {0} from the community repository", repositoryStage.Name);
        MainClass.UpdateGitHub(commitMessage);
        string response = String.Format(@"Successfully removed the stage ""{0}"" from the community repository!", stageName);
        await message.Channel.SendMessageAsync(response);
    }

    public async static Task HandleUploadCommand(SocketMessage message, CommunityRepository repository)
    {
        var attachments = message.Attachments;

        if (attachments.Count == 0)
        {
            await message.Channel.SendMessageAsync("You did not attach a zip file to your message. Please try again.");
            return;
        }

        if (attachments.Count > 1)
        {
            await message.Channel.SendMessageAsync("Please upload only one stage package at a time.");
            return;
        }

        Attachment attachment = attachments.ElementAt(0);
        string oldFileName = attachment.Filename;
        bool changesMade = false;

        if (oldFileName.Contains("-") || oldFileName.Contains("_"))
        {
            changesMade = true;
        }

        string fileName = oldFileName.Replace("-", " ").Replace("_", " ");

        Stage stage = new Stage()
        {
            Name = Path.GetFileNameWithoutExtension(fileName),
            Author = String.Format("{0}#{1}", message.Author.Username.ToLower(), message.Author.Discriminator.ToLower()),
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
        };

        if (repository.StageExists(stage))
        {
            string response = String.Format("This stage already exists in the community repository. If you are the author of the existing stage, please use !update instead to update your stage.");
            await message.Channel.SendMessageAsync(response);
            return;
        }

        string URL = attachment.Url;

        using (WebClient client = new WebClient())
        {
            client.DownloadFile(new Uri(URL), fileName);
        }

        DamnedPackage package = new DamnedPackage();

        await message.Channel.SendMessageAsync("Checking your stage archive now...");

        if (!package.Check(fileName))
        {
            string reason = package.reasonForFailedCheck;
            await message.Channel.SendMessageAsync(reason);
            Directory.Delete(package.tempDirectory, true);
            Directory.Delete(fileName);
            return;
        }

        await message.Channel.SendMessageAsync("Stage check successful! I am adding your stage into the community repository now...");

        repository.AddStage(stage);
        string commitMessage = String.Format("Added in new stage {0} to the community repository", stage.Name);
        MainClass.UpdateGitHub(commitMessage);
        Directory.Delete(package.tempDirectory, true);

        if (changesMade)
        {
            string response = String.Format("Success! Your stage has been added into the community repository!\n\nBy the way, I have renamed your zip file from \"{0}\" to \"{1}.zip\" because it looks nicer and is slightly easier to search for", fileName, stage.Name);
            await message.Channel.SendMessageAsync(response);
            return;
        }

        await message.Channel.SendMessageAsync("Success! Your stage has been added into the community repository!");
    }


    public async static Task HandleSearchCommand(SocketMessage message, CommunityRepository repository)
    {
        string originalMessage = message.Content;
        string[] splitMessage = originalMessage.Split(new char[] { ' ' }, 2);
        string stageName = splitMessage[1];

        Stage stage = new Stage()
        {
            Name = stageName.ToLower(),
        };

        string possibleStrings = repository.GetPossibleStageNames(stage.Name);

        if (possibleStrings == String.Empty)
        {
            await message.Channel.SendMessageAsync("I did not found any stages matching your query :(");
            return;
        }

        string response = String.Format("Here are the stages that I have found that contains the search query \"{0}\" within the stage name.\n\n{1}", stageName, possibleStrings);
        await message.Channel.SendMessageAsync(response);
        return;

    }

    public async static Task HandleInformationCommand(SocketMessage message, CommunityRepository repository)
    {
        string originalMessage = message.Content;
        string[] splitMessage = originalMessage.Split(new char[] { ' ' }, 2);
        string stageName = splitMessage[1];

        Stage stage = new Stage()
        {
            Name = stageName.ToLower(),
        };

        if (!repository.StageExists(stage))
        {
            string possibleStrings = repository.GetPossibleStageNames(stage.Name);

            if (possibleStrings != String.Empty)
            {
                string reply = String.Format("Failed to grab information for the stage \"{0}\" because it does not exist in the repository.\n\n Could you mean one of these?\n\n{1}", stageName, possibleStrings);
                await message.Channel.SendMessageAsync(reply);
                return;
            }

            string response = String.Format(@"Failed to grab information for the stage ""{0}""  because it does not exist in the community repository", stageName);
            await message.Channel.SendMessageAsync(response);
            return;
        }

        Stage foundStage = repository.GetStage(stage.Name);

        string reponse = String.Format("Information for stage {0}\n\nAuthor: {1}\n\nUpload Date: {2}\n\nDescription: {3}", foundStage.Name, foundStage.Author, foundStage.Date, foundStage.Description);
        await message.Channel.SendMessageAsync(reponse);

    }

    public async static Task HandleGetCommand(SocketMessage message, CommunityRepository repository)
    {
        string originalMessage = message.Content;
        string[] splitMessage = originalMessage.Split(new char[] { ' ' }, 2);
        string stageName = splitMessage[1];

        Stage stage = new Stage()
        {
            Name = stageName
        };

        string filePath = repository.GetPathToStageInCommunityRepository(stage);

        if (filePath == String.Empty)
        {
            string reply = String.Format(@"Failed  to find the stage ""{0}"". because it does not exist in the community repository. If you are not sure what you are looking for, use !search instead.", stageName);
            await message.Channel.SendMessageAsync(reply);
            return;
        }

        await message.Channel.SendMessageAsync("Uploading the stage now...");
        string response = "Here is the requested stage that you asked for.";
        await message.Channel.SendFileAsync(filePath, response);
    }


    public async static Task HandleUpdateCommand(SocketMessage message, CommunityRepository repository)
    {
        var attachments = message.Attachments;

        if (attachments.Count == 0)
        {
            await message.Channel.SendMessageAsync("You did not upload a file for me to update in the community repository");
            return;
        }

        if (attachments.Count > 1)
        {
            await message.Channel.SendMessageAsync("Please only send one file at a time.");
            return;
        }

        var attachment = message.Attachments.ElementAt(0);
        string stageName = Path.GetFileNameWithoutExtension(attachment.Filename).Replace("_", " ").Replace("-", " ");

        Stage stage = new Stage()
        {
            Name = stageName,
            Author = String.Format("{0}#{1}", message.Author.Username.ToLower(), message.Author.Discriminator.ToLower()),
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
        };

        if (!repository.StageExists(stage))
        {
            await message.Channel.SendMessageAsync("Failed to update the stage because the stage does not exist in the community repository.");
            return;
        }

        Stage repositoryStage = repository.GetStage(stage.Name);

        if (stage.Author != repositoryStage.Author)
        {
            await message.Channel.SendMessageAsync("Failed to update the stage because you are not the author of the stage.");
            return;
        }

        string URL = attachment.Url;
        string fileName = attachment.Filename.Replace("_", " ").Replace("-", " ");
        int randomNumber = new Random().Next();
        string damnedStageCheckerDirectoryName = String.Format("DamnedStageChecker_{0}", randomNumber);
        string tempArchive = Path.Combine(Path.GetTempPath(), damnedStageCheckerDirectoryName, fileName);
        string parentTempArchive = Path.Combine(Path.GetTempPath(), damnedStageCheckerDirectoryName);
        
        if (Directory.Exists(parentTempArchive))
        {
            Directory.Delete(parentTempArchive, true);
        }

        Directory.CreateDirectory(parentTempArchive);

        using (WebClient client = new WebClient())
        {
            client.DownloadFile(new Uri(URL), tempArchive);
        }


        DamnedPackage package = new DamnedPackage();

        await message.Channel.SendMessageAsync("Checking your updated stage archive now...");

        if (!package.Check(tempArchive))
        {
            Directory.Delete(parentTempArchive, true);
            Directory.Delete(package.tempDirectory, true);
            await message.Channel.SendMessageAsync(package.reasonForFailedCheck);
            return;

        }

        await message.Channel.SendMessageAsync("Stage check successful! I am updating your stage in the community repository now...");
        string oldFilePath = repository.GetPathToStageInCommunityRepository(repositoryStage);
        File.Delete(oldFilePath);
        File.Copy(tempArchive, fileName);
        Directory.Delete(parentTempArchive, true);
        Directory.Delete(package.tempDirectory, true);
        repository.UpdateStage(repositoryStage, stage);

        string commitMessage = String.Format("Updated the stage {0} in the community repository", stage.Name);
        MainClass.UpdateGitHub(commitMessage);
        await message.Channel.SendMessageAsync("Succcessfully updated your stage that is in the community repository!");

    }


    public static async Task HandleAuthorCommand(SocketMessage message, CommunityRepository repository)
    {
        string originalMessage = message.Content;
        string[] splitMessages = originalMessage.Split(new char[] { ' ' }, 3);

        if (splitMessages.Length < 3)
        {
            await message.Channel.SendMessageAsync(@"You did not specify a stage name or author. Type in ""!help author"" if you need help.");
            return;
        }

        string author = splitMessages[1];
        string stageName = splitMessages[2];

        Stage stage = new Stage()
        {
            Name = stageName,
            Author = author,
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
        };

        
        if (!repository.StageExists(stage))
        {
            await message.Channel.SendMessageAsync(String.Format(@"Failed to update the author for the stage ""{0}"" because the stage does not exist in the community repository.", stageName));
            return;

        }

        Stage oldStage = repository.GetStage(stage.Name);

        string messageAuthor = String.Format("{0}#{1}", message.Author.Username.ToLower(), message.Author.Discriminator.ToLower());


        if (oldStage.Author != messageAuthor)
        {
            await message.Channel.SendMessageAsync("Failed to update the author of the stage because you are not the author");
            return;
        }

        repository.UpdateStage(oldStage, stage);
        await message.Channel.SendMessageAsync("Updating the author for the stage in the community repository now...");
        string commitMessage = String.Format(@"Updated the author for the stage {0} from ""{1}"" to ""{2}"".", stageName, oldStage.Author, stage.Author);
        //MainClass.UpdateGitHub(commitMessage);
        await message.Channel.SendMessageAsync("Successfully updated the author of the stage!");
    }

}

