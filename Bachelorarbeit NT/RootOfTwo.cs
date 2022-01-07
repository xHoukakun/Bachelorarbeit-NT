using System;

namespace Bachelorarbeit_NT
{/// <summary>
/// Implementation der Quadratischen Form für wurzel 2
/// </summary>
    public class RootOfTwo : Term
    {

        public RootOfTwo()
        {
            alpha = Convert.ToDecimal(Math.Sqrt(2)); //Das Alpha wird gesetzt
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">Coordinate</param>
        /// <returns>Gibt den Wert der Quadratischen Form zurück</returns>
        public override decimal CalcQuadratic(Coordinate c)
        {
            return Convert.ToDecimal(Math.PI)/(4*Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(alpha)))) * (c.x * c.x + alpha * c.y * c.y);
        }
        /// <summary>
        /// Gibt das Alpha zurück
        /// </summary>
        /// <param name="c">Coordinate</param>
        /// <returns></returns>
        public override string Alpha(Coordinate c)
        {
            return "RootOfTwo";
        }
    }
}
