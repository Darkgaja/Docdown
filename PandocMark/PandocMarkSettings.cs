﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PandocMark
{
    /// <summary>
    /// Class used to configure the behavior of <see cref="PandocMarkConverter"/>.
    /// </summary>
    /// <remarks>This class is not thread-safe so any changes to a instance that is reused (for example, the 
    /// <see cref="Default"/>) has to be updated while it is not in use otherwise the
    /// behaviour is undefined.</remarks>
    public sealed class PandocMarkSettings
    {
        /// <summary>Initializes a new instance of the <see cref="PandocMarkSettings" /> class.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private PandocMarkSettings()
        { }

        /// <summary>
        /// Gets or sets the output format used by the last stage of conversion.
        /// </summary>
        public OutputFormat OutputFormat { get; set; }

        private Action<Syntax.Block, System.IO.TextWriter, PandocMarkSettings> _outputDelegate;
        /// <summary>
        /// Gets or sets the custom output delegate function used for formatting PandocMark output.
        /// Setting this to a non-null value will also set <see cref="OutputFormat"/> to <see cref="OutputFormat.CustomDelegate"/>.
        /// </summary>
        public Action<Syntax.Block, System.IO.TextWriter, PandocMarkSettings> OutputDelegate
        {
            get { return _outputDelegate; }
            set
            {
                if (_outputDelegate != value)
                {
                    _outputDelegate = value;
                    OutputFormat = value == null ? default(OutputFormat) : OutputFormat.CustomDelegate;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether soft line breaks should be rendered as hard line breaks.
        /// </summary>
        public bool RenderSoftLineBreaksAsLineBreaks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parser tracks precise positions in the source data for
        /// block and inline elements. This is disabled by default because it incurs an additional performance cost to
        /// keep track of the original position.
        /// Setting this to <see langword="true"/> will populate <see cref="Syntax.Inline.SourcePosition"/>, 
        /// <see cref="Syntax.Inline.SourceLength"/>, <see cref="Syntax.Block.SourcePosition"/> and 
        /// <see cref="Syntax.Block.SourceLength"/> properties with correct information, otherwise the values
        /// of these properties are undefined.
        /// This also controls if these values will be written to the output.
        /// </summary>
        public bool TrackSourcePosition { get; set; }

        private PandocMarkAdditionalFeatures _additionalFeatures;

        /// <summary>
        /// Gets or sets any additional features (that are not present in the current PandocMark specification) that
        /// the parser and/or formatter will recognize.
        /// </summary>
        public PandocMarkAdditionalFeatures AdditionalFeatures
        {
            get { return _additionalFeatures; }
            set { _additionalFeatures = value; _inlineParsers = null; _inlineParserSpecialCharacters = null; }
        }

        private Func<string, string> _uriResolver;
        /// <summary>
        /// Gets or sets the delegate that is used to resolve addresses during rendering process. Can be used to process application relative URLs (<c>~/foo/bar</c>).
        /// </summary>
        /// <example><code>PandocMarkSettings.Default.UriResolver = VirtualPathUtility.ToAbsolute;</code></example>
        public Func<string, string> UriResolver
        {
            get => _uriResolver;
            set
            {
                if (value != null)
                {
                    var orig = value;
                    value = x =>
                    {
                        try
                        {
                            return orig(x);
                        }
                        catch (Exception ex)
                        {
                            throw new PandocMarkException("An error occurred while executing the PandocMarkSettings.UriResolver delegate. View inner exception for details.", ex);
                        }
                    };
                }

                this._uriResolver = value;
            }
        }

        /// <summary>
        /// The default settings for the converter. If the properties of this instance are modified, the changes will be applied to all
        /// future conversions that do not specify their own settings.
        /// </summary>
        public static PandocMarkSettings Default => _default.Clone();

        /// <summary>
        /// Creates a copy of this configuration object.
        /// </summary>
        public PandocMarkSettings Clone()
        {
            return (PandocMarkSettings)MemberwiseClone();
        }

        private static readonly PandocMarkSettings _default = new PandocMarkSettings();

        #region [ Properties that cache structures used in the parsers ]

        private Func<Parser.Subject, Syntax.Inline>[] _inlineParsers;

        /// <summary>
        /// Gets the delegates that parse inline elements according to these settings.
        /// </summary>
        internal Func<Parser.Subject, Syntax.Inline>[] InlineParsers
        {
            get
            {
                var p = this._inlineParsers;
                if (p == null)
                {
                    p = Parser.InlineMethods.InitializeParsers(this);
                    this._inlineParsers = p;
                }

                return p;
            }
        }

        private char[] _inlineParserSpecialCharacters;

        /// <summary>
        /// Gets the characters that have special meaning for inline element parsers according to these settings.
        /// </summary>
        internal char[] InlineParserSpecialCharacters
        {
            get
            {
                var v = this._inlineParserSpecialCharacters;
                if (v == null)
                {
                    var p = this.InlineParsers;
                    var vs = new List<char>(20);
                    for (var i = 0; i < p.Length; i++)
                        if (p[i] != null)
                            vs.Add((char)i);

                    v = this._inlineParserSpecialCharacters = vs.ToArray();
                }

                return v;
            }
        }

        #endregion
    }
}
