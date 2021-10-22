namespace Bachelorarbeit_NT
{
    public class Coordinate
    {

        //Beachte t ist eine Referenz auf ein Objekt. C# ist Objektorientiert, t gibt also eine Referenz auf eine Klasse zurück.
        private Term t { get; set; }
        public decimal x = 1;
        public decimal y = 1;
        public ulong n = 1;
        /// <summary>
        /// Konstruktor der Coordinate Klasse
        /// </summary>
        /// <param name="_t">Referenz auf den Term</param>
        /// <param name="_x">x coordinate</param>
        /// <param name="_y">y Coordinate</param>
        /// <param name="_n">n Coordinate</param>
        public Coordinate(Term _t, decimal _x, decimal _y, ulong _n)
        {
            y = _y;
            x = _x;
            t = _t;
            n = _n;
        }


        /// <summary>
        /// Ich gebe per Selbstreferenz Variablen zurück 
        /// </summary>
        /// <returns>quadratic form of coordinate</returns>
        public decimal Calc()
        {
            return t.CalcQuadratic(this);
        }
        /// <summary>
        /// Gebe das Alpha zurück
        /// </summary>
        /// <returns>Alpha</returns>
        public string Alpha()
        {
            return t.Alpha(this);
        }

        public ulong get_n()
        {
            return n;
        }
    }
}
