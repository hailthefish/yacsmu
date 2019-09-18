using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace yacsmu
{
    internal static class RandomGen
    {

        internal static RNGCryptoServiceProvider csRng = new RNGCryptoServiceProvider();

        internal static int Roll(byte numberSides)
        {

            if (numberSides <= 0)
                throw new ArgumentOutOfRangeException("numberSides");

            byte[] randomNumber = new byte[1];
            do
            {
                csRng.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberSides));

            return ((randomNumber[0] % numberSides) + 1);
        }

        private static bool IsFairRoll(byte roll, byte numSides)
        {
            // There are MaxValue / numSides full sets of numbers that can come up
            // in a single byte.  For instance, if we have a 6 sided die, there are
            // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
            int fullSetsOfValues = byte.MaxValue / numSides;

            // If the roll is within this range of fair values, then we let it continue.
            // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
            // < rather than <= since the = portion allows through an extra 0 value).
            // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
            // to use.
            return roll < numSides * fullSetsOfValues;
        }


    }
}
