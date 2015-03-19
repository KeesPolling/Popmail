using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

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
