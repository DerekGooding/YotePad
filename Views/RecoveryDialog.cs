// RecoveryDialog.cs
using YotePad.Helpers;

namespace YotePad.Views;

public class RecoveryDialog : Form
{
    private readonly RecoveryFile[] _files;
    private readonly ThemeManager _themeManager;
    private readonly List<RecoveryRowControl> _rows = [];
    private readonly Panel _rowPanel = new();
    private readonly Button _btnRestoreSelected = new();
    private readonly Button _btnDiscardAll = new();

    // Returns the files the user chose to restore
    public List<RecoveryFile> FilesToRestore { get; } = [];

    public RecoveryDialog(RecoveryFile[] files, ThemeManager themeManager)
    {
        _files = files;
        _themeManager = themeManager;
        InitializeComponent();
        ApplyTheme();
        PopulateRows();
    }

    private void InitializeComponent()
    {
        Text = "YotePad — Session Recovery";
        Size = new Size(560, 420);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;

        // Header label
        var lblHeader = new Label
        {
            Text = "YotePad found unsaved files from a previous session.",
            Location = new Point(12, 12),
            Size = new Size(520, 20),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
        };

        var lblSub = new Label
        {
            Text = "Select the files you want to restore:",
            Location = new Point(12, 34),
            Size = new Size(520, 18),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
        };

        // Scrollable row panel
        _rowPanel.Location = new Point(12, 58);
        _rowPanel.Size = new Size(520, 260);
        _rowPanel.AutoScroll = true;
        _rowPanel.BorderStyle = BorderStyle.FixedSingle;

        // Bottom buttons
        _btnRestoreSelected.Text = "Apply";
        _btnRestoreSelected.Size = new Size(90, 28);
        _btnRestoreSelected.Location = new Point(330, 340);
        _btnRestoreSelected.Click += BtnRestoreSelected_Click;

        _btnDiscardAll.Text = "Discard All";
        _btnDiscardAll.Size = new Size(100, 28);
        _btnDiscardAll.Location = new Point(432, 340);
        _btnDiscardAll.Click += BtnDiscardAll_Click;

        Controls.Add(lblHeader);
        Controls.Add(lblSub);
        Controls.Add(_rowPanel);
        Controls.Add(_btnRestoreSelected);
        Controls.Add(_btnDiscardAll);
    }

    private void PopulateRows()
    {
        var y = 4;
        foreach (var file in _files)
        {
            var row = new RecoveryRowControl(file, _themeManager);
            row.Location = new Point(4, y);
            row.Width = _rowPanel.Width - 24;
            row.OnPreview += ShowPreview;
            _rows.Add(row);
            _rowPanel.Controls.Add(row);
            y += row.Height + 4;
        }
    }

    private void ReflowRows()
    {
        var y = 4;
        foreach (var row in _rows)
        {
            row.Location = new Point(4, y);
            y += row.Height + 4;
        }
    }

    private void ShowPreview(RecoveryFile file)
    {
        // Spawn preview to the right of the recovery dialog
        var spawnLocation = new Point(
            Location.X + Width + 10,
            Location.Y
        );

        var preview = new PreviewWindow(file, _themeManager, spawnLocation);
        preview.Owner = this;

        preview.OnRecover += (f) =>
        {
            // Launch a new instance directly from preview — no routing through FilesToRestore
            RecoveryLauncher.Launch(f, Location, 0);

            var row = _rows.Find(r => r.RecoveryFile == f);
            if (row != null)
            {
                _rowPanel.Controls.Remove(row);
                _rows.Remove(row);
                ReflowRows();
            }
            preview.Close();

            // Close recovery dialog if this was the last file
            if (_rows.Count == 0) DialogResult = DialogResult.OK;
        };

        preview.OnDelete += (f) =>
        {
            RecoveryService.DeleteRecoveryFileAt(f.RecoveryFilePath);
            var row = _rows.Find(r => r.RecoveryFile == f);
            if (row != null)
            {
                _rowPanel.Controls.Remove(row);
                _rows.Remove(row);
                ReflowRows();
            }
            preview.Close();
            if (_rows.Count == 0) DialogResult = DialogResult.Cancel;
        };

        preview.Show(); // Non-blocking — floats alongside recovery dialog
    }

    private void BtnRestoreSelected_Click(object? sender, EventArgs e)
    {
        foreach (var row in _rows)
        {
            if (row.IsChecked)
                FilesToRestore.Add(row.RecoveryFile);
            else
                RecoveryService.DeleteRecoveryFileAt(row.RecoveryFile.RecoveryFilePath);
        }
        DialogResult = DialogResult.OK;
    }

    private void BtnDiscardAll_Click(object? sender, EventArgs e)
    {
        foreach (var row in _rows)
            RecoveryService.DeleteRecoveryFileAt(row.RecoveryFile.RecoveryFilePath);
        DialogResult = DialogResult.Cancel;
    }

    public void ApplyTheme()
    {
        BackColor = _themeManager.BackgroundColor;
        ForeColor = _themeManager.TextColor;
        _rowPanel.BackColor = _themeManager.MenuBackgroundColor;

        foreach (var btn in (Button[])[_btnRestoreSelected, _btnDiscardAll])
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.DimGray;
            btn.BackColor = _themeManager.BackgroundColor;
            btn.ForeColor = _themeManager.TextColor;
        }

        foreach (Control c in Controls)
        {
            if (c is Label lbl)
            {
                lbl.BackColor = _themeManager.BackgroundColor;
                lbl.ForeColor = _themeManager.TextColor;
            }
        }

        foreach (var row in _rows)
            row.ApplyTheme();
    }
}

// A single row in the recovery list
public class RecoveryRowControl : Panel
{
    public RecoveryFile RecoveryFile { get; }
    public bool IsChecked => _chk.Checked;

    public event Action<RecoveryFile>? OnPreview;

    private readonly CheckBox _chk = new();
    private readonly Label _lblName = new();
    private readonly Button _btnPreview = new();
    private readonly ThemeManager _themeManager;

    public RecoveryRowControl(RecoveryFile file, ThemeManager themeManager)
    {
        RecoveryFile = file;
        _themeManager = themeManager;
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