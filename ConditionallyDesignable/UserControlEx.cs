using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using Microsoft.DotNet.DesignTools.Designers;
using System.IO;
using System.Collections;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using System.Drawing.Design;

namespace ConditionallyDesignable
{
    [Designer(typeof(ParentControlDesignerEx))]
    public partial class UserControlEx : UserControl
    {
        public UserControlEx()
        {
            BackColor = ColorTranslator.FromHtml("#444444");
            InitializeComponent();
            Controls.Add(new ImmutableContent
            {
                Name = nameof(ImmutableContent),
                Dock = DockStyle.Fill,
            });
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            OnPropertyChanged(nameof(ContentType));
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Design-time content type preset.")]
        public ContentType ContentType
        {
            get => _contentType;
            set
            {
                if (!Equals(_contentType, value))
                {
                    _contentType = value;
                    OnPropertyChanged();
                }
            }
        }
        ContentType _contentType = default;

        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design", typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Design-time logging path.")]
        public string? LogFilePath
        {
            get => _logFilePath;
            set
            {
                if (!Equals(_logFilePath, value))
                {
                    _logFilePath = value;
                    if (_memoryLog.Any() && !string.IsNullOrWhiteSpace(_logFilePath))
                    {
                        var msgInfo = _memoryLog.Dequeue();
                        Log(msgInfo.msg, append: false);
                        while (_memoryLog.Count > 0)
                        {
                            msgInfo = _memoryLog.Dequeue();
                            Log(msgInfo.msg, msgInfo.append);
                        }
                    }                       
                }
            }
        }

        string? _logFilePath = null;

        internal void Log(string msg, bool append = true)
        {
            if (DesignMode)
            {
                msg =
                    msg.EndsWith(Environment.NewLine)
                    ? msg
                    : $"{msg}{Environment.NewLine}";

                bool success = false;
                if (!string.IsNullOrWhiteSpace(LogFilePath))
                {
                    if (append && File.Exists(LogFilePath))
                    {
                        File.AppendAllText(LogFilePath, msg);
                        success = true;
                    }
                    else
                    {
                        File.WriteAllText(LogFilePath, msg);
                        success = true;
                    }
                }
                if (!success)
                {
                    _memoryLog.Enqueue((msg: msg, append: append));
                }
            }
        }
        private readonly Queue<(string msg, bool append)> _memoryLog = new();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            switch (propertyName)
            {
                case nameof(ContentType):
                    if (Controls[nameof(ImmutableContent)] is { } immutable)
                    {
                        immutable.Visible = ContentType == ContentType.Immutable;
                        Controls.SetChildIndex(immutable, 0);
                    }
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
