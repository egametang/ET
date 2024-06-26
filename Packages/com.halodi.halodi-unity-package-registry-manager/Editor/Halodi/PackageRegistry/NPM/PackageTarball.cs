using Newtonsoft.Json.Linq;
using System.IO;
using Unity.SharpZipLib.GZip;
using Unity.SharpZipLib.Tar;

namespace Halodi.PackageRegistry.NPM
{

    /// <summary>
    /// Tool to create tarballs for packages.
    /// UnityEditor.PackageManager.Client.Pack() creates a broken tarball that gets rejected by bintray.
    /// </summary>
    public class PackageTarball
    {
        public static string Create(string packageFolder, string outputFolder)
        {
            JObject manifest = PublicationManifest.LoadManifest(packageFolder);

            string packageName = manifest["name"] + "-" + manifest["version"] + ".tgz";

            Directory.CreateDirectory(outputFolder);

            string outputFile = Path.Combine(outputFolder, packageName);


            Stream outStream = File.Create(outputFile);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            AddDirectoryFilesToTar(tarArchive, packageFolder, true, "packages/");
            tarArchive.Close();
            gzoStream.Close();
            outStream.Close();

            return outputFile;
        }



        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (Directory.Exists(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }



        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse, string directoryName)
        {
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarEntry.Name = directoryName;
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                TarEntry fileEntry = TarEntry.CreateEntryFromFile(filename);
                fileEntry.Name = directoryName + Path.GetFileName(filename);
                tarArchive.WriteEntry(fileEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                {
                    string dirname = new DirectoryInfo(directory).Name;
                    if (dirname == ".git")
                    {
                        continue;
                    }
                    string newDirectory = directoryName + dirname + "/";
                    AddDirectoryFilesToTar(tarArchive, directory, recurse, newDirectory);
                }
            }
        }
    }
}