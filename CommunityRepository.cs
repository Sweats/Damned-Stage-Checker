using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using LibGit2Sharp;

public class Stage
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }
}


public class CommunityRepository
{
    public List<Stage> Stages { get; set; }

    private void Refresh()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText("CommunityStages.json", json);
    }

    // Called once when the bot is started.
    public static CommunityRepository Load(string jsonFile)
    {
        CommunityRepository repository = JsonConvert.DeserializeObject<CommunityRepository>(File.ReadAllText(jsonFile));
        return repository;
    }

    private void DeleteStageArchive(Stage stage)
    {
        FileInfo[] files = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.zip", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Length; i++)
        {
            string name = Path.GetFileNameWithoutExtension(files[i].FullName).ToLower();

            if (stage.Name.ToLower() == name.ToLower())
            {
                File.Delete(files[i].FullName);
                break;
            }
        }

    }

    public bool StageExists(Stage stage)
    {
        FileInfo[] files = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.zip", SearchOption.TopDirectoryOnly);

        bool success = false;
        string stageName = stage.Name.ToLower();

        for (int i = 0; i < files.Length; i++)
        {
            string archiveName = Path.GetFileNameWithoutExtension(files[i].FullName).ToLower();

            if (archiveName == stageName)
            {
                success = true;
                break;
            }
        }

        return success;
    }


    public void AddStage(Stage newStage)
    {
        Stages.Add(newStage);
        Refresh();
    }

    public bool RemoveStage(Stage stageToRemove)
    {
        bool success = false;

        for (int i = 0; i < Stages.Count; i++)
        {
            Stage stage = Stages[i];

            if (stageToRemove.Name.ToLower() == stage.Name.ToLower())
            {
                Stages.RemoveAt(i);
                DeleteStageArchive(stage);
                Refresh();
                success = true;
                break;
            }
        }


        return success;
    }

    public void UpdateStage(Stage oldStage, Stage newStage)
    {
        for (int i = 0; i < Stages.Count; i++)
        {
            Stage stage = Stages[i];

            if (stage.Name.ToLower() == oldStage.Name.ToLower())
            {
                Stages[i].Name = newStage.Name;
                Stages[i].Author = newStage.Author;
                Stages[i].Description = newStage.Description;
                Stages[i].Date = newStage.Date;
                Refresh();
                break;
            }
        }

    }

    public Stage GetStage(string stageName)
    {
        Stage stage = null;

        for (int i = 0; i < Stages.Count; i++)
        {
            stage = Stages[i];

            if (stage.Name.ToLower() == stageName.ToLower())
            {
                break;
            }
        }

        return stage;
    }

    public string GetPossibleStageNames(string stageName)
    {
        StringBuilder stringBuilder = new StringBuilder(String.Empty);

        for (int i = 0; i < Stages.Count; i++)
        {
            string name = Stages[i].Name;
            string lowercasedName = name.ToLower();

            if (lowercasedName.Contains(stageName))
            {
                stringBuilder.Append(name);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }
        }

        return stringBuilder.ToString();
    }

    public string GetPathToStageInCommunityRepository(Stage stage)
    {
        string result = String.Empty;
        FileInfo[] info = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.zip");

        for (int i = 0; i < info.Length; i++)
        {
            string stageName = Path.GetFileNameWithoutExtension(info[i].FullName).ToLower();

            if (stageName == stage.Name.ToLower())
            {
                result = info[i].FullName;
            }
        }

        return result;
    }
}
 

        /* private Stage[] GetStages()
    {
        FileInfo[] info = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.zip", SearchOption.TopDirectoryOnly);
        int stageCount = info.Length;
        Stage[] stages = new Stage[stageCount];

        for (int i = 0; i < info.Length; i++)
        {
            stages[i] = new Stage
            {
                Name = Path.GetFileNameWithoutExtension(info[i].FullName),
                Author = String.Empty;
        };
    }

        return stages;
    */

