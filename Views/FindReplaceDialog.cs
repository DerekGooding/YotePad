namespace Yotepad.Dialogs;

public class FindReplaceDialog : Form
{
    private readonly TextBox _txtFind = new TextBox();
    private readonly TextBox _txtReplace = new TextBox();
    private readonly CheckBox _chkMatchCase = new CheckBox();
    private readonly CheckBox _chkMatchWholeWord = new CheckBox();
    private readonly Button _btnFindNext = new Button();
    private readonly Button _btnReplace = new Button();
    private readonly Button _btnReplaceAll = new Button();
    private readonly Button _btnCancel = new Button();
    private readonly Button _btnToggleReplace = new Button();
    private readonly Label _lblReplace = new Label();

    private bool _isExpanded = false;
    private const int COLLAPSED_HEIGHT = 190;
    private const int EXPANDED_HEIGHT = 190;

    public event Action<string, bool, bool>? OnFindNext;
    public event Action<string, string, bool, bool>? OnReplace;
    public event Action<string, string, bool, bool>? OnReplaceAll;

    public FindReplaceDialog(ThemeManager themeManager)
    {
        InitializeComponent();
        ApplyTheme(themeManager);
        SetMode(false); // Default to the compact Find mode
    }

    private void InitializeComponent()
    {
        ClientSize = new Size(380, COLLAPSED_HEIGHT);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;

        // The magical expand/collapse button
        _btnToggleReplace.Text = "▽";
        _btnToggleReplace.Size = new Size(24, 23);
        _btnToggleReplace.Location = new Point(10, 12);
        _btnToggleReplace.Click += (s, e) => SetMode(!_isExpanded);

        Label lblFind = new Label { Text = "Find what:", Location = new Point(40, 15), AutoSize = true };
        _txtFind.Location = new Point(115, 12);
        _txtFind.Size = new Size(160, 23);
        
        _lblReplace.Text = "Replace with:";
        _lblReplace.Location = new Point(35, 45);
        _lblReplace.AutoSize = true;
        
        _txtReplace.Location = new Point(115, 42);
        _txtReplace.Size = new Size(160, 23);

        _txtFind.TextChanged += (s, e) => 
        {
            bool hasText = _txtFind.Text.Length > 0;
            _btnFindNext.Enabled = hasText;
            _btnReplace.Enabled = hasText;
            _btnReplaceAll.Enabled = hasText;
        };

        _btnFindNext.Text = "Find Next";
        _btnFindNext.Location = new Point(285, 10);
        _btnFindNext.Enabled = false;
        _btnFindNext.Click += (s, e) => OnFindNext?.Invoke(_txtFind.Text, _chkMatchCase.Checked, _chkMatchWholeWord.Checked);

        _btnReplace.Text = "Replace";
        _btnReplace.Location = new Point(285, 40);
        _btnReplace.Enabled = false;
        _btnReplace.Click += (s, e) => OnReplace?.Invoke(_txtFind.Text, _txtReplace.Text, _chkMatchCase.Checked, _chkMatchWholeWord.Checked);

        _btnReplaceAll.Text = "Replace All";
        _btnReplaceAll.Location = new Point(285, 70);
        _btnReplaceAll.Enabled = false;
        _btnReplaceAll.Click += (s, e) => OnReplaceAll?.Invoke(_txtFind.Text, _txtReplace.Text, _chkMatchCase.Checked, _chkMatchWholeWord.Checked);

        _btnCancel.Text = "Cancel";
        _btnCancel.Click += (s, e) => Hide();

        _chkMatchCase.Text = "Match case";
        _chkMatchCase.AutoSize = true;

        _chkMatchWholeWord.Text = "Match whole word";
        _chkMatchWholeWord.AutoSize = true;

        PaintEventHandler customPaint = (s, e) =>
        {
            Button btn = (Button)s!;
            if (!btn.Enabled)
            {
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(bgBrush, e.ClipRectangle);
                }
                ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.DimGray, ButtonBorderStyle.Solid);
                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, e.ClipRectangle, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        };

        _btnFindNext.Paint += customPaint;
        _btnReplace.Paint += customPaint;
        _btnReplaceAll.Paint += customPaint;

        Controls.Add(_btnToggleReplace);
        Controls.Add(lblFind);
        Controls.Add(_txtFind);
        Controls.Add(_lblReplace);
        Controls.Add(_txtReplace);
        Controls.Add(_btnFindNext);
        Controls.Add(_btnReplace);
        Controls.Add(_btnReplaceAll);
        Controls.Add(_btnCancel);
        Controls.Add(_chkMatchCase);
        Controls.Add(_chkMatchWholeWord);

        AcceptButton = _btnFindNext;
        CancelButton = _btnCancel;

        FormClosing += (s, e) => 
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        };
    }

   public void SetMode(bool expand)
    {
        _isExpanded = expand;
        _btnToggleReplace.Text = _isExpanded ? "△" : "▽";
        Text = _isExpanded ? "Find and Replace" : "Find";
        
        _lblReplace.Visible = _isExpanded;
        _txtReplace.Visible = _isExpanded;
        _btnReplace.Visible = _isExpanded;
        _btnReplaceAll.Visible = _isExpanded;

        if (_isExpanded)
        {
            ClientSize = new Size(380, EXPANDED_HEIGHT);
            _btnCancel.Location = new Point(285, 100);
            
            // Stacked neatly on the left side
            _chkMatchCase.Location = new Point(115, 75);
            _chkMatchWholeWord.Location = new Point(115, 100); 
        }
        else
        {
            ClientSize = new Size(380, COLLAPSED_HEIGHT);
            _btnCancel.Location = new Point(285, 40);
            
            // Stacked neatly on the left side
            _chkMatchCase.Location = new Point(115, 45);
            _chkMatchWholeWord.Location = new Point(115, 70); 
        }
    }

   public void ApplyTheme(ThemeManager theme)
    {
        BackColor = theme.BackgroundColor;
        ForeColor = theme.TextColor;
        
        _txtFind.BackColor = theme.BackgroundColor;
        _txtFind.ForeColor = theme.TextColor;
        _txtFind.BorderStyle = BorderStyle.FixedSingle;

        _txtReplace.BackColor = theme.BackgroundColor;
        _txtReplace.ForeColor = theme.TextColor;
        _txtReplace.BorderStyle = BorderStyle.FixedSingle;

        _chkMatchCase.BackColor = theme.BackgroundColor;
        _chkMatchCase.ForeColor = theme.TextColor;
        _chkMatchWholeWord.BackColor = theme.BackgroundColor;
        _chkMatchWholeWord.ForeColor = theme.TextColor;

        // Apply standard borders to the main action buttons
        Button[] buttons = { _btnFindNext, _btnReplace, _btnReplaceAll, _btnCancel };
        foreach (var btn in buttons)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.DimGray;
            btn.BackColor = theme.BackgroundColor;
            btn.ForeColor = theme.TextColor;
        }

        // Treat the toggle button like a borderless icon
        _btnToggleReplace.FlatStyle = FlatStyle.Flat;
        _btnToggleReplace.FlatAppearance.BorderSize = 0; 
        _btnToggleReplace.BackColor = theme.BackgroundColor;
        _btnToggleReplace.ForeColor = theme.TextColor;
    }

    public void SetSearchTerm(string term)
    {
        if (!string.IsNullOrEmpty(term))
        {
            _txtFind.Text = term;
            _txtFind.SelectAll();
        }
        _txtFind.Focus();
    }
}