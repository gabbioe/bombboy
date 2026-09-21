using System;

namespace SignalRChatApp.Models
{
    
    public class Map
    {
        private int Size_X {  get; set; }

        private int Size_Y { get; set; }

        private Block[,] Blocks;

        public Map (int size_X, int size_Y)
        {
            if(size_X >6 && size_Y > 6)
            {

                Size_X = size_X;
                Size_Y = size_Y;
                //del borderiu +2 
                Blocks = new Block[Size_X + 2, Size_Y + 2];
                for (int i = 0; i < Size_X; i++)
                {
                    for (int j = 0; j < Size_Y; j++)
                    {
                        //borderiai
                        if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                            Blocks[i, j] = Block(i, j, false);
                        //kas antra bloka pazymi kad butu 
                        if (i % 2 == 0 && j % 2 == 0)
                            Blocks[i, j] = Blocks[i, j, false];
                        //Tikrina ar nera kampuose kur playeriai spawninasi 
                        if ((i > 2 || j > 2) && (i < Size_X - 3 || j > 2) && (i > 2 || j < Size_Y - 3) && (i < Size_X - 3 || j < Size_Y - 3))
                        {
                            
                        }


                }
                }
            }
            else
            {
                throw new ArgumentException($"Both Size X (currently{size_X}) and size Y (currently{size_Y}) must be more than 6");
            }
            
        }
    
        public bool CheckForBlock(int dx, int dy)
        {

        }


        // getMap

        


    }
}
