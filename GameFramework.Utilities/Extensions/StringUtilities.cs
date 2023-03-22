using System.Text;

namespace GameFramework.Utilities.Extensions;

public static class StringUtilities
{
    // https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
    public static string AddSpacesToSentence(this string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        sb.Append(text[0]);
        for (var i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                if ((text[i - 1] != ' ' && 
                     !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                {
                    sb.Append(' ');
                }
            }
                
            sb.Append(text[i]);
        }

        return sb.ToString();
    }

}