using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Microsoft.Scripting.Hosting;

namespace PyDoodle
{
    public partial class TweaksPanel : DockContent
    {
        private List<Tweakable> _tweakables;

        public TweaksPanel()
        {
            InitializeComponent();

            _tweakables = null;
        }

        public void Reset()
        {
            if (_tweakables != null)
            {
                foreach (Tweakable t in _tweakables)
                {
                    t.TweakControl.Dispose();
                    t.Label.Dispose();
                }
            }

            _tweakables = new List<Tweakable>();

            ArrangeControls();
        }

        private class Tweakable
        {
            public Label Label = null;
            public TweakControl TweakControl = null;
        }

        public void AddTweakControl(TweakControl tweakControl)
        {
            Tweakable t = new Tweakable();

            // Add label
            {
                t.Label = new Label();
                t.Label.Text = tweakControl.Attr.Name;

                _leftPanel.Controls.Add(t.Label);

                t.Label.Left = 0;
                t.Label.Width = t.Label.Parent.Width;
                t.Label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            }

            // Add tweak control
            {
                t.TweakControl = tweakControl;

                _rightPanel.Controls.Add(t.TweakControl);

                t.TweakControl.Left = 0;
                t.TweakControl.Width = t.TweakControl.Parent.Width;
                t.TweakControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                //t.TweakControl.Tweak += this.HandleTweak;
                t.TweakControl.Tag = t;
            }

            _tweakables.Add(t);

            ArrangeControls();
        }

        private void ArrangeControls()
        {
            // update Y coordinates and add labels appropriately
            int y = 0;
            int labelsW = 0;

            double h, s, v;
            Misc.ColorToHSV(Control.DefaultBackColor, out h, out s, out v);

            // "v-0.1" is highly unscientific
            Color[] colours = { Control.DefaultBackColor, Misc.ColorFromHSV(h, s, Math.Max(v - 0.1, 0.0)) };
            int colourIdx = 0;

            foreach (Tweakable t in _tweakables)
            {
                labelsW = Math.Max(labelsW, t.Label.PreferredWidth);

                t.Label.Top = y;
                t.Label.Height = t.TweakControl.Height;
                t.Label.BackColor = colours[colourIdx];
                t.Label.TextAlign = ContentAlignment.MiddleLeft;

                t.TweakControl.Top = y;
                t.TweakControl.BackColor = colours[colourIdx];
                //t.TweakControl.SetValue(t.Attr.GetValue());

                y += t.TweakControl.Height;
                colourIdx ^= 1;
            }

            // expand panels and splitter panel.
            _leftPanel.Height = y;
            _rightPanel.Height = y;
            _splitContainer.Height = y;

            // set split position
            _splitContainer.SplitterDistance = labelsW + 10;
        }

//         private void HandleTweak(object sender, TweakControl.EventArgs ea)
//         {
//             TweakControl tweakControl = sender as TweakControl;
//             Tweakable t = tweakControl.Tag as Tweakable;
// 
//             t.Attr.SetValue(ea.Value);
//         }
    }
}
