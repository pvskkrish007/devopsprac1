using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Configuration;
using System.Collections.Specialized;

namespace ZipFolderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourcePath = ConfigurationManager.AppSettings["sourcePath"].ToString();
            string targetzipPath = ConfigurationManager.AppSettings["targetPath"].ToString();
            string logfileName = ConfigurationManager.AppSettings["logfileName"].ToString();
            int fileDaystoZip = Convert.ToInt32(ConfigurationManager.AppSettings["fileDaystoZip"].ToString());
            string[] fileextension = ConfigurationManager.AppSettings["fileextension"].ToString().Split(',');

            if (File.Exists(logfileName))
                File.Delete(logfileName);

            WriteLog(logfileName, "Log archive started.");

            bool IsFileExist = ArchiveLogFiles(sourcePath, targetzipPath, logfileName, fileDaystoZip, fileextension);

            WriteLog(logfileName, "Log archive Ended.");
        }


        public static bool ArchiveLogFiles(string sourcePath, string targetzipPath, string logfileName, int fileDaystoZip, string[] fileExtensionAllowed)
        {
            WriteLog(logfileName, sourcePath + " checking Source Path.");

            if (Directory.Exists(sourcePath))
            {
                WriteLog(logfileName, sourcePath + " source Path Found.");

                WriteLog(logfileName, "Checking files at Source Path.");

                DirectoryInfo info = new DirectoryInfo(sourcePath);
                var files = info.GetFiles().AsEnumerable().Where(file => file.LastWriteTime <= DateTime.Now.AddDays(-1 * fileDaystoZip)).ToArray();

                bool IsFolderCreated = false;
                string targetPath = string.Empty;
                string targetfolderName = string.Empty;

                if(files.Count() <= 0)
                {
                    WriteLog(logfileName, "No files at Source Path.");
                }

                foreach (FileInfo file in files)
                {

                    if ((file.LastWriteTime.Date <= DateTime.Now.AddDays(-1 * fileDaystoZip).Date) && 
                        FileExtensionContains(file.Extension.ToString(), fileExtensionAllowed))
                    {
                        WriteLog(logfileName, file.Name.ToString() + " file found at Source Path.");

                        if (!IsFolderCreated)
                        {
                            WriteLog(logfileName, "Creating target folder.");

                            targetfolderName = CreateFolder(targetzipPath);
                            targetPath = targetzipPath + "\\" + targetfolderName;
                            IsFolderCreated = true;

                            WriteLog(logfileName, targetPath + " target folder created.");
                        }

                        if (IsFolderCreated)
                        {
                            WriteLog(logfileName, targetPath + " checking target folder path.");

                            if (Directory.Exists(targetPath))
                            {
                                WriteLog(logfileName, targetPath + " target Path Found.");

                                WriteLog(logfileName, "Process start to move file from source to target folder.");

                                string fileName = Path.GetFileName(file.Name);
                                string sourceFile = Path.Combine(sourcePath, fileName);
                                string destFile = Path.Combine(targetPath, fileName);
                                File.Move(sourceFile, destFile, true);

                                WriteLog(logfileName, file.Name.ToString() + " file moved to target folder.");
                            }
                        }
                        else
                        {
                            WriteLog(logfileName, "Target folder not found.");
                        }
                    }
                    else
                    {
                        WriteLog(logfileName, file.Name.ToString() + " file format is not supported.");
                    }
                }

                WriteLog(logfileName, targetPath + " checking target folder to zip files.");

                if (Directory.Exists(targetPath))
                {
                    WriteLog(logfileName, targetPath + " found target folder to zip files.");

                    WriteLog(logfileName, "Process start to zip folder.");

                    string zipPath = targetzipPath + "\\" + targetfolderName + ".zip";

                    string[] zipfiles = Directory.GetFiles(targetzipPath, "*.zip");

                    foreach (string s in zipfiles)
                    {
                        if (s == zipPath)
                            File.Delete(s);    
                    }

                    ZipFile.CreateFromDirectory(targetPath, zipPath, CompressionLevel.Fastest, true);

                    WriteLog(logfileName, "Process completed to zip folder.");

                    WriteLog(logfileName, targetPath + " process start to delete folder contain old files.");

                    Directory.Delete(targetPath, true);

                    WriteLog(logfileName, targetPath + " process complete to delete folder contain old files.");
                }

            }
            else
            {
                WriteLog(logfileName, sourcePath + " is not a valid directory." );
            }

            return true;
        }

        public static string CreateFolder(string targetzipPath)
        {
            string folderName = DateTime.Now.ToString("MM.dd.yyyy");
            string locationToCreateFolder = targetzipPath + "\\" + folderName;
            Directory.CreateDirectory(locationToCreateFolder);
            return folderName;
        }

        public static void WriteLog(string logPath, string strMsg)
        {
            if (!File.Exists(logPath))
            {
                FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            File.AppendAllText(logPath, DateTime.Now.ToString() + " ---- " + strMsg + Environment.NewLine);
        }

        public static bool FileExtensionContains(string FileExtension, string[] FileExtensionAllowed)
        {
            foreach (string x in FileExtensionAllowed)
            {
                if (x.Trim().ToUpper() == FileExtension.ToUpper())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
