namespace razor.Components
{
    public static class Extentions
    {
        public static string ToD(this double x)
        {
            return x.ToString().Replace(",", ".");
        }
    }
}
