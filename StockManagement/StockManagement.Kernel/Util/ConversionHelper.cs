namespace StockManagement.Kernel.Util;


public static class ConversionHelper
{
	private static readonly string[] Ones = { string.Empty, Language.Numbers.one, Language.Numbers.two, Language.Numbers.three, Language.Numbers.four, Language.Numbers.five, Language.Numbers.six, Language.Numbers.seven, Language.Numbers.eight, Language.Numbers.ten };
	private static readonly string[] Teens = { Language.Numbers.ten, Language.Numbers.eleven, Language.Numbers.twelve, Language.Numbers.thirteen, Language.Numbers.fourteen, Language.Numbers.fifteen, Language.Numbers.sixteen, Language.Numbers.seventeen, Language.Numbers.eighteen, Language.Numbers.nineteen };
	private static readonly string[] Twentys = { Language.Numbers.twenty, Language.Numbers.twentyone, Language.Numbers.twentytwo, Language.Numbers.twentythree, Language.Numbers.twentyfour, Language.Numbers.twentyfive, Language.Numbers.twentysix, Language.Numbers.twentyseven, Language.Numbers.twentyeight, Language.Numbers.twentynine };
	private static readonly string[] Tens = { string.Empty, string.Empty, string.Empty, Language.Numbers.thirty, Language.Numbers.fourty, Language.Numbers.fifty, Language.Numbers.sixty, Language.Numbers.seventy, Language.Numbers.eighty, Language.Numbers.ninety };
	private static readonly string[] Hundreds = { string.Empty, Language.Numbers.hundredSomething, Language.Numbers.twohundred, Language.Numbers.threehundred, Language.Numbers.fourhundred, Language.Numbers.fivehundred, Language.Numbers.sixhundred, Language.Numbers.sevenhundred, Language.Numbers.eighthundred, Language.Numbers.ninehundred };
	private static readonly string[] Thousands = { string.Empty, Language.Numbers.thousand, Language.Numbers.million, Language.Numbers.billion, Language.Numbers.trillion };


	public static string ConvertToWords(long number)
	{
		if (number == 0)
			return Language.Numbers.zero;

		var words = string.Empty;

		for (int i = 0; number > 0; i++)
		{
			if (number % 1000 != 0)
				words = ConvertHundreds(number % 1000) + Thousands[i] + " " + words;

			number /= 1000;
		}

		return words.Trim();
	}

	private static string ConvertHundreds(long number)
	{
		var words = string.Empty;
		if (number == 100)
		{
			return Language.Numbers.hundred;
		}

		if (number >= 100)
		{
			words += Hundreds[number / 100] + " ";
			number %= 100;
		}

		if (number >= 20 && number <= 29)
		{
			words += Twentys[number % 20] + " ";
		}
		else if (number >= 10 && number <= 19)
		{
			words += Teens[number % 10] + " ";
		}
		else
		{
			words += Tens[number / 10] + " ";
			if (number > 30 && number % 10 != 0)
			{
				words += Language.Numbers.and + " ";
			}
			words += Ones[number % 10] + " ";
		}

		return words;
	}
}
