using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System;
using System.Text.RegularExpressions;
using System.Drawing;

public struct Dimensions
{
    public int x;
    public int y;
}

public class DamnedPackage
{

    public string tempDirectory { get; private set; }
    private string zipArchivePath;
    public string reasonForFailedCheck { get; private set; }
    public int objectsCount { get; private set; }

    public bool Check(string zipArchivePath)
    {
        this.zipArchivePath = zipArchivePath;

        if (Path.GetExtension(zipArchivePath) != ".zip")
        {
            string archiveName = Path.GetFileName(zipArchivePath);
            reasonForFailedCheck = String.Format("Check failed because \"{0}\" is not a .zip file. Please repackage your stage into a .zip file", archiveName);
            return false;
        }

        CreateTempDirectory();

        ZipFile.ExtractToDirectory(zipArchivePath, tempDirectory);

        if (!CheckDirectories())
        {
            return false;
        }

        if (!CheckFiles())
        {
            return false;
        }

        return true;
    }


    private bool CheckFiles()
    {
        FileInfo[] files = new DirectoryInfo(tempDirectory).GetFiles("*", SearchOption.AllDirectories);
        bool success = true;

        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i].Name;
            string filePath = files[i].FullName;
            string fileExtension = Path.GetExtension(fileName);

            if (fileExtension == ".jpg" || fileExtension == ".png")
            {
                if (!CheckImage(filePath))
                {
                    success = false;
                    break;
                }
            }

            else if (fileExtension == ".stage" || fileExtension == ".scene")
            {
                if (!CheckStageOrScene(filePath))
                {
                    success = false;
                    break;
                }
            }

            else if (fileExtension == ".object")
            {
                if (!CheckObject(filePath))
                {
                    success = false;
                    break;
                }
            }

