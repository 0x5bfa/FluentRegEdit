using RegistryValley.App.Models;
using Windows.UI;

namespace RegistryValley.App.Data
{
    public static class AppThemeResourceFactory
    {
        public static ObservableCollection<AppThemeResourceItem> AppThemeResources { get; } = new()
        {
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(0, 0, 0, 0), /* Transparent */
                Name = "Default"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 255, 185, 0), /* #FFB900 */
                Name = "Yellow Gold"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 247, 99, 12), /* #F7630C */
                Name = "Orange Bright"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 209, 52, 56), /* #D13438 */
                Name = "Brick Red"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 255, 67, 67), /* #FF4343 */
                Name = "Mod Red"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 232, 17, 35), /* #E81123 */
                Name = "Red"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 234, 0, 94), /* #EA005E */
                Name = "Rose Bright"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 0, 120, 215), /* #0078D7 */
                Name = "Blue"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 135, 100, 184), /* #8764B8 */
                Name = "Iris Pastel"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 177, 70, 194), /* #B146C2 */
                Name = "Violet Red Light"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 0, 153, 188), /* #0099BC */
                Name = "Cool Blue Bright"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 0, 183, 195), /* #00B7C3 */
                Name = "Seafoam"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 0, 178, 148), /* #00B294 */
                Name = "Mint Light"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 122, 117, 116), /* #7A7574 */
                Name = "Gray"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 16, 124, 16), /* #107C10 */
                Name = "Green"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 118, 118, 118), /* #767676 */
                Name = "Overcast"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 76, 74, 72), /* #4C4A48 */
                Name = "Storm"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 105, 121, 126), /* #69797E */
                Name = "Blue Gray"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 74, 84, 89), /* #4A5459 */
                Name = "Gray Dark"
            },
            new AppThemeResourceItem
            {
                BackgroundColor = Color.FromArgb(50, 126, 115, 95), /* #7E735F */
                Name = "Camouflage"
            }
        };
    }
}
