namespace YotePad.Helpers;

public class YotePadMenuRenderer : ToolStripProfessionalRenderer
{
    private readonly ThemeManager _theme = Program.Get<ThemeManager>();

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
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
        using var brush = new SolidBrush(_theme.MenuBackgroundColor);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var y = e.Item.Height / 2;
        using var pen = new Pen(Color.FromArgb(70, 70, 70));
        e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Enabled
            ? Color.FromArgb(220, 220, 220)
            : Color.FromArgb(110, 110, 110);
        base.OnRenderItemText(e);
    }
}