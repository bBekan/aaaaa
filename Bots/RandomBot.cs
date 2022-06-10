using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelaAI
{
    internal class RandomBot : Player
    {
        public RandomBot() : base()
        {
        }
        public override SuitEnum? CallTrump(int scoreDifference, bool muss, bool last = false)
        {
                Random rand = new Random();
                if (last)
                {
                    var se = (SuitEnum)Enum.GetValues(typeof(SuitEnum)).GetValue(rand.Next(0, 4));
                    return se;
                }

                int index = rand.Next(0, 5);
                if (index == 4)
                    return null;
                return (SuitEnum)Enum.GetValues(typeof(SuitEnum)).GetValue(index);

        }
    }
}
