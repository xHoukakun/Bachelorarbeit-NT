namespace Bachelorarbeit_NT
{
    /// <summary>
    /// Diese Klasse hält einen Dezimalwert und einen ulong Wert Der Ulong Wert soll eine Art Index sein für die Starter Klasse. 
    /// </summary>
    public class ExtDecimal
    {
        public decimal d;
        public ulong n;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_d">Dezimal Wert</param>
        /// <param name="_n">Laufende Variable N</param>
        public ExtDecimal(decimal _d, ulong _n)
        {
            d = _d;
            n = _n;
        }
    }
}
