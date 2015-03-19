using SQLite;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PopMailDemo.MVVM.Model
{
    class Email
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public DateTime OrigDateTime { get; set; }

        [OneToMany]
        public List<EmailFrom> From { get; set; }
    }

    class EmailFrom
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, ForeignKey(typeof(Email))]
        public int EmailId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        [ManyToOne]
        public Email Email { get; set; }
    }
}

