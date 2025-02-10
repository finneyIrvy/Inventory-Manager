using System.Collections.Generic;
using Inventory_Management_System__Miracle_Shop_.Models;

namespace Inventory_Management_System__Miracle_Shop_.ViewModel
{
    public class MoveProductViewModel
    {
        public Product Product { get; set; }
        public List<Folder> Folders { get; set; }
    }
}
