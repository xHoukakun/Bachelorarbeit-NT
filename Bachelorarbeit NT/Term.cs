using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{
    public abstract class Term
    {
        /// <summary>
        /// hier definiere ich enums für die Klasse Term, das erleichtert mir nachher eine Zuweisung 
        /// </summary>
        public enum TermType
        {
            Unknown,
            QuadraticTwo,
            Zeta3,
            Euler,
        }

        /// <summary>
        /// @TODO Weitere Enums setzen um damit halt die ganzen dinger unterscheiden zu können.
        /// </summary>
        public decimal alpha { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha">Irrational Constant</param>
        public Term() { }

        /// <summary>
        /// Calculates Quadratic Form
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract decimal CalcQuadratic(Coordinate c);
        public abstract string Alpha(Coordinate c);

    }
}
//abstract meint hier, dass es eine Abstrakte Klasse ist. Es ist ähnlich wie bei verebung. 
// Man kann sagen: Es gibt viele Quadratischen Formen 
//Für unseren fall betrachten wir bekanntlich die Formen von der Form ( x^2+ay^2 für a = irrational aber Konstant. 
// Das bedeutet wir legen die Form durch eine Eigenschaft fest. ( Alpha) 
//Deswegen definiere ich die beiden Methoden:
//public abstract decimal CalcQuadratic(Coordinate c);
//public abstract string Alpha(Coordinate c);
//Ich versichere den Workern damit: Das sie immer eine Form berechnen können. ( Da sie die Referenz auf den Typen der Form bekommen)
//Und dem DB Worker versichere ich: Du kannst immer erkennen woher diese "Form" kommt ( In welche Tabelle du diese "Sortieren" sollst)

