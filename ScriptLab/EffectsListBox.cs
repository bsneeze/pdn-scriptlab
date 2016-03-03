using System;
using System.Drawing;
using System.Windows.Forms;

namespace pyrochild.effects.scriptlab
{
    public class EffectsListBox : ListBox
    {
        public EffectsListBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                Rectangle r = GetItemRectangle(i);
                if (e.ClipRectangle.IntersectsWith(r))
                {
                    if (SelectedIndices.Contains(i))
                    {
                        OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, r, i, DrawItemState.Selected, ForeColor, BackColor));
                    }
                    else
                    {
                        OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, r, i, DrawItemState.Default, ForeColor, BackColor));
                    }
                }
            }
            base.OnPaint(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            Invalidate();

            base.OnSelectedIndexChanged(e);
        }
    }
}