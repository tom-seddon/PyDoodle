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
        private ObjectOperations _operations;
        private List<Tweakable> _tweakables;

        public TweaksPanel()
            : this(null)
        {
        }

        public TweaksPanel(ScriptEngine se)
        {
            InitializeComponent();

            _tweakables = new List<Tweakable>();

            _operations = null;
            if (se != null)
                _operations = se.CreateOperations();

            _leftPanel.Height = 0;
            _rightPanel.Height = 0;
        }

        private class Tweakable
        {
            public int y = 0;
            public object Pyobj = null;
            public string Attr = null;
            public TweakControl.TweakControlCreator Creator = null;
            public TweakControl TweakControl = null;
            public Label label = null;
        }

        public void AddTweak(object pyobj, string attr, TweakControl.TweakControlCreator creator)
        {
            // don't add if it's already there.
            foreach (Tweakable ot in _tweakables)
            {
                if (object.ReferenceEquals(pyobj, ot.Pyobj) && attr == ot.Attr && creator == ot.Creator)
                    return;
            }

            // it isn't already there.
            Tweakable nt = new Tweakable();

            nt.Pyobj = pyobj;
            nt.Attr = attr;
            nt.Creator = creator;

            _tweakables.Add(nt);

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
                // Create controls if necessary
                if (t.label == null)
                {
                    t.label = new Label();
                    t.label.Text = t.Attr;

                    _leftPanel.Controls.Add(t.label);

                    t.label.Left = 0;
                    t.label.Width = t.label.Parent.Width;
                    t.label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                }

                if (t.TweakControl == null)
                {
                    t.TweakControl = t.Creator();

                    _rightPanel.Controls.Add(t.TweakControl);

                    t.TweakControl.Left = 0;
                    t.TweakControl.Width = t.TweakControl.Parent.Width;
                    t.TweakControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                    t.TweakControl.Tweak += this.HandleTweak;
                    t.TweakControl.Tag = t;
                }

                t.y = y;

                labelsW = Math.Max(labelsW, t.label.PreferredWidth);

                t.label.Top = t.y;
                t.label.Height = t.TweakControl.Height;
                t.label.BackColor = colours[colourIdx];

                t.TweakControl.Top = t.y;
                t.TweakControl.BackColor = colours[colourIdx];
                t.TweakControl.SetValue(_operations.GetMember(t.Pyobj, t.Attr));

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

        private void HandleTweak(object sender, TweakControl.EventArgs ea)
        {
            TweakControl tweakControl = sender as TweakControl;
            Tweakable t = tweakControl.Tag as Tweakable;

            _operations.SetMember(t.Pyobj, t.Attr, ea.Value);
        }
    }
}
