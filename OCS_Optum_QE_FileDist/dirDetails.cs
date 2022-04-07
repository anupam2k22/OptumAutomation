using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OCS_Optum_QE_FileDist
{
    public class dirCountModel
    {
        public string dirPath { get; set; }
        public int count { get; set; }
    }
    public static class dirDetails
    {
        public static int getDirectoryJobCount(string[] dirList, Regex dirReg)
        {
            int dirCount = 0;
            foreach (string dir in dirList)
            {
                if (dirReg.IsMatch(dir))
                {
                    dirCount++;
                }
            }
            return dirCount;
        }

    }
}
