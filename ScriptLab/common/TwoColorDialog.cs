using PaintDotNet;
using System.Windows.Forms;

namespace pyrochild.effects.common
{
    public partial class TwoColorDialog : Form
    {
        public TwoColorDialog()
        {
            InitializeComponent();
        }

        public ColorBgra Color1
        {
            get
            {
                return colorPanel1.Color;
            }
            set
            {
                colorPanel1.Color = value;
            }
        }

        public ColorBgra Color2
        {
            get
            {
                return colorPanel2.Color;
            }
            set
            {
                colorPanel2.Color = value;
            }
        }

        public void SetColors(ColorBgra color1, ColorBgra color2)
        {
            Color1 = color1;
            Color2 = color2;
        }
    }
}
