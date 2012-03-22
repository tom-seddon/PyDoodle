using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;

namespace PyDoodle
{
    public partial class TextPanel : DockContent
    {
        public TextPanel()
        {
            InitializeComponent();
        }

        public void ClearText()
        {
            _textBox.Clear();
        }

        public void AppendText(string text)
        {
            _textBox.AppendText(text);
        }

        public class WriteStream : Stream
        {
            public override bool CanRead { get { return false; } }
            public override bool CanWrite { get { return true; } }
            public override bool CanSeek { get { return false; } }

            public override long Length { get { throw new NotImplementedException(); } }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException("Can't read from a TextBoxWriteStream");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_textPanel != null)
                    _textPanel.AppendText(Encoding.UTF8.GetString(buffer, offset, count));
            }

            private TextPanel _textPanel;

            public TextPanel TextBox
            {
                get { return _textPanel; }
                set { _textPanel = value; }
            }

            public WriteStream(TextPanel textPanel = null)
            {
                _textPanel = textPanel;
            }
        }

    }
}
