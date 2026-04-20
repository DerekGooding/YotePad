namespace YotePad.Model;

public readonly struct SearchState(string term, bool matchCase, bool matchWholeWord, bool searchDown)
{
    public string SearchTerm { get; init; } = term;
    public bool MatchCase { get; init; } = matchCase;
    public bool MatchWholeWord { get; init; } = matchWholeWord;
    public bool SearchDown { get; init; } = searchDown;
}
