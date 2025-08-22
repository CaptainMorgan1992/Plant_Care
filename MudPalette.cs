using MudBlazor;

namespace Auth0_Blazor;

public class GreenTheme : MudTheme
{
    public GreenTheme()
    {
        PaletteLight = new PaletteLight
        {
            Black = "#000000",
            White = "#FFFFFF",
            Primary = "#49694d", 
            Secondary = "#75b875", 
            Tertiary = "#c9ffc2",
            Success = "#5bd24b",
            Info = "#fdfea9",
            Warning = "#ff526c",
            Error = "#ff1438",
            Dark = "#68a267",
            Background = "#ecf9ec",
        };
        
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "fontfile", "Arial", "sans-serif" }
            },
            H6 = new H6Typography()
            {
                FontFamily = new[] { "fontfile", "Arial", "sans-serif" },
                FontSize = "1.2rem",
                FontWeight = "500",
                LineHeight = "1.6",
                LetterSpacing = ".0075em"
            },
            H5 = new H5Typography()
            {
                FontFamily = new[] { "fontfile", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = "500",
                LineHeight = "1.3",
                LetterSpacing = ".0075em"
            },
            H3 = new H5Typography()
            {
                FontFamily = new[] { "fontfile", "Arial", "sans-serif" },
                FontSize = "1.5rem",
                FontWeight = "500",
                LineHeight = "1.4",
                LetterSpacing = ".5em"
            },
            H2 = new H5Typography()
            {
                FontFamily = new[] { "fontfile", "Arial", "sans-serif" },
                FontSize = "2rem",
                FontWeight = "700",
                LineHeight = "1.6",
                LetterSpacing = ".5em"
            }
        };
    }
}