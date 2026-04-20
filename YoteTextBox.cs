namespace Yotepad;

public class YoteTextBox : TextBox
{
    private const int WM_PASTE = 0x0302;
    
    public bool IsOverwriteMode { get; private set; } = false;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_PASTE && Clipboard.ContainsText())
        {
            string text = Clipboard.GetText();
            text = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
            SelectedText = text;
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Insert)
        {
            IsOverwriteMode = !IsOverwriteMode;
            UpdateCaretAppearance();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (IsOverwriteMode && 
            SelectionLength == 0 && 
            SelectionStart < TextLength && 
            !char.IsControl(e.KeyChar))
        {
            char nextChar = Text[SelectionStart];
            
            if (nextChar != '\r' && nextChar != '\n')
            {
                SelectionLength = 1;
            }
        }
        base.OnKeyPress(e);
    }

    // The OS constantly tries to reset the caret to a line. 
    // We must reassert our block caret after these events.
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        UpdateCaretAppearance();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        UpdateCaretAppearance();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        UpdateCaretAppearance();
    }

    private void UpdateCaretAppearance()
    {
        if (IsOverwriteMode)
        {
            // Measure roughly how wide a character is in the current font
            int width = TextRenderer.MeasureText("W", Font).Width / 2;
            int height = Font.Height;
            
            // Passing IntPtr.Zero creates a solid black/white inverted block
            NativeMethods.CreateCaret(Handle, IntPtr.Zero, width, height);
            NativeMethods.ShowCaret(Handle);
        }
        else
        {
            // Revert to a standard 1-pixel wide line caret
            NativeMethods.CreateCaret(Handle, IntPtr.Zero, 1, Font.Height);
            NativeMethods.ShowCaret(Handle);
        }
    }
}