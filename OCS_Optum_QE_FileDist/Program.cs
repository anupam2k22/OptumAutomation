using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OCS_Optum_QE_FileDist
{
    class Program
    {
        static Logs log;
        static void Main(string[] args)
        {
            string logPath = ConfigurationManager.AppSettings.Get("LogPath");
            string LogExtnsn = ConfigurationManager.AppSettings.Get("LogExtnsn");
            log = new Logs(logPath, LogExtnsn);
            string dirRegex = @"(job\d+)(_\d+)";
            Regex dirReg = new Regex(dirRegex);
            try
            {
                string SourceDir = ConfigurationManager.AppSettings.Get("SourceDir");
                //list all Destination Directory
                List<string> DestDirList = Array.ConvertAll(ConfigurationManager.AppSettings.Get("DestinationList").Split(','), p => p.Trim()).ToList();
                //get all directories in source
                string[] AllSourceDirList = Directory.GetDirectories(SourceDir);
                List<dirCountModel> objDirList = new List<dirCountModel>();
                int srcdirCount = dirDetails.getDirectoryJobCount(AllSourceDirList, dirReg);
                if (srcdirCount == 0)
                {
                    log.LogToFile("There are no Jobs to move");
                }
                else
                {
                    //Loop through each destination directory to get jobcount
                    foreach (string DestDir in DestDirList)
                    {
                        string[] dirList = Directory.GetDirectories(DestDir);
                        dirCountModel objDirCountModel = new dirCountModel();
                        objDirCountModel.dirPath = DestDir;
                        objDirCountModel.count = dirDetails.getDirectoryJobCount(dirList,dirReg);
                        objDirList.Add(objDirCountModel);
                    }

                    #region Transfer jobs from source to destination
                    foreach (dirCountModel dir in objDirList)
                    {
                        string[] SourceDirList = Directory.GetDirectories(SourceDir);
                        log.LogToFile("Scanning Output Thread: " + dir.dirPath);
                        log.LogToFile("Number of jobs found: " + dir.count);
                        if (dir.count == 0)
                        {
                            string srcDir = SourceDirList[0];
                            string srcSubDir = srcDir.Replace(SourceDir, "");
                            MoveDir(srcDir, Path.Combine(dir.dirPath, srcSubDir));
                            DB_Update_Phase1(srcSubDir, dir.dirPath);
                        }
                        else
                            log.LogToFile("Thread is already occupied, due to which no job is moved.");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.LogToFile("Error Info : " + ex.Message);
            }
        }
        private static void MoveDir(string SourcePath, string DestinationPath)
        {
            if (Directory.Exists(SourcePath) && !Directory.Exists(DestinationPath))
            {
                Directory.Move(SourcePath, DestinationPath);
                log.LogToFile("Directory " + Path.GetFileName(SourcePath) + " moved from "+ SourcePath +" to "+ DestinationPath);
            }
            else
            {
                log.LogToFile("Error in moving: Dirctory "+ Path.GetFileName(SourcePath) + " could not be moved because:"+ Environment.NewLine +"A. It may not exist in "+ SourcePath + Environment.NewLine + "B. It may already exist in " + DestinationPath);
            }
        }
        private static void DB_Update_Phase1(string jobFolderName, string assignedThreadFullPath)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["OptumCISConnectionString"].ConnectionString);
                conn.Open();
                char threadName = assignedThreadFullPath[assignedThreadFullPath.Length - 2];
                string insertQuery = "insert into tblQualEngineThreads (job_folder_name,thread_assigned,assigned_datetime) values (@jobFolderName,@assignedThread,@assignedDateTime)";
                SqlCommand com1 = new SqlCommand(insertQuery, conn);
                com1.Parameters.AddWithValue("@jobFolderName", jobFolderName);
                com1.Parameters.AddWithValue("@assignedThread", threadName.ToString());
                com1.Parameters.AddWithValue("@assignedDateTime", DateTime.Now.ToString());
                com1.ExecuteNonQuery();
                log.LogToFile("Inserted into DB Table: tblQualEngineThreads.");
                conn.Close();
            }
            catch (Exception ex)
            {
                log.LogToFile("Error Info : " + ex.Message);
            }
        }

    }
}
