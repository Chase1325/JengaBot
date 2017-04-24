using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JengaBot.Models
{
    class Tower
    {
       public static T[] InitializeArray<T>(int length) where T : new()
        {

            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }
            return array;
        }

    

        public Block[] blocks = InitializeArray<Block>(54);
        // public Row[] rows = InitializeArray<Row>(18);
        public List<Row> rows;
        public List<Block> priorityBlock;

        public double leftTilt = 0;
        public double rightTilt = 0;

    }
}
