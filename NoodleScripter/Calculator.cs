using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter
{
    public class Calculator
    {
        private double SolveInternal(Problem problem)
        {
            return problem.exp switch
            {
                Expression.Divide => problem.left / problem.right,
                Expression.Multiply => problem.left * problem.right,
                Expression.Addition => problem.left + problem.right,
                Expression.Subtraction => problem.left - problem.right,
                Expression.Remainder => problem.left % problem.right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private double ConsolidateSolve(List<double> enumerable, Expression expression)
        {
            while (enumerable.Count > 1)
            {
                enumerable[0] = SolveInternal(new Problem
                {
                    left = enumerable[0],
                    right = enumerable[1],
                    exp = expression
                });
                enumerable.RemoveAt(1);
            }

            return enumerable[0];
        }

        public double Solve(string inp)
        {
            var solve = inp.Split('%').Select(j => j.Split('-').Select(t => t.Split('+').Select(f => f.Split('*').Select(g => g.Split('/')).ToArray()).ToArray()).ToArray()).ToArray();
            var rem = new List<double>();
            for (int i = 0; i < solve.Length; i++)
            {
                var sub = new List<double>();
                for (var i1 = 0; i1 < solve[i].Length; i1++)
                {
                    var add = new List<double>();
                    for (var i2 = 0; i2 < solve[i][i1].Length; i2++)
                    {
                        var mult = new List<double>();
                        for (var i3 = 0; i3 < solve[i][i1][i2].Length; i3++)
                        {
                            if (solve[i][i1][i2][i3].Length == 2)
                            {
                                mult.Add(SolveInternal(new Problem
                                {
                                    left = Convert.ToDouble(solve[i][i1][i2][i3][0]),
                                    right = Convert.ToDouble(solve[i][i1][i2][i3][1]),
                                    exp = Expression.Divide
                                }));
                            }
                            else
                            {
                                mult.Add(Convert.ToDouble(solve[i][i1][i2][i3][0]));
                            }
                        }
                        add.Add(ConsolidateSolve(mult, Expression.Multiply));
                    }
                    sub.Add(ConsolidateSolve(add, Expression.Addition));
                }
                rem.Add(ConsolidateSolve(sub, Expression.Subtraction));
            }
            return ConsolidateSolve(rem, Expression.Remainder);
        }

        private struct Problem
        {
            public double left;
            public double right;
            public Expression exp;
        }

        private enum Expression
        {
            Divide,
            Multiply,
            Addition,
            Subtraction,
            Remainder
        }
    }
}
