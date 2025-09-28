using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMauiApp.Models
{
    public class CardModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        // you can add more, like:
        public string ImagePath { get; set; }
        public bool IsFavorite { get; set; }
    }
}