            else
            {
                success = false;
                reasonForFailedCheck = String.Format("Check failed because \"{0}\" is not a .jpg, .png, .stage, .scene, or .object format.", fileName);
                break;
            }
        }

        return success;
    }

    private bool CheckDirectories()
    {
        DirectoryInfo[] info = new DirectoryInfo(tempDirectory).GetDirectories("*", SearchOption.AllDirectories);

        string[] directoriesToCheck = new string[] { "DamnedData", "GUI", "Resources", "TerrorImages", "Stages" };

        bool success = true;

        for (int i = 1; i < info.Length; i++)
        {
            bool found = false;
            string directory = info[i].Name;

            if (directory == "Objects")
            {
                continue;
            }

            for (int j = 0; j < directoriesToCheck.Length; j++)
            {
                if (directory == directoriesToCheck[j])
                {
                    found = true;
                }
            }

            if (!found)
            {
                success = false;
                reasonForFailedCheck = String.Format("Check failed because the directory \"{0}\" is not supposed to be in your zip archive", directory);
            }
        }

        return success;
    }

    private bool CheckStageOrScene(string stagePath)
    {
        string directoryPath = Path.GetDirectoryName(stagePath);
        string directoryName = Path.GetFileName(directoryPath);

        if (directoryName != "Stages")
        {
            string stageName = Path.GetFileName(stagePath);
            reasonForFailedCheck = String.Format("Check failed because \"{0}\" does not reside in the Stages directory", stageName);
            return false;
        }

        FileInfo[] stages = new DirectoryInfo(directoryPath).GetFiles("*", SearchOption.TopDirectoryOnly);

        if (stages.Length > 2)
        {
            reasonForFailedCheck = "Check failed because the Stages directory has more than 2 files in it. Only 1 scene and 1 file is allowed";
            return false;
        }

        bool success = true;

        for (int i = 0; i < stages.Length; i++)
        {
            string stageName = stages[i].Name;

            if (!FindCorrespondingFile(stages, stageName))
            {
                success = false;
                break;
            }
        }

        if (success)
        {
            string extension = Path.GetExtension(stagePath);
            string reason = String.Empty;

            if (extension == ".stage")
            {
                success = CheckInnerStageFile(stagePath, ref reason);
            }

            else if (extension == ".scene")
            {
                success = CheckInnerSceneFile(stagePath, ref reason);
            }

            if (!success)
            {
                this.reasonForFailedCheck = reason;
            }
        }

        return success;
    }

    private bool FindCorrespondingFile(FileInfo[] stages, string stageOrScene)
    {
        string extension = Path.GetExtension(stageOrScene);
        bool success = false;

        for (int i = 0; i < stages.Length; i++)
        {
            string otherExtension = Path.GetExtension(stages[i].Name);

            if (otherExtension != extension)
            {
                string name = Path.GetFileNameWithoutExtension(stageOrScene);
                string otherName = Path.GetFileNameWithoutExtension(stages[i].Name);

                if (name == otherName)
                {
                    success = true;
                    break;
                }
            }
        }

        if (!success)
        {
            string otherExtension;

            if (extension == ".stage")
            {
                otherExtension = ".scene";

            }

            else
            {
                otherExtension = ".stage";
            }

            string stageOrSceneName = Path.GetFileName(stageOrScene);
            reasonForFailedCheck = String.Format("Check failed because \"{0}\" does not have its corresponding \"{1}{2}\" file in the same directory.", stageOrSceneName, stageOrScene, otherExtension);
            return false;
        }

        return success;

    }

    private bool CheckImage(string imagePath)
    {
        string directoryPath = Path.GetDirectoryName(imagePath);
        string directoryName = Path.GetFileName(directoryPath);

        if (directoryName != "GUI" && directoryName != "TerrorImages")
        {
            string imageName = Path.GetFileName(imagePath);
            reasonForFailedCheck = String.Format("Check failed because \"{0}\" does not reside in either the GUI directory or the TerrorImages directory.", imageName);
            return false;
        }

        string fileExtension = Path.GetExtension(imagePath);

        if (fileExtension == ".png")
        {
            Dimensions dimensions = GetDimensions(imagePath);

            if (dimensions.x != 300 && dimensions.y != 100 || dimensions.x != 900 && dimensions.y != 100)
            {
                string imageName = Path.GetFileName(imagePath);
                reasonForFailedCheck = String.Format("Check failed because the dimensions for the image \"{0}\" is not 300 X 100 or 900 X 100", imageName);
                return false;
            }
        }

        else if (fileExtension == ".jpg")
        {
            Dimensions dimensions = GetDimensions(imagePath);

            if (dimensions.x != 1920 && dimensions.y != 1080)
            {
                string imageName = Path.GetFileName(imagePath);
                reasonForFailedCheck = String.Format("Check failed because the dimensions for the image \"{0}\" is not 1920 X 1080", imageName);
                return false;
            }
        }

        return true;
    }


    private bool CheckObject(string objectPath)
    {
        string directoryNamePath = Path.GetDirectoryName(objectPath);
        string directoryName = Path.GetFileName(directoryNamePath);

        if (directoryName != "Objects")
        {
            string objectName = Path.GetFileName(objectPath);
            reasonForFailedCheck = String.Format("Check failed because object \"{0}\" does not reside in the Objects directory.", objectName);
            return false;
        }

        return true;
    }


    private void CreateTempDirectory()
    {
        string tempPath = Path.GetTempPath();
        int randomNumber = new Random().Next();
        string tempStringNumber = String.Format("DamnedWorkshop_{0}", randomNumber);
        tempPath = Path.Combine(tempPath, tempStringNumber);

        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
        }

        Directory.CreateDirectory(tempPath);
        tempDirectory = tempPath;
    }

    private Dimensions GetDimensions(string imagePath)
    {
        Dimensions dimensions = new Dimensions();

        using (var image = Image.FromFile(imagePath))
        {
            dimensions.x = image.Width;
            dimensions.y = image.Height;
        }

        return dimensions;
    }

    private bool CheckInnerStageFile(string stagePath, ref string failedReason)
    {
        string nameToMatch = Path.GetFileNameWithoutExtension(stagePath);
        string stageName = Path.GetFileName(stagePath);

        using (StreamReader reader = new StreamReader(stagePath))
        {
            string contents = reader.ReadToEnd();
            string stageLineToFind = String.Format("stage {0}", nameToMatch);
            string sceneLineToFind = String.Format("scene {0}", nameToMatch);

            Match match = Regex.Match(contents, stageLineToFind);

            if (!match.Success)
            {
                failedReason = String.Format("Check failed because the stage section in \"{0}\" does not match the file name.", stageName);
                return false;
            }

            match = Regex.Match(contents, sceneLineToFind);

            if (!match.Success)
            {
                failedReason = String.Format("Check failed because the scene section in \"{0}\" does not match the scene name", stageName);
                return false;
            }
        }

        return true;


    }

    private bool CheckSceneForLights(string sceneFileContents, string scenePath, ref string failedReason)
    {
        MatchCollection collection = Regex.Matches(sceneFileContents, "light light.[0-9]+");

        if (collection.Count < 1)
        {
            string name = Path.GetFileName(scenePath);
            failedReason = String.Format("Check failed because \"{0}\" does not have any light points.", name);
            return false;
        }

        return true;
    }

    private bool CheckSceneForSpawnPoints(string sceneFileContents, string scenePath, ref string failedReason)
    {
        MatchCollection collection = Regex.Matches(sceneFileContents, "spawn_point [0-9]+");
        string name = Path.GetFileName(scenePath);

        if (collection.Count < 1)
        {
            failedReason = String.Format("Check failed because \"{0}\" does not have any spawn points", name);
            return false;
        }

        int matchCount = collection.Count;

        if (matchCount < 7)
        {
            failedReason = String.Format("Check failed because \"{0}\" does not have enough spawn points. Found spawn point count: {1}. Required count: 7.", name, matchCount);
            return false;
        }

        return true;
    }

    private bool CheckSceneForInvalidObjects(string sceneFileContents, string scenePath, ref string failedReason)
    {
        return true;
        //MatchCollection collection = Regex.Matches(sceneFileContents, "");
    }


    private bool CheckSceneForProperSceneName(string sceneFileContents, string scenePath, ref string failedReason)
    {
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        string pattern = String.Format("scene {0}", sceneName);
        Match match = Regex.Match(sceneFileContents, pattern);

        if (!match.Success)
        {
            failedReason = String.Format("Check failed because the scene section in {0} does not match the actual file name.", sceneName);
            return false;
        }

        return true;
    }

    private bool CheckInnerSceneFile(string scenePath, ref string failedReason)
    {
        using (StreamReader reader = new StreamReader(scenePath))
        {
            string contents = reader.ReadToEnd();

            if (!CheckSceneForProperSceneName(contents, scenePath, ref failedReason))
            {
                return false;
            }

            if (!CheckSceneForSpawnPoints(contents, scenePath, ref failedReason))
            {
                return false;
            }

            if (!CheckSceneForLights(contents, scenePath, ref failedReason))
            {
                return false;
            }

        }

        return true;
    }
}
