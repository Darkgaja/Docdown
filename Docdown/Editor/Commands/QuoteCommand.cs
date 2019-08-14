﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor.Commands
{
    internal class QuoteCommand : SorroundCommand
    {
        public QuoteCommand() : base(">", "")
        {
        }
    }
}
