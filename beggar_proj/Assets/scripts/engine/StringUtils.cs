namespace HeartUnity
{
    public static class StringUtils
	{
		public static string RemoveNonAlphaNumeric(string input)
		{
			char[] result = new char[input.Length];
			int index = 0;

			foreach (char c in input)
			{
				if (char.IsLetterOrDigit(c))
				{
					result[index++] = c;
				}
			}

			return new string(result, 0, index);
		}
	}
}