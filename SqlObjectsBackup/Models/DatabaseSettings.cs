using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlObjectsBackup.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string Folder { get; set; }
    }
}
