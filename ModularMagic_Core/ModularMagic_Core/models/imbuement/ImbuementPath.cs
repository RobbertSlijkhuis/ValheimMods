namespace ModularMagic_Core.Models
{ 
    internal class ImbuementPath
    {
        public bool allowIntersect;
        public int column;
        public int row;

        public ImbuementPath(int column, int row, bool allowIntersect = false)
        {
            this.allowIntersect = allowIntersect;
            this.column = column;
            this.row = row;
        }
    }
}
