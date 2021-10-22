namespace Bachelorarbeit_NT
{
    /// <summary>
    /// Diese Klasse hält 3 Variablen um informationen für die Starter Klasse temporär zu speichern
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Speichert die zuordnung des Ergebnisses ab
        /// </summary>
        public string type;
        public decimal result;
        public ulong n;
        /// <summary>
        /// Hier werden die Variablen gesetzt.
        /// </summary>
        /// <param name="s">Typ</param>
        /// <param name="r">Result</param>
        /// <param name="n">N</param>
        public Result(string s, decimal r, ulong n)
        {
            this.type = s;
            this.result = r;
            this.n = n;
        }

    }
}
