using Yotepad.Services;

namespace Yotepad.Dialogs;

public class PreviewWindow : Form
{
    private readonly RecoveryFile _file;
    private readonly ThemeManager _themeManager;
    private readonly TextBox _txtPreview = new();
    private readonly Button _btnClose = new();
    private readonly Button _btnRecover = new();
    private readonly Button _btnDelete = new();

    public event Action<RecoveryFile>? OnRecover;

    public event Action<RecoveryFile>? OnDelete;

    public PreviewWindow(RecoveryFile file, ThemeManager themeManager, Point spawnLocation)
    {
        _file = file;
        _themeManager = themeManager;
        InitializeComponent(spawnLocation);
        ApplyTheme();
    }

    private void InitializeComponent(Point spawnLocation)
    {
        string title = string.IsNullOrEmpty(_file.OriginalFilePath)
            ? "Preview — Untitled"
            : $"Preview — {System.IO.Path.GetFileName(_file.OriginalFilePath)}";

        Text = title;
        Size = new Size(600, 500);
        StartPosition = FormStartPosition.Manual;
        Location = spawnLocation;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(400, 300);
        ShowInTaskbar = false;
        TopMost = false;

        // Button strip at the top
        Panel btnPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36
        };

        _btnClose.Text = "Close";
        _btnClose.Size = new Size(80, 26);
        _btnClose.Location = new Point(8, 5);
        _btnClose.Click += (s, e) => Close();

        _btnRecover.Text = "Recover";
        _btnRecover.Size = new Size(80, 26);
        _btnRecover.Location = new Point(96, 5);
        _btnRecover.Click += (s, e) => OnRecover?.Invoke(_file);

        _btnDelete.Text = "Delete";
        _btnDelete.Size = new Size(80, 26);
        _btnDelete.Location = new Point(184, 5);
        _btnDelete.Click += (s, e) => OnDelete?.Invoke(_file);

        btnPanel.Controls.Add(_btnClose);
        btnPanel.Controls.Add(_btnRecover);
        btnPanel.Controls.Add(_btnDelete);

        _txtPreview.Multiline = true;
        _txtPreview.ReadOnly = true;
        _txtPreview.WordWrap = true;
        _txtPreview.ScrollBars = ScrollBars.Vertical;
        _txtPreview.Dock = DockStyle.Fill;
        _txtPreview.BorderStyle = BorderStyle.None;
        _txtPreview.Text = _file.Content;
        _txtPreview.Font = new Font("Consolas", 10f);

        Controls.Add(_txtPreview);
        Controls.Add(btnPanel);

        // Apply dark scrollbar after handle is created
        Load += (s, e) =>
        {
            int darkVal = _themeManager.IsDarkMode ? 1 : 0;
            NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkVal, sizeof(int));
            NativeMethods.SetWindowTheme(_txtPreview.Handle, _themeManager.IsDarkMode ? "DarkMode_Explorer" : "Explorer", null);
        };
    }

    public void ApplyTheme()
    {
        BackColor = _themeManager.BackgroundColor;
        ForeColor = _themeManager.TextColor;
        _txtPreview.BackColor = _themeManager.BackgroundColor;
        _txtPreview.ForeColor = _themeManager.TextColor;

        foreach (Control c in Controls)
        {
            if (c is Panel panel)
            {
                panel.BackColor = _themeManager.MenuBackgroundColor;
                foreach (Control pc in panel.Controls)
                {
                    if (pc is Button btn)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.DimGray;
                        btn.BackColor = _themeManager.BackgroundColor;
                        btn.ForeColor = _themeManager.TextColor;
                    }
                }
            }
        }
    }
}