using System.Text;

public static class NumericInputFilter
{
    public static string FilterFloat(string input, bool allowNegative)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        bool hasDot = false;
        bool hasMinus = false;

        StringBuilder stringBuilder = new(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsDigit(c))
            {
                stringBuilder.Append(c);
            }
            else if (c == '.' && !hasDot)
            {
                hasDot = true;
                stringBuilder.Append(c);
            }
            else if (c == '-' && allowNegative && i == 0 && !hasMinus)
            {
                hasMinus = true;
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }

    public static string FilterInt(string input, bool allowNegative)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        bool hasMinus = false;
        StringBuilder stringBuilder = new(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsDigit(c))
            {
                stringBuilder.Append(c);
            }
            else if (c == '-' && allowNegative && i == 0 && !hasMinus)
            {
                hasMinus = true;
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}
