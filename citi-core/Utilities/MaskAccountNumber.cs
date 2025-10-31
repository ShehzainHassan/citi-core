public static class AccountMaskingHelper
{
    public static string MaskAccountNumber(string accountNumber)
    {
        if (accountNumber.Length < 8) return "****";
        return accountNumber.Substring(0, 8) + " ****";
    }
}