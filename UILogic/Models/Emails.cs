using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace Popmail.UILogic.Models
{
    public class Emails
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, ForeignKey(typeof(Folders))]
        public int FolderId { get; set; }
        [Indexed]
        public DateTime OrigDateTime { get; set; }
        public DateTime ReceivedDateTime { get; set; }
        
        [OneToMany]
        public List<EmailFrom> From { get; set; }
        [OneToMany]
        public List<EmailSender> Sender { get; set; }
        [OneToMany]
        public List<EmailReplyTo> ReplyTo { get; set; }
    }

    public class EmailFrom
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Indexed, ForeignKey(typeof(Emails))]
        public int EmailId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        [ManyToOne]
        public Emails Email { get; set; }
    }

    public class EmailSender
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Indexed, ForeignKey(typeof(Emails))]
        public int EmailId{get; set;}
        public string Name{ get; set; }
        public string Address { get; set; }

        [ManyToOne]
        public Emails Email {get; set;}
    }
    public class EmailReplyTo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, ForeignKey(typeof(Emails))]
        public int EmailId { get; set; }
        public string Name { get; set; }
        [NotNull]
        public string Address { get; set; }

        [ManyToOne]
        public Emails Email { get; set; }
    }
}

