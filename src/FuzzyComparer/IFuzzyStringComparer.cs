namespace FuzzyComparer
{
    public interface IFuzzyStringComparer
    {
        double Similarity(string src, string modified);
    }
}