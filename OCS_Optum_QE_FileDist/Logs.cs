using System;
using System.IO;

namespace OCS_Optum_QE_FileDist
{
    public class Logs
    {
        private string slogFile { get; set; }

        public Logs(string Filename, string Extension)
        {
            slogFile = string.Concat(Filename, "_", DateTime.Now.ToString("yyyyMMdd"), Extension);
        }


        public void LogToFile(params string[] args)
        {
            FileStream fs = null;
            StreamWriter sw = null;
            string message = string.Empty;
            try
            {
                if (!File.Exists(slogFile))
                {
                    fs = File.Create(slogFile);
                    fs.Flush();
                    fs.Close();
                    fs = null;
                }
                using (sw = File.AppendText(slogFile))
                {
                    message = string.Concat(DateTime.Now.ToString(), " : ", args[0]);
                    sw.WriteLine(message);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                sw.Close();
                sw = null;
                throw new Exception(String.Format("Error while writing Log File. {0}", ex.Message));
            }
            finally
            {
                sw.Close();
                sw = null;
            }
        }
    }
}
