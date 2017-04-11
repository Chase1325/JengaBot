using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JengaBot.Models
{
    class Blocks
    {
       private int index;
       private int row;
       private int column;
       private float xpos;
       private float ypos;
       private float zpos;

        public int Index
        {
            get { return index; }
            set { index=value; }
        }

        public int Row
        {
            get { return row; }
            set { row=value; }
        }
        public int Column
        {
            get { return column; }
            set { column=value; }
        }
    }
}
