namespace citi_core.Utilities
{
    public static class CardMasking
    {
        public static string Mask(string last4Digits) => $"**** **** {last4Digits}";
    }
}
