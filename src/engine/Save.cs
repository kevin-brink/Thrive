using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Godot;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using Directory = Godot.Directory;
using File = Godot.File;

/// <summary>
///   A class representing a single saved game
/// </summary>
public class Save
{
    public const string SAVE_SAVE_JSON = "save.json";
    public const string SAVE_INFO_JSON = "info.json";

    /// <summary>
    ///   Name of this save on disk
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   General information about this save
    /// </summary>
    public SaveInformation Info { get; set; } = new SaveInformation();

    /// <summary>
    ///   The state the game was in when it was saved
    /// </summary>
    public string GameStateName { get; set; }

    /// <summary>
    ///   The game properties of the saved game
    /// </summary>
    public GameProperties SavedProperties { get; set; }

    /// <summary>
    ///   Loads a save from a file or throws an exception
    /// </summary>
    /// <param name="saveName">The name of the save. This is not the full path.</param>
    /// <returns>The loaded save</returns>
    public static Save LoadFromFile(string saveName)
    {
        var target = PathUtils.Join(Constants.SAVE_FOLDER, saveName);

        var directory = new Directory();

        if (!directory.FileExists(target))
            throw new ArgumentException("save with the given name doesn't exist");

        using (var file = new File())
        {
            file.Open(target, File.ModeFlags.Read);
            if (!file.IsOpen())
                throw new ArgumentException("couldn't open the file for reading");

            // var result = file.GetAsText();

            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///   Makes sure the save directory exists
    /// </summary>
    public static void MakeSureSaveSaveDirectoryExists()
    {
        var directory = new Directory();
        var result = directory.MakeDirRecursive(Constants.SAVE_FOLDER);

        if (result != Error.AlreadyExists && result != Error.Ok)
        {
            throw new IOException("can't create saves folder");
        }
    }

    /// <summary>
    ///   Writes this save to disk
    /// </summary>
    public void SaveToFile()
    {
        MakeSureSaveSaveDirectoryExists();

        var serialized = JsonConvert.SerializeObject(this, Constants.SAVE_FORMATTING);
        var justInfo = JsonConvert.SerializeObject(Info);

        // var screenshot;

        var target = PathUtils.Join(Constants.SAVE_FOLDER, Name);

        using (var file = new File())
        {
            file.Open(target, File.ModeFlags.Write);
            using (Stream gzoStream = new GZipOutputStream(new GodotFileStream(file)))
            {
                using (var tar = new TarOutputStream(gzoStream))
                {
                    OutputEntry(tar, SAVE_INFO_JSON, Encoding.UTF8.GetBytes(justInfo));
                    OutputEntry(tar, SAVE_SAVE_JSON, Encoding.UTF8.GetBytes(serialized));
                }
            }
        }
    }

    private static void OutputEntry(TarOutputStream archive, string name, byte[] data)
    {
        var entry = TarEntry.CreateTarEntry(name);

        // TODO: could fill in more of the properties

        entry.Size = data.Length;

        archive.PutNextEntry(entry);

        archive.Write(data, 0, data.Length);

        archive.CloseEntry();
    }
}

/// <summary>
///   Info embedded in a save file
/// </summary>
public class SaveInformation
{
    public enum SaveType
    {
        /// <summary>
        ///   Player initiated save
        /// </summary>
        Manual,

        /// <summary>
        ///   Automatic save
        /// </summary>
        AutoSave,

        /// <summary>
        ///   Quick save, separate from manual to make it easier to keep a fixed number of quick saves
        /// </summary>
        QuickSave,
    }

    /// <summary>
    ///   Version of the game the save was made with, used to detect incompatible versions
    /// </summary>
    public string ThriveVersion { get; set; } = Constants.Version;

    // TODO: get current platform
    public string Platform { get; set; }

    // TODO: get current username
    public string Creator { get; set; }

    public SaveType Type { get; set; } = SaveType.Manual;
}
