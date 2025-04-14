using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionallyDesignable
{
    class ImmutableContent : RichTextBox
    {
        public ImmutableContent()
        {
            Dock = DockStyle.Fill;
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            BackColor = SystemColors.Control;
            TabStop = false;
            DetectUrls = false;
            ScrollBars = RichTextBoxScrollBars.None;
            Margin = new Padding(0);
            Padding = new Padding(0);
            SelectionAlignment = HorizontalAlignment.Center;
            Text = "";
            AppendText(Environment.NewLine);
            AppendTextStyled("Immutable Surface\n", new Font(FontFamily.GenericSansSerif, 14f, FontStyle.Bold), Color.FromArgb(160, 0, 0));
            AppendText(Environment.NewLine);
            AppendTextStyled("This is not a designable surface in the\nWindows Forms Designer.", new Font(FontFamily.GenericSansSerif, 11.5f), Color.FromArgb(51, 51, 51));
        }

        void AppendTextStyled(string text, Font font, Color color)
        {
            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionFont = font;
            SelectionColor = color;
            SelectionAlignment = HorizontalAlignment.Center;
            AppendText(text);
        }
    }
}
