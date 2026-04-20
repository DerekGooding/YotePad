namespace YotePad.Model;

public class RecoveryRowControl : Panel
{
    public RecoveryFile RecoveryFile { get; }
    public bool IsChecked => _chk.Checked;

    public event Action<RecoveryFile>? OnPreview;

    private readonly CheckBox _chk = new();
    private readonly Label _lblName = new();
    private readonly Button _btnPreview = new();
    private readonly ThemeManager _themeManager = Program.Get<ThemeManager>();

    public RecoveryRowControl(RecoveryFile file, Point location, int width)
    {
        Location = location;
        Width = width;
        RecoveryFile = file;
        Height = 32;
        Padding = new Padding(0);

        _chk.Checked = true;
        _chk.Size = new Size(20, 20);
        _chk.Location = new Point(6, 6);

        _lblName.Text = file.DisplayName;
        _lblName.Location = new Point(30, 8);
        _lblName.Size = new Size(360, 18);
        _lblName.AutoEllipsis = true;

        // Tooltip for full filename on hover
        var tooltip = new ToolTip();
        tooltip.SetToolTip(_lblName, file.DisplayName);

        _btnPreview.Text = "Preview";
        _btnPreview.Size = new Size(70, 24);
        _btnPreview.Location = new Point(396, 4);
        _btnPreview.Click += (s, e) => OnPreview?.Invoke(RecoveryFile);

        Controls.Add(_chk);
        Controls.Add(_lblName);
        Controls.Add(_btnPreview);

        ApplyTheme();
    }

    public void ApplyTheme()
    {
        BackColor = _themeManager.MenuBackgroundColor;
        ForeColor = _themeManager.TextColor;
        _lblName.BackColor = _themeManager.MenuBackgroundColor;
        _lblName.ForeColor = _themeManager.TextColor;
        _chk.BackColor = _themeManager.MenuBackgroundColor;
        _chk.ForeColor = _themeManager.TextColor;
        _btnPreview.FlatStyle = FlatStyle.Flat;
        _btnPreview.FlatAppearance.BorderColor = Color.DimGray;
        _btnPreview.BackColor = _themeManager.BackgroundColor;
        _btnPreview.ForeColor = _themeManager.TextColor;
    }
}