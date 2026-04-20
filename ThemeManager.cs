using Microsoft.Win32;

namespace Yotepad;

public class ThemeManager
{
    public bool IsDarkMode { get; private set; }

    public Color BackgroundColor => IsDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
    public Color TextColor => IsDarkMode ? Color.FromArgb(220, 220, 220) : Color.Black;
    public Color MenuBackgroundColor => IsDarkMode ? Color.FromArgb(45, 45, 45) : SystemColors.Control;

    public void InitializeTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int lightTheme)
            {
                IsDarkMode = (lightTheme == 0);
            }
        }
        catch { IsDarkMode = true; }
    }

    public void ToggleTheme() => IsDarkMode = !IsDarkMode;

    public void ApplyTheme(Form form, TextBox textBox, MenuStrip menu, StatusStrip status)
    {
        // Call the undocumented API: 2 = ForceDark, 0 = Default Light
        try { NativeMethods.SetPreferredAppMode(IsDarkMode ? 2 : 0); } catch { }

        var darkVal = IsDarkMode ? 1 : 0;
        NativeMethods.DwmSetWindowAttribute(form.Handle, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkVal, sizeof(int));

        NativeMethods.SetWindowTheme(textBox.Handle, IsDarkMode ? "DarkMode_Explorer" : "Explorer", null);

        form.BackColor = BackgroundColor;
        textBox.BackColor = BackgroundColor;
        textBox.ForeColor = TextColor;

        menu.Renderer = IsDarkMode ? new YotePadMenuRenderer(this) : new ToolStripProfessionalRenderer();
        menu.BackColor = MenuBackgroundColor;
        menu.ForeColor = TextColor;

        status.BackColor = MenuBackgroundColor;
        status.ForeColor = TextColor;

        foreach (ToolStripMenuItem item in menu.Items)
        {
            ApplyMenuTheme(item);
        }
    }

    private void ApplyMenuTheme(ToolStripMenuItem item)
    {
        item.ForeColor = TextColor;
        if (item.DropDown is ToolStripDropDownMenu dropDown)
        {
            dropDown.ShowImageMargin = false;
            dropDown.BackColor = MenuBackgroundColor;
            dropDown.ForeColor = TextColor;
        }

        foreach (ToolStripItem subItem in item.DropDownItems)
        {
            if (subItem is ToolStripMenuItem subMenu)
            {
                subMenu.BackColor = MenuBackgroundColor;
                ApplyMenuTheme(subMenu);
            }
        }
    }
}

public class YotePadMenuRenderer(ThemeManager theme) : ToolStripProfessionalRenderer()
{
    private readonly ThemeManager _theme = theme;

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!_theme.IsDarkMode)
        {
            base.OnRenderMenuItemBackground(e);
            return;
        }

        var rect = new Rectangle(Point.Empty, e.Item.Size);

        if (e.Item.Selected || e.Item.Pressed)
        {
            using var brush = new SolidBrush(Color.FromArgb(80, 80, 80));
            e.Graphics.FillRectangle(brush, rect);
        }
        else
        {
            using var brush = new SolidBrush(_theme.MenuBackgroundColor);
            e.Graphics.FillRectangle(brush, rect);
        }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        if (!_theme.IsDarkMode)
        {
            base.OnRenderToolStripBackground(e);
            return;
        }

        using var brush = new SolidBrush(_theme.MenuBackgroundColor);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        if (!_theme.IsDarkMode)
        {
            base.OnRenderSeparator(e);
            return;
        }

        var y = e.Item.Height / 2;
        using var pen = new Pen(Color.FromArgb(70, 70, 70));
        e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (!_theme.IsDarkMode)
        {
            base.OnRenderItemText(e);
            return;
        }

        e.TextColor = e.Item.Enabled
            ? Color.FromArgb(220, 220, 220)
            : Color.FromArgb(110, 110, 110);
        base.OnRenderItemText(e);
    }
}