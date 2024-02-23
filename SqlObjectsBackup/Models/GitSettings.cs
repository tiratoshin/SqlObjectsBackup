using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlObjectsBackup.Models
{
    public class GitSettings
    {
        public string RepoPath { get; set; }
        public string Branch { get; set; }
        public string Repo { get; set; }
        public string LikePattern { get; set; }
        public string LogFilePath { get; set; }
    }
}
