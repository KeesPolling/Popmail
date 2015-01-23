using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensions.Exceptions;

namespace PopMailDemo.MVVM.Model
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        [Indexed, ForeignKey(typeof(Folder))] 
        public int Parent { get; set; }
 
    }
}
