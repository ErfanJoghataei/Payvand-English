namespace Payvand.UI.Services;

public sealed class UsageLimitsOptions
{
    public int GuestMonthlyLinks { get; set; } = 10;
    public int UserMonthlyLinks { get; set; } = 100;
    public int GuestMonthlyQrCodes { get; set; } = 10;
    public int UserMonthlyQrCodes { get; set; } = 100;
}
