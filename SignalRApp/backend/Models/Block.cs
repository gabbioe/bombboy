namespace SignalRChatApp.Models
{
    public class Block
    {
        public int Position_X {  get; set; }
        public int Position_Y { get; set; }

        public bool Is_Breakable { get; set; }

        public Block (int position_X, int position_Y, bool is_Breakable )
        {
            Position_X = position_X;
            Position_Y = position_Y;
            Is_Breakable = is_Breakable;
        }

    }
}
