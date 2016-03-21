using SQLite;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace PopMail.Models
{
    class Email
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, ForeignKey(typeof(Folder))]
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

    class EmailSender
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Indexed, ForeignKey(typeof(Email))]
        public int EmailId{get; set;}
        public string Name{ get; set; }
        public string Address { get; set; }

        [ManyToOne]
        public Email Email {get; set;}
    }
    class EmailReplyTo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed, ForeignKey(typeof(Email))]
        public int EmailId { get; set; }
        public string Name { get; set; }
        [NotNull]
        public string Address { get; set; }

        [ManyToOne]
        public Email Email { get; set; }
    }
}

