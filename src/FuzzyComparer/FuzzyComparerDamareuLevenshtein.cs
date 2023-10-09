using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyComparer
{
    public class FuzzyComparerDamareuLevenshtein : IFuzzyStringComparer
    {
        public double Similarity(string? src, string? modified)
        {
            src ??= string.Empty;
            modified ??= string.Empty;

            if (string.Equals(src, modified, StringComparison.OrdinalIgnoreCase))
            {
                return 1d;
            }

            src = src.ToLowerInvariant();
            modified = modified.ToLowerInvariant();

            var distance = (double)GetDistance(src, modified);
            var result = 1d - distance / (double)Math.Max(src.Length, modified.Length);
            return result;
        }

        public int GetDistance(string? original, string? modified)
        {
            original ??= string.Empty;
            modified ??= string.Empty;

            var matrix = GetMatrix(original, modified);
            return matrix[original.Length, modified.Length];
        }

        public int[,] GetMatrix(string? original, string? modified)
        {
            //this follows the https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance
            //apprently lucence use's Levenshtein https://lucene.apache.org/core/7_3_0/core/org/apache/lucene/search/FuzzyQuery.html
            //https://dzone.com/articles/the-levenshtein-algorithm-1

            original ??= string.Empty;
            modified ??= string.Empty;

            int len_orig = original.Length;
            int len_diff = modified.Length;

            var matrix = new int[len_orig + 1, len_diff + 1];
            for (int i = 0; i <= len_orig; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= len_diff; j++)
            {
                matrix[0, j] = j;
            }

            var org = original.AsSpan();
            var mod = modified.AsSpan();
            for (int i = 1; i <= len_orig; i++)
            {
                for (int j = 1; j <= len_diff; j++)
                {
                    int cost = mod[j - 1] == org[i - 1] ? 0 : 1;

                    //below is 
                    //  minimum( d[i-1, j] + 1,              // deletion 
                    //           d[i, j - 1] + 1,            // insertion 
                    //           d[i - 1, j - 1] + cost)     // substitution
                    //  witten as
                    //  minimum( minimum (deletion, insertion) + 1, substitution)

                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j], matrix[i, j - 1]) + 1, matrix[i - 1, j - 1] + cost);

                    if (i > 1 && j > 1 && org[i - 1] == mod[j - 2] && org[i - 2] == mod[j - 1])
                    {
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost); //transposition making this Damareu
                    }
                }
            }

            return matrix;
        }
    }
}
