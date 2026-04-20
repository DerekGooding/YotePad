namespace Yotepad.Services;

public class SearchService
{
    public string LastSearchTerm { get; private set; } = string.Empty;
    public bool LastMatchCase { get; private set; } = false;
    public bool LastMatchWholeWord { get; private set; } = false;
    public bool LastSearchDown { get; private set; } = true;

    public void UpdateSearchState(string term, bool matchCase, bool matchWholeWord, bool searchDown)
    {
        LastSearchTerm = term;
        LastMatchCase = matchCase;
        LastMatchWholeWord = matchWholeWord;
        LastSearchDown = searchDown;
    }

    public int Find(string fullText, string searchTerm, int startIndex, bool matchCase, bool matchWholeWord, bool searchDown)
    {
        if (string.IsNullOrEmpty(fullText) || string.IsNullOrEmpty(searchTerm)) return -1;

        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (searchDown)
        {
            var index = startIndex < fullText.Length
                ? FindNext(fullText, searchTerm, startIndex, comparison, matchWholeWord)
                : -1;

            // Wrap to top
            if (index == -1)
                index = FindNext(fullText, searchTerm, 0, comparison, matchWholeWord);

            return index;
        }
        else
        {
            var index = startIndex > 0
                ? FindPrevious(fullText, searchTerm, startIndex - 1, comparison, matchWholeWord)
                : -1;

            // Wrap to bottom
            if (index == -1)
                index = FindPrevious(fullText, searchTerm, fullText.Length - 1, comparison, matchWholeWord);

            return index;
        }
    }

    private int FindNext(string fullText, string searchTerm, int fromIndex, StringComparison comparison, bool matchWholeWord)
    {
        var pos = fromIndex;
        while (pos <= fullText.Length - searchTerm.Length)
        {
            var found = fullText.IndexOf(searchTerm, pos, comparison);
            if (found == -1) return -1;
            if (!matchWholeWord || IsWholeWordMatch(fullText, found, searchTerm.Length))
                return found;
            pos = found + 1;
        }
        return -1;
    }

    private int FindPrevious(string fullText, string searchTerm, int fromIndex, StringComparison comparison, bool matchWholeWord)
    {
        var pos = fromIndex;
        while (pos >= 0)
        {
            var found = fullText.LastIndexOf(searchTerm, pos, pos + 1, comparison);
            if (found == -1) return -1;
            if (!matchWholeWord || IsWholeWordMatch(fullText, found, searchTerm.Length))
                return found;
            pos = found - 1;
        }
        return -1;
    }

    private bool IsWholeWordMatch(string fullText, int index, int length)
    {
        // Check character before — must be start of text or non-word character
        if (index > 0 && IsWordChar(fullText[index - 1]))
            return false;

        // Check character after — must be end of text or non-word character
        var after = index + length;
        return after >= fullText.Length || !IsWordChar(fullText[after]);
    }

    private bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    public string ReplaceAll(string fullText, string searchTerm, string replaceTerm, bool matchCase, bool matchWholeWord)
    {
        if (string.IsNullOrEmpty(fullText) || string.IsNullOrEmpty(searchTerm)) return fullText;

        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (!matchWholeWord)
            return fullText.Replace(searchTerm, replaceTerm ?? string.Empty, comparison);

        // Whole word replace — walk the string manually
        var result = new System.Text.StringBuilder();
        var pos = 0;
        while (pos < fullText.Length)
        {
            var found = fullText.IndexOf(searchTerm, pos, comparison);
            if (found == -1)
            {
                result.Append(fullText, pos, fullText.Length - pos);
                break;
            }

            result.Append(fullText, pos, found - pos);

            if (IsWholeWordMatch(fullText, found, searchTerm.Length))
            {
                result.Append(replaceTerm ?? string.Empty);
                pos = found + searchTerm.Length;
            }
            else
            {
                result.Append(fullText[found]);
                pos = found + 1;
            }
        }
        return result.ToString();
    }
}