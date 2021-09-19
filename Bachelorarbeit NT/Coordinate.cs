using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelorarbeit_NT
{
    public class Coordinate
    {

        //Beachte t ist eine Referenz auf ein Objekt. C# ist Objektorientiert, t gibt also eine Referenz auf eine Klasse zurück.
        private Term t { get; set; }
        public decimal x = 1;
        public decimal y = 1;
        /// <summary>
        /// Konstruktor der Coordinate Klasse
        /// </summary>
        /// <param name="_t">Referenz auf den Term</param>
        /// <param name="_x">x coordinate</param>
        /// <param name="_y">y Coordinate</param>
        public Coordinate(Term _t, decimal _x, decimal _y)
        {
            y = _y;
            x = _x;
            t = _t;
        }


        /// <summary>
        /// Ich gebe per Selbstreferenz Variablen zurück 
        /// </summary>
        /// <returns>quadratic form of coordinate</returns>
        public decimal Calc()
        {
            return t.CalcQuadratic(this);
        }
        public string Alpha()
        {
            return t.Alpha(this);
        }
       
    }
}
