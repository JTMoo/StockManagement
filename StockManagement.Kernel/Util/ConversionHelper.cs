namespace StockManagement.Kernel.Util;


public static class ConversionHelper
{
	private const long Thousand = 1_000;
	private const long Million = 1_000_000;
	private const long MaxNumber = Million * Million * Million - 1;

	private static readonly string[] Ones = [string.Empty, Language.Numbers.one, Language.Numbers.two, Language.Numbers.three, Language.Numbers.four, Language.Numbers.five, Language.Numbers.six, Language.Numbers.seven, Language.Numbers.eight, Language.Numbers.nine];
	private static readonly string[] Teens = [Language.Numbers.ten, Language.Numbers.eleven, Language.Numbers.twelve, Language.Numbers.thirteen, Language.Numbers.fourteen, Language.Numbers.fifteen, Language.Numbers.sixteen, Language.Numbers.seventeen, Language.Numbers.eighteen, Language.Numbers.nineteen];
	private static readonly string[] Twentys = [Language.Numbers.twenty, Language.Numbers.twentyone, Language.Numbers.twentytwo, Language.Numbers.twentythree, Language.Numbers.twentyfour, Language.Numbers.twentyfive, Language.Numbers.twentysix, Language.Numbers.twentyseven, Language.Numbers.twentyeight, Language.Numbers.twentynine];
	private static readonly string[] Tens = [string.Empty, string.Empty, string.Empty, Language.Numbers.thirty, Language.Numbers.fourty, Language.Numbers.fifty, Language.Numbers.sixty, Language.Numbers.seventy, Language.Numbers.eighty, Language.Numbers.ninety];
	private static readonly string[] Hundreds = [string.Empty, Language.Numbers.hundredSomething, Language.Numbers.twohundred, Language.Numbers.threehundred, Language.Numbers.fourhundred, Language.Numbers.fivehundred, Language.Numbers.sixhundred, Language.Numbers.sevenhundred, Language.Numbers.eighthundred, Language.Numbers.ninehundred];


	/// <summary>
	/// Spells <paramref name="number"/> out in Spanish words
	/// </summary>
	/// <remarks>
	/// Long scale: MIL, MILLÓN, BILLÓN. Supports 0 to 10^18 - 1.
	/// "UNO"/"VEINTIUNO" at the end, "UN"/"VEINTIÚN" before MIL, MILLÓN, BILLÓN.
	/// </remarks>
	public static string ConvertToWords(long number)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(number);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(number, MaxNumber);
		if (number == 0) return Language.Numbers.zero;

		List<string> words = [];
		AddScale(words, number / (Million * Million), Language.Numbers.trillion, Language.Numbers.multipleTrillions);
		AddScale(words, number / Million % Million, Language.Numbers.million, Language.Numbers.multipleMillions);
		AddBelowMillion(words, number % Million, true);
		return string.Join(" ", words);
	}

	private static void AddScale(List<string> words, long count, string singular, string plural)
	{
		if (count == 0) return;
		if (count == 1)
		{
			words.Add(Language.Numbers.oneShort);
			words.Add(singular);
			return;
		}

		AddBelowMillion(words, count, false);
		words.Add(plural);
	}

	private static void AddBelowMillion(List<string> words, long number, bool isEnd)
	{
		var thousands = number / Thousand;
		if (thousands > 1) AddBelowThousand(words, thousands, false);
		if (thousands > 0) words.Add(Language.Numbers.thousand);
		AddBelowThousand(words, number % Thousand, isEnd);
	}

	private static void AddBelowThousand(List<string> words, long number, bool isEnd)
	{
		if (number == 100)
		{
			words.Add(Language.Numbers.hundred);
			return;
		}

		if (number > 100) words.Add(Hundreds[number / 100]);

		var rest = number % 100;
		if (rest == 0) return;
		if (rest < 10)
		{
			words.Add(OnesWord(rest, isEnd));
			return;
		}

		if (rest < 20)
		{
			words.Add(Teens[rest - 10]);
			return;
		}

		if (rest < 30)
		{
			words.Add(rest == 21 && !isEnd ? Language.Numbers.twentyoneShort : Twentys[rest - 20]);
			return;
		}

		words.Add(Tens[rest / 10]);
		if (rest % 10 == 0) return;

		words.Add(Language.Numbers.and);
		words.Add(OnesWord(rest % 10, isEnd));
	}

	private static string OnesWord(long digit, bool isEnd)
	{
		return digit == 1 && !isEnd ? Language.Numbers.oneShort : Ones[digit];
	}
}
