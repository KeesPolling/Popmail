using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popmail.UILogic.ViewModels
{
    public class AccountPageParameters
    {
        public int Account { get; set; }
        public FolderTreeViewModel FolderTree { get; set; }
    }
}